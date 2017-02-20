using System;
using System.Collections.Generic;
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
        private readonly List<string> _animations;
        private readonly Random _random;

        public ViewController(IntPtr handle) : base(handle)
        {
            _animations = new List<string> {"allboard.json", "hamburger.json", "twitterheart.json"};
            _random = new Random();
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
            var index = _random.Next(0, _animations.Count);
            var url = NSUrl.FromFilename(_animations[index]);
            await LottieDialog.Instance.ShowDialog(url, MaskType.Black, 0, true, DialogType.AnimationOnly, "Progress: " + 0 + "%", "", null, CancelCallback);
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