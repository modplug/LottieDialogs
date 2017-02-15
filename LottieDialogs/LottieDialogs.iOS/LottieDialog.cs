using System;
using System.Threading;
using Airbnb.Lottie;
using Cirrious.FluentLayouts.Touch;
using Foundation;
using LottieDialogs.Abstractions;
using UIKit;

namespace LottieDialogs.iOS
{
    public class LottieDialog : UIViewController
    {
        public static LottieDialog Instance => _dialog ?? (_dialog = new LottieDialog());

        private LAAnimationView _animationView;
        private static LottieDialog _dialog;
        private Progress<float> _progress;
        private bool _isIndeterminate;
        private Timer _timer;
        private UIViewController _vc;
        private UILabel _label;
        private UITapGestureRecognizer _tapGestureRecognizer;
        private UIView _backgroundView;

        public void ShowDialog(NSUrl nsUrl, bool isIndeterminate, Progress<float> progress, TimeSpan? dialogTimeOut = null, MaskType maskType = MaskType.Black)
        {
            _progress = progress;
            _isIndeterminate = isIndeterminate;
            if (dialogTimeOut != null)
            {
                _timer = new Timer(state => DismissDialog(), null, dialogTimeOut.Value, TimeSpan.FromSeconds(1));
            }

            _animationView = new LAAnimationView(nsUrl);
            _label = new UILabel();
            _backgroundView = new UIView();

            if (!_isIndeterminate)
            {
                _progress.ProgressChanged += OnProgressChanged;
            }
            else
            {
                _animationView.LoopAnimation = true;
                _animationView.Play();
            }

            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                var window = UIApplication.SharedApplication.KeyWindow;
                ModalPresentationStyle = UIModalPresentationStyle.OverFullScreen;
                ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve;
                
                _vc = window.RootViewController;
                while (_vc.PresentedViewController != null)
                {
                    _vc = _vc.PresentedViewController;
                }
                
                _vc.PresentViewController(this, true, null);
            });

            _tapGestureRecognizer = new UITapGestureRecognizer(DismissDialog);
            View.AddGestureRecognizer(_tapGestureRecognizer);
            View.Alpha = 0.01f;
            SetupView();
        }

        private void OnProgressChanged(object sender, float progress)
        {
            _animationView.AnimationProgress = progress;
            if (progress >= 1)
            {
                DismissDialog();
            }
        }

        public void DismissDialog()
        {
            _progress.ProgressChanged -= OnProgressChanged;
            _timer?.Dispose();
            _animationView.Pause();
            _label.RemoveFromSuperview();
            _label?.Dispose();
            _animationView.RemoveFromSuperview();
            _animationView?.Dispose();
            View.RemoveGestureRecognizer(_tapGestureRecognizer);
            InvokeOnMainThread(() =>
            {
                UIView.Animate(0.3f, 0, UIViewAnimationOptions.CurveEaseOut, () =>
                {
                    View.Alpha = 0;
                }, () => DismissViewController(false, null));
            });
        }

        public void UpdateStatusText(string s)
        {
            _label.Text = s;
        }

        public void SetupView()
        {
            _backgroundView.Layer.CornerRadius = 10;
            _backgroundView.BackgroundColor = UIColor.White;
            _backgroundView.ClipsToBounds = true;
            Add(_backgroundView);
            _backgroundView.Add(_label);
            _backgroundView.Add(_animationView);

            View.AddConstraints(new FluentLayout[]
            {
                _backgroundView.Width().EqualTo(120),
                _backgroundView.Height().EqualTo(120),
                _backgroundView.WithSameCenterX(View),
                _backgroundView.WithSameCenterY(View),
            });

            _backgroundView.AddConstraints(new FluentLayout[]
            {
                _label.Width().LessThanOrEqualTo(150),
                _label.Height().LessThanOrEqualTo(40),
                _label.WithSameCenterX(_backgroundView),
                _label.AtTopOf(_backgroundView, 10),
                _animationView.WithSameCenterX(_backgroundView),
                _animationView.Height().EqualTo(75),
                _animationView.Width().EqualTo(75),
                _animationView.Below(_label, 0),
            });

            _backgroundView.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
            View.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();

            View.BackgroundColor = new UIColor(0, 0, 0, 0.7f);

            UIView.Animate(0.3f, 0, UIViewAnimationOptions.CurveEaseOut, () =>
            {
                View.Alpha = 1;
            }, null);
        }
    }
}
