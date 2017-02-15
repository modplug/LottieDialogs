using System;
using System.Collections.Generic;
using System.IO;
using Airbnb.Lottie;
using Cirrious.FluentLayouts.Touch;
using CoreGraphics;
using Foundation;
using LottieDialogs.Abstractions;
using ObjCRuntime;
using UIKit;

namespace LottieDialogs.iOS
{
    public class LottieDialogView : UIView
    {
        private static Class clsUIPeripheralHostView;
        private static Class clsUIKeyboard;
        private static Class clsUIInputSetContainerView;
        private static Class clsUIInputSetHostView;
        private LAAnimationView _lottieAnimationView;
        private MaskType _maskType;
        private ToastPosition _toastPosition;
        private UILabel _descriptionLabel;
        private UIButton _dismissDialogButton;
        private List<NSObject> _eventListeners;
        private UIView _hudView;
        private UIView _overlayView;

        public UIColor HudBackgroundColour = UIColor.FromWhiteAlpha(0.0f, 0.8f);
        public UIFont HudFont = UIFont.BoldSystemFontOfSize(16f);
        public UIColor HudForegroundColor = UIColor.White;
        public UIColor HudStatusShadowColor = UIColor.Black;
        public UITextAlignment HudTextAlignment = UITextAlignment.Center;
        public UIColor HudToastBackgroundColor = UIColor.Clear;

        public static LottieDialogView Shared => _dialog ?? (_dialog = new LottieDialogView());
        private static LottieDialogView _dialog;

        public void ShowLottieDialog(MaskType maskType, Stream stream, bool isIndeterminate = true,
            ToastPosition toastPosition = ToastPosition.Bottom, string dismissDescription = null,
            Action dimissCallback = null)
        {
            IsIndeterminate = isIndeterminate;
            _maskType = maskType;
            _toastPosition = toastPosition;

            //initialize static fields used for input view detection
            var ptrUIPeripheralHostView = Class.GetHandle("UIPeripheralHostView");
            if (ptrUIPeripheralHostView != IntPtr.Zero)
                clsUIPeripheralHostView = new Class(ptrUIPeripheralHostView);
            var ptrUIKeyboard = Class.GetHandle("UIKeyboard");
            if (ptrUIKeyboard != IntPtr.Zero)
                clsUIKeyboard = new Class(ptrUIKeyboard);
            var ptrUIInputSetContainerView = Class.GetHandle("UIInputSetContainerView");
            if (ptrUIInputSetContainerView != IntPtr.Zero)
                clsUIInputSetContainerView = new Class(ptrUIInputSetContainerView);
            var ptrUIInputSetHostView = Class.GetHandle("UIInputSetHostView");
            if (ptrUIInputSetHostView != IntPtr.Zero)
                clsUIInputSetHostView = new Class(ptrUIInputSetHostView);

            var json = new StreamReader(stream).ReadToEnd();
            _lottieAnimationView = new LAAnimationView(NSUrl.FromString(json));
            if (isIndeterminate)
            {
                _lottieAnimationView.LoopAnimation = true;
                _lottieAnimationView.Play();
            }
            else
            {
                _lottieAnimationView.LoopAnimation = false;
            }

            var windows = UIApplication.SharedApplication.Windows;
            Array.Reverse(windows);
            foreach (var window in windows)
                if (window.WindowLevel == UIWindowLevel.Normal && !window.Hidden)
                {
                    window.AddSubview(OverlayView);
                    break;
                }

            SetupView();
            OverlayView.AddSubview(this);

            if (!string.IsNullOrEmpty(dismissDescription))
            {
                DismissDialogButtonButton.SetTitle(dismissDescription, UIControlState.Normal);
                DismissDialogButtonButton.TouchUpInside += (sender, args) =>
                {
                    DismissDialog();
                    if (dimissCallback != null)
                    {
                        InvokeOnMainThread(() => dimissCallback.DynamicInvoke(null));
                    }
                };
            }
            RegisterNotifications();
        }

        private void DismissDialog()
        {
            _lottieAnimationView.RemoveFromSuperview();
            _lottieAnimationView.Pause();
            HudView.RemoveFromSuperview();
            OverlayView.RemoveFromSuperview();
            UnRegisterNotifications();
            IsVisible = false;
        }

        public bool IsVisible { get; private set; }

        public bool IsIndeterminate { get; set; }

        private UILabel DescriptionLabel
        {
            get
            {
                if (_descriptionLabel == null)
                    _descriptionLabel = new UILabel
                    {
                        BackgroundColor = HudToastBackgroundColor,
                        AdjustsFontSizeToFitWidth = true,
                        TextAlignment = HudTextAlignment,
                        BaselineAdjustment = UIBaselineAdjustment.AlignCenters,
                        TextColor = HudForegroundColor,
                        Font = HudFont,
                        Lines = 0
                    };
                if (_descriptionLabel.Superview == null)
                    HudView.AddSubview(_descriptionLabel);
                return _descriptionLabel;
            }
        }

