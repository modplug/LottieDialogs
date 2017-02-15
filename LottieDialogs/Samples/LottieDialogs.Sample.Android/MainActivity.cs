using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content.Res;
using Android.Widget;
using Android.OS;
using LottieDialogs.Abstractions;
using LottieDialogs.Android.Controls;

namespace LottieDialogs.Sample.Android
{
    [Activity(Label = "LottieDialogs.Sample.Android", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private LottieProgressControl _progress;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            var button = FindViewById<Button>(Resource.Id.MyButton);
            _progress = FindViewById<LottieProgressControl>(Resource.Id.MyProgressControl);
            button.Click += ButtonOnClick;
        }

        private async void ButtonOnClick(object sender, EventArgs eventArgs)
        {
            var stream = Assets.Open("TwitterHeart.json", Access.Buffer);
            var stream2 = Assets.Open("TwitterHeart.json", Access.Buffer);
            _progress.Source = stream;

            for (int i = 0; i <= 100; i++)
            {
                LottieDialogs.Android.LottieDialog.Shared.ShowProgressDialog(this, stream2, MaskType.Clear, i, false, ToastPosition.Bottom, "Progress: " + i + "%", "");
                await Task.Delay(50);
                if (i == 100)
                {
                    LottieDialogs.Android.LottieDialog.Shared.Dismiss(this);
                    _progress.IsAnimating = false;
                }
            }
        }
    }
}