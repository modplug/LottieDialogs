using System;
using System.IO;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Airbnb.Lottie;
using Com.Airbnb.Lottie.Model;

namespace LottieDialogs.Android.Controls
{
    public class LottieProgressControl : LinearLayout
    {
        private readonly LottieAnimationView _lottieAnimationView;
        private bool _isAnimating;
        private bool _isIndeterminate;
        private ViewGroup.LayoutParams _layoutParameters;
        private float _progress;
        private Stream _source;
        private bool _sourceIsValid;

        public LottieProgressControl(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public LottieProgressControl(Context context) : base(context)
        {
            _lottieAnimationView = new LottieAnimationView(context);
            SetView();
        }

        public LottieProgressControl(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            _lottieAnimationView = new LottieAnimationView(context);
            var attributes = context.ObtainStyledAttributes(attrs, Resource.Styleable.LottieProgressControl);
            IsIndeterminate = attributes.GetBoolean(Resource.Styleable.LottieProgressControl_IsIndeterminate, true);
            SetView();
        }

        public LottieProgressControl(Context context, IAttributeSet attrs, int defStyleAttr)
            : base(context, attrs, defStyleAttr)
        {
            _lottieAnimationView = new LottieAnimationView(context);
            var a = context.ObtainStyledAttributes(attrs, Resource.Styleable.LottieProgressControl, defStyleAttr, 0);
            IsIndeterminate = a.GetBoolean(Resource.Styleable.LottieProgressControl_IsIndeterminate, true);
            SetView();
        }

        public LottieProgressControl(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes)
            : base(context, attrs, defStyleAttr, defStyleRes)
        {
            _lottieAnimationView = new LottieAnimationView(context);
            SetView();
        }

        public override ViewGroup.LayoutParams LayoutParameters
        {
            get { return _layoutParameters; }
            set
            {
                _layoutParameters = value;
                _lottieAnimationView.LayoutParameters = _layoutParameters;
            }
        }

        public Stream Source
        {
            get { return _source; }
            set
            {
                _source = value;
                UpdateSource(_source);
            }
        }

        public float Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                if (!_sourceIsValid)
                    return;
                if (_sourceIsValid && !_isIndeterminate)
                    _lottieAnimationView.Progress = _progress;
            }
        }

        public bool IsIndeterminate
        {
            get { return _isIndeterminate; }
            set
            {
                _isIndeterminate = value;
                if (!_sourceIsValid)
                    return;
                if (_isIndeterminate)
                {
                    _lottieAnimationView.Progress = 0;
                    _lottieAnimationView.PlayAnimation();
                    _lottieAnimationView.Loop(true);
                }
                else
                {
                    _lottieAnimationView.PauseAnimation();
                    _lottieAnimationView.Loop(false);
                }
            }
        }

        public bool IsAnimating
        {
            get { return _isAnimating; }
            set
            {
                _isAnimating = value;
                if (!_isAnimating && _isIndeterminate)
                    _lottieAnimationView.PauseAnimation();
            }
        }

        private void SetView()
        {
            AddView(_lottieAnimationView);
        }

        private void UpdateSource(Stream stream)
        {
            LottieComposition.FromInputStream(Context, stream, lottieComposition =>
            {
                try
                {
                    _lottieAnimationView.SetComposition(lottieComposition);
                    _sourceIsValid = true;
                    if (_isIndeterminate)
                    {
                        _lottieAnimationView.Progress = 0;
                        _lottieAnimationView.PlayAnimation();
                        _lottieAnimationView.Loop(true);
                    }
                    else
                    {
                        _lottieAnimationView.PauseAnimation();
                        _lottieAnimationView.Loop(false);
                    }
                }
                catch (Exception)
                {
                    _sourceIsValid = false;
                }
            });
        }
    }
}