        private UIButton DismissDialogButtonButton
        {
            get
            {
                if (_dismissDialogButton == null)
                {
                    _dismissDialogButton = new UIButton {BackgroundColor = UIColor.Clear};

                    _dismissDialogButton.SetTitleColor(HudForegroundColor, UIControlState.Normal);
                    _dismissDialogButton.UserInteractionEnabled = true;
                    _dismissDialogButton.Font = HudFont;
                    UserInteractionEnabled = true;
                }
                if (_dismissDialogButton.Superview == null)
                    HudView.AddSubview(_dismissDialogButton);
                return _dismissDialogButton;
            }
        }

        private UIView OverlayView => _overlayView ?? (_overlayView = new UIView(UIScreen.MainScreen.Bounds)
        {
            AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
            BackgroundColor = UIColor.Clear,
            UserInteractionEnabled = false
        });

        private float VisibleKeyboardHeight
        {
            get
            {
                foreach (var testWindow in UIApplication.SharedApplication.Windows)
                    if (testWindow.Class.Handle != Class.GetHandle("UIWindow"))
                        foreach (var possibleKeyboard in testWindow.Subviews)
                            if (clsUIPeripheralHostView != null &&
                                possibleKeyboard.IsKindOfClass(clsUIPeripheralHostView) ||
                                clsUIKeyboard != null && possibleKeyboard.IsKindOfClass(clsUIKeyboard))
                                return (float) possibleKeyboard.Bounds.Size.Height;
                            else if (clsUIInputSetContainerView != null &&
                                     possibleKeyboard.IsKindOfClass(clsUIInputSetContainerView))
                                foreach (var possibleKeyboardSubview in possibleKeyboard.Subviews)
                                    if (clsUIInputSetHostView != null &&
                                        possibleKeyboardSubview.IsKindOfClass(clsUIInputSetHostView))
                                        return (float) possibleKeyboardSubview.Bounds.Size.Height;

                return 0;
            }
        }

        private UIView HudView
        {
            get
            {
                if (_hudView == null)
                {
                    _hudView = new UIToolbar();
                    ((UIToolbar) _hudView).Translucent = true;
                    ((UIToolbar) _hudView).BarTintColor = HudBackgroundColour;

                    _hudView.Layer.CornerRadius = 10;
                    _hudView.Layer.MasksToBounds = true;
                    _hudView.BackgroundColor = HudBackgroundColour;
                    _hudView.AutoresizingMask = UIViewAutoresizing.FlexibleBottomMargin |
                                                UIViewAutoresizing.FlexibleTopMargin |
                                                UIViewAutoresizing.FlexibleRightMargin |
                                                UIViewAutoresizing.FlexibleLeftMargin;

                    AddSubview(_hudView);
                }
                return _hudView;
            }
            set { _hudView = value; }
        }

        private void SetupView()
        {
            Add(HudView);
            HudView.Add(DescriptionLabel);
            HudView.Add(_lottieAnimationView);
            HudView.Add(_dismissDialogButton);

            var constraints = HudView.VerticalStackPanelConstraints(new Margins()
            {
                Bottom = 5,
                HSpacing = 5,
                Left = 5,
                Right = 5,
                Top = 5,
                VSpacing = 5
            }, DescriptionLabel, _lottieAnimationView, DismissDialogButtonButton);

            HudView.AddConstraints(constraints);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (IsVisible)
                {
                    DismissDialog();
                }
                _lottieAnimationView.Dispose();
                HudView.Dispose();
                OverlayView.Dispose();
            }
            base.Dispose(disposing);
        }

        public void UpdateProgress(int progress)
        {
            if (IsIndeterminate)
                throw new ArgumentOutOfRangeException(nameof(progress),
                    "Can't set progress on an indeterminate progress");
            _lottieAnimationView.AnimationProgress = progress;
        }

        public override void Draw(CGRect rect)
        {
            using (var context = UIGraphics.GetCurrentContext())
            {
                switch (_maskType)
                {
                    case MaskType.Black:
                        UIColor.FromWhiteAlpha(0f, 0.5f).SetColor();
                        context.FillRect(Bounds);
                        break;
                    case MaskType.Gradient:
                        nfloat[] colors = {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.75f};
                        nfloat[] locations = {0.0f, 1.0f};
                        using (var colorSpace = CGColorSpace.CreateDeviceRGB())
                        {
                            using (var gradient = new CGGradient(colorSpace, colors, locations))
                            {
                                var center = new CGPoint(Bounds.Size.Width / 2, Bounds.Size.Height / 2);
                                var radius = Math.Min((float) Bounds.Size.Width, (float) Bounds.Size.Height);
                                context.DrawRadialGradient(gradient, center, 0, center, radius,
                                    CGGradientDrawingOptions.DrawsAfterEndLocation);
                            }
                        }
                        break;
                }
            }
        }

