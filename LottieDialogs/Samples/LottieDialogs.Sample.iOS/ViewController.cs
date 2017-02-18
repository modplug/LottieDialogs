using System;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using LottieDialogs.Abstractions;
using LottieDialogs.iOS;
using UIKit;

namespace LottieDialogs.Sample.iOS
{
    public partial class ViewController : UIViewController
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();
        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            UIButton button = new UIButton(new CGRect(0, 0, 100, 50));
            button.SetTitle("Show dialog", UIControlState.Normal);
            button.BackgroundColor = UIColor.Black;
            button.TouchUpInside += (sender, args) => ShowDialog();
            Add(button);
        }

        private async void ShowDialog()
        {
            var url = NSUrl.FromFilename("TwitterHeart.json");
            await LottieDialog.Instance.ShowDialog(url, MaskType.Clear, 0, false, StatusTextPosition.Center, "Progress: " + 0 + "%", "", TimeSpan.FromSeconds(2), CancelCallback);
            for (var i = 0; i <= 100; i++)
            {
                if (_cts.IsCancellationRequested)
                {
                    _cts = new CancellationTokenSource();
                    break;
                }
                LottieDialog.Instance.UpdateProgress((float)i / 100, "Progress: " + i + "%");
                await Task.Delay(50);
                if (i == 100)
                {
                    await LottieDialog.Instance.DismissDialog();
                }
            }
        }

        private void CancelCallback()
        {
            _cts.Cancel();
        }
    }
}