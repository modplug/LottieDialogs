using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Airbnb.Lottie;
using Com.Airbnb.Lottie.Model;
using LottieDialogs.Abstractions;

namespace LottieDialogs.Android
{
    public class LottieDialog
    {
        private static LottieDialog _instance;

        private readonly object _dialogLock = new object();
        private readonly ManualResetEvent _waitDismiss = new ManualResetEvent(false);
        private LottieAnimationView _animationView;
        private TextView _statusView;

        public static LottieDialog Instance => _instance ?? (_instance = new LottieDialog());

        public Dialog CurrentDialog { get; private set; }

        public void ShowProgressDialog(Context context, Stream stream, MaskType maskType, float progress,
            bool isIndeterminate = true,
            StatusTextPosition statusTextPosition = StatusTextPosition.Bottom, string status = null, string dismissDescription = null,
            TimeSpan? timeout = null, Action clickCallback = null, Action dismissCallback = null)
        {
            if (!timeout.HasValue)
                timeout = TimeSpan.Zero;

            if (CurrentDialog != null && _animationView == null)
                DismissCurrent(context);

            lock (_dialogLock)
            {
                if (CurrentDialog == null)
                {
                    Application.SynchronizationContext.Send(state =>
                    {
                        CurrentDialog = new Dialog(context);

                        CurrentDialog.RequestWindowFeature((int) WindowFeatures.NoTitle);

                        if (maskType != MaskType.Black)
                            CurrentDialog.Window.ClearFlags(WindowManagerFlags.DimBehind);

                        if (maskType == MaskType.None)
                            CurrentDialog.Window.SetFlags(WindowManagerFlags.NotTouchModal,
                                WindowManagerFlags.NotTouchModal);

                        CurrentDialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));

                        var inflater = LayoutInflater.FromContext(context);

                        View view;
                        switch (statusTextPosition)
                        {
                            case StatusTextPosition.Bottom:
                                view = inflater.Inflate(Resource.Layout.bottom_header_progress_dialog, null);
                                _statusView = view.FindViewById<TextView>(Resource.Id.bottom_textViewStatus);
                                break;
                            case StatusTextPosition.Center:
                                view = inflater.Inflate(Resource.Layout.center_header_progress_dialog, null);
                                _statusView = view.FindViewById<TextView>(Resource.Id.center_textViewStatus);
                                break;
                            case StatusTextPosition.Top:
                                view = inflater.Inflate(Resource.Layout.top_header_progress_dialog, null);
                                _statusView = view.FindViewById<TextView>(Resource.Id.top_textViewStatus);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(statusTextPosition), statusTextPosition, null);
                        }
                        

                        if (clickCallback != null)
                            view.Click += (sender, e) => clickCallback();
                        _animationView = view.FindViewById<LottieAnimationView>(Resource.Id.lottieAnimationView);
                        try
                        {
                            LottieComposition.FromInputStream(context, stream,
                                lottieComposition =>
                                {
                                    _animationView.SetComposition(lottieComposition);
                                    _animationView.Loop(isIndeterminate);
                                    if (!isIndeterminate)
                                        _animationView.PauseAnimation();
                                });
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }

                        switch (maskType)
                        {
                            case MaskType.None:
                                view.SetBackgroundResource(Resource.Drawable.roundedbgdark);
                                _statusView.SetTextColor(Color.White);
                                break;
                            case MaskType.Clear:
                                view.SetBackgroundColor(Color.Transparent);
                                _statusView.SetTextColor(Color.Black);
                                break;
                            case MaskType.Black:
                                view.SetBackgroundResource(Resource.Drawable.roundedbg_white);
                                _statusView.SetTextColor(Color.Black);
                                break;
                            case MaskType.Gradient:
                                view.SetBackgroundResource(Resource.Drawable.roundedbg_white);
                                _statusView.SetTextColor(Color.Black);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(maskType), maskType, null);
                        }

                        if (_statusView != null)
                        {
                            _statusView.Text = status ?? "";
                            _statusView.Visibility = string.IsNullOrEmpty(status) ? ViewStates.Gone : ViewStates.Visible;
                        }

                        CurrentDialog.SetContentView(view);

                        CurrentDialog.SetCancelable(dismissCallback != null);
                        if (dismissCallback != null)
                            CurrentDialog.CancelEvent += (sender, e) => dismissCallback();

                        CurrentDialog.Show();
                    }, null);


                    if (timeout.Value > TimeSpan.Zero)
                        Task.Factory.StartNew(() =>
                        {
                            if (!_waitDismiss.WaitOne(timeout.Value))
                                DismissCurrent(context);
                        }).ContinueWith(ct =>
                        {
                            var ex = ct.Exception;

                            if (ex != null)
                                Log.Error("LottieDialog", ex.ToString());
                        }, TaskContinuationOptions.OnlyOnFaulted);
                }
                else
                {
                    Application.SynchronizationContext.Send(state =>
                    {
                        _animationView.Progress = progress / 100;
                        _statusView.Text = status ?? "";
                    }, null);
                }
            }
        }

        public void Dismiss(Context context = null)
        {
            DismissCurrent(context);
        }

        private void DismissCurrent(Context context = null)
        {
            lock (_dialogLock)
            {
                if (CurrentDialog != null)
                {
                    _waitDismiss.Set();

                    Action actionDismiss = () =>
                    {
                        CurrentDialog.Hide();
                        CurrentDialog.Dismiss();
                        _statusView = null;
                        _animationView = null;
                        CurrentDialog = null;
                        _waitDismiss.Reset();
                    };

                    //First try the SynchronizationContext
                    if (Application.SynchronizationContext != null)
                    {
                        Application.SynchronizationContext.Send(state => actionDismiss(), null);
                        return;
                    }

                    //Next let's try and get the Activity from the CurrentDialog
                    if (CurrentDialog?.Window?.Context != null)
                    {
                        var activity = CurrentDialog.Window.Context as Activity;

                        if (activity != null)
                        {
                            activity.RunOnUiThread(actionDismiss);
                            return;
                        }
                    }

                    //Finally if all else fails, let's see if someone passed in a context to dismiss and it
                    // happens to also be an Activity
                    if (context != null)
                    {
                        var activity = context as Activity;
                        activity?.RunOnUiThread(actionDismiss);
                    }
                }
            }
        }
    }
}