        private void RegisterNotifications()
        {
            if (_eventListeners == null)
                _eventListeners = new List<NSObject>();
            _eventListeners.Add(
                NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.DidChangeStatusBarOrientationNotification,
                    PositionHUD));
            _eventListeners.Add(NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification,
                PositionHUD));
            _eventListeners.Add(NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidHideNotification,
                PositionHUD));
            _eventListeners.Add(NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification,
                PositionHUD));
            _eventListeners.Add(NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidShowNotification,
                PositionHUD));
        }

        public bool IsLandscape(UIInterfaceOrientation orientation)
        {
            return orientation == UIInterfaceOrientation.LandscapeLeft ||
                   orientation == UIInterfaceOrientation.LandscapeRight;
        }

        public bool IsPortrait(UIInterfaceOrientation orientation)
        {
            return orientation == UIInterfaceOrientation.Portrait ||
                   orientation == UIInterfaceOrientation.PortraitUpsideDown;
        }

        private void PositionHUD(NSNotification notification)
        {
            nfloat keyboardHeight = 0;
            double animationDuration = 0;

            Frame = UIScreen.MainScreen.Bounds;

            var orientation = UIApplication.SharedApplication.StatusBarOrientation;
            var ignoreOrientation = UIDevice.CurrentDevice.CheckSystemVersion(8, 0);

            if (notification != null)
            {
                var keyboardFrame = UIKeyboard.FrameEndFromNotification(notification);
                animationDuration = UIKeyboard.AnimationDurationFromNotification(notification);

                if (notification.Name == UIKeyboard.WillShowNotification ||
                    notification.Name == UIKeyboard.DidShowNotification)
                    if (ignoreOrientation || IsPortrait(orientation))
                        keyboardHeight = keyboardFrame.Size.Height;
                    else
                        keyboardHeight = keyboardFrame.Size.Width;
                else
                    keyboardHeight = 0;
            }
            else
            {
                keyboardHeight = VisibleKeyboardHeight;
            }

            var orientationFrame = UIApplication.SharedApplication.KeyWindow.Bounds;

            var statusBarFrame = UIApplication.SharedApplication.StatusBarFrame;

            if (!ignoreOrientation && IsLandscape(orientation))
            {
                orientationFrame.Size = new CGSize(orientationFrame.Size.Height, orientationFrame.Size.Width);
                statusBarFrame.Size = new CGSize(statusBarFrame.Size.Height, statusBarFrame.Size.Width);
            }

            var activeHeight = orientationFrame.Size.Height;

            if (keyboardHeight > 0)
                activeHeight += statusBarFrame.Size.Height * 2;

            activeHeight -= keyboardHeight;
            nfloat posY = (float) Math.Floor(activeHeight * 0.45);
            var posX = orientationFrame.Size.Width / 2;
            var textHeight = DescriptionLabel.Frame.Height / 2 + 40;

            switch (_toastPosition)
            {
                case ToastPosition.Bottom:
                    posY = activeHeight - textHeight;
                    break;
                case ToastPosition.Center:
                    // Already set above
                    break;
                case ToastPosition.Top:
                    posY = textHeight;
                    break;
                default:
                    break;
            }

            CGPoint newCenter;
            float rotateAngle;

            if (ignoreOrientation)
            {
                rotateAngle = 0.0f;
                newCenter = new CGPoint(posX, posY);
            }
            else
            {
                switch (orientation)
                {
                    case UIInterfaceOrientation.PortraitUpsideDown:
                        rotateAngle = (float) Math.PI;
                        newCenter = new CGPoint(posX, orientationFrame.Size.Height - posY);
                        break;
                    case UIInterfaceOrientation.LandscapeLeft:
                        rotateAngle = (float) (-Math.PI / 2.0f);
                        newCenter = new CGPoint(posY, posX);
                        break;
                    case UIInterfaceOrientation.LandscapeRight:
                        rotateAngle = (float) (Math.PI / 2.0f);
                        newCenter = new CGPoint(orientationFrame.Size.Height - posY, posX);
                        break;
                    default: // as UIInterfaceOrientationPortrait
                        rotateAngle = 0.0f;
                        newCenter = new CGPoint(posX, posY);
                        break;
                }
            }

            if (notification != null)
                Animate(animationDuration,
                    0, UIViewAnimationOptions.AllowUserInteraction, delegate { MoveToPoint(newCenter, rotateAngle); },
                    null);
            else
                MoveToPoint(newCenter, rotateAngle);
        }

        private void MoveToPoint(CGPoint newCenter, float angle)
        {
            HudView.Transform = CGAffineTransform.MakeRotation(angle);
            HudView.Center = newCenter;
        }

        private void UnRegisterNotifications()
        {
            if (_eventListeners != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObservers(_eventListeners);
                _eventListeners.Clear();
                _eventListeners = null;
            }
        }
    }
}