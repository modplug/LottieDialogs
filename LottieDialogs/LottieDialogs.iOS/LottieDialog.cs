using System;
using System.Threading;
using System.Threading.Tasks;
using Airbnb.Lottie;
using Foundation;
using LottieDialogs.Abstractions;
using LottieDialogs.iOS.Extensions;
using UIKit;

namespace LottieDialogs.iOS
{
    public class LottieDialog : UIViewController
    {
        private static LottieDialog _dialog;
        private LAAnimationView _animationView;
        private UIView _backgroundView;
        private Action _dismissDialogAction;
        private bool _isIndeterminate;
        private static bool _isVisible;
        private MaskType _maskType;
        private float _progress;
        private UILabel _statusLabel;
        private StatusTextPosition _statusTextPosition;
        private UITapGestureRecognizer _tapGestureRecognizer;
        private Timer _timer;
        private UIViewController _vc;
        private Action _cancelCallback;
        private string _dismissDescription;
        public static LottieDialog Instance => _dialog ?? (_dialog = new LottieDialog());

        public async Task ShowDialog(NSUrl url, MaskType maskType, float progress,
            bool isIndeterminate = true,
            StatusTextPosition statusTextPosition = StatusTextPosition.Bottom, string status = null,
            string dismissDescription = null,
            TimeSpan? timeout = null, Action cancelCallback = null, Action dismissCallback = null)
        {
            _progress = progress;
            _isIndeterminate = isIndeterminate;
            _dismissDialogAction = dismissCallback;
            _cancelCallback = cancelCallback;
            _maskType = maskType;
            _statusTextPosition = statusTextPosition;
            _dismissDescription = dismissDescription;

            if (!_isVisible)
            {
                _animationView = new LAAnimationView(url);
                _statusLabel = new UILabel();
                _backgroundView = new UIView();
                SetupView();
                PresentDialog();
            }

            if (!_isIndeterminate)
            {
                _animationView.LoopAnimation = false;
                _animationView.AnimationProgress = _progress;
                if (_progress > 1)
                {
                    await DismissDialog();
                }
            }
            else
            {
                _animationView.LoopAnimation = true;
                _animationView.Play();
            }

            if (timeout != null)
            {
                _timer = new Timer(async state => await DismissDialog(), null, timeout.Value, TimeSpan.FromSeconds(1));
            }

            _statusLabel.Text = status;
            if (cancelCallback != null)
            {
                _tapGestureRecognizer = new UITapGestureRecognizer(async () =>
                {
                    await DismissDialog();
                    cancelCallback.Invoke();
                });
                View.AddGestureRecognizer(_tapGestureRecognizer);
            }
        }

        private void PresentDialog()
        {
            var window = UIApplication.SharedApplication.KeyWindow;
            ModalPresentationStyle = UIModalPresentationStyle.OverFullScreen;
            ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve;
            _vc = window.RootViewController;
            if (_vc.PresentedViewController != null && !Equals(_vc.PresentedViewController, this))
            {
                _vc = _vc.PresentedViewController;
            }
            BeginInvokeOnMainThread(() =>
            {
                _isVisible = true;
                if ((_vc.PresentedViewController == null) ||
                    _vc.PresentedViewController != null && !_vc.PresentedViewController.Equals(this))
                {
                    _vc.PresentViewController(this, true, null);
                }
            });
        }

        public async Task DismissDialog()
        {
            TaskCompletionSource<bool> dismissTaskCompletionSource = new TaskCompletionSource<bool>();
            BeginInvokeOnMainThread(() =>
            {
                DismissViewController(true, () =>
                {
                    _animationView.Pause();
                    _backgroundView.RemoveFromSuperview();
                    _statusLabel.RemoveFromSuperview();
                    _animationView.RemoveFromSuperview();
                    _statusLabel?.Dispose();
                    _timer?.Dispose();
                    _animationView?.Dispose();
                    _isVisible = false;
                    _dismissDialogAction?.Invoke();
                    dismissTaskCompletionSource.SetResult(true);
                    if (_cancelCallback != null)
                    {
                        View.RemoveGestureRecognizer(_tapGestureRecognizer);
                    }
                });
            });
            await dismissTaskCompletionSource.Task;
        }

        private void SetupView()
        {
            switch (_statusTextPosition)
            {
                case StatusTextPosition.Bottom:
                    View.TopHeaderDialog(_backgroundView, _animationView, _statusLabel, _maskType);
                    break;
                case StatusTextPosition.Center:
                    View.CenterHeaderDialog(_backgroundView, _statusLabel, _maskType);
                    break;
                case StatusTextPosition.Top:
                    View.TopHeaderDialog(_backgroundView, _animationView, _statusLabel, _maskType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void UpdateProgress(float progress, string status)
        {
            BeginInvokeOnMainThread(() =>
            {
                _statusLabel.Text = status;
                _animationView.AnimationProgress = progress;
            });
        }
    }
}