using Airbnb.Lottie;
using CoreGraphics;
using Foundation;
using UIKit;

namespace LottieDialogs.iOS.Controls
{
    public class LottieProgressView : UIView
    {
        private readonly CGRect _frame;
        private readonly bool _isIndeterminate;
        private readonly LAAnimationView _animationView;
        private float _progress;

        public LottieProgressView(CGRect frame, NSUrl url, bool isIndeterminate = true)
        {
            _frame = frame;
            _isIndeterminate = isIndeterminate;
            _animationView = new LAAnimationView(url)
            {
                ContentStretch = frame,
                ContentMode = UIViewContentMode.ScaleAspectFit,
                LoopAnimation =  _isIndeterminate
            };
        }

        public bool HidesWhenStopped { get; set; }
        public void StartAnimating()
        {
            _animationView.Frame = _frame;
            _animationView.ContentStretch = _frame;
            _animationView.ContentMode = UIViewContentMode.ScaleAspectFit;
            _animationView.Hidden = false;
            if (_isIndeterminate)
            {
                _animationView.Play();
                IsAnimating = true;
            }
        }

        public float Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                if (!_isIndeterminate)
                {
                    _animationView.AnimationProgress = _progress;
                }
            }
        }

        public bool IsAnimating{ get; private set; }

        public void StopAnimating()
        {
            _animationView.Pause();
            IsAnimating = false;
            if (HidesWhenStopped)
            {
                _animationView.Frame = CGRect.Empty;
                _animationView.Hidden = true;
            }
        }
    }
}