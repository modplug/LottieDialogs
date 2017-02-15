using System;
using CoreGraphics;
using Foundation;
using LottieDialogs.iOS;
using LottieDialogs.iOS.Controls;
using UIKit;

namespace LottieDialogs.Sample.iOS
{
    public partial class ViewController : UIViewController
    {
        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            var nsUrl = NSUrl.FromFilename("TwitterHeart.json");
            LottieProgressView  progressView = new LottieProgressView(new CGRect(0, 0, 100, 100), nsUrl);
            UIButton button = new UIButton(new CGRect(0, 0, 100, 50));
            button.SetTitle("Show dialog", UIControlState.Normal);
            button.BackgroundColor = UIColor.Black;
            button.TouchUpInside += (sender, args) => ShowDialog();
            //progressView.HidesWhenStopped = true;
            //progressView.StartAnimating();
            //Add(progressView);
            Add(button);
            var tapGestureRecognizer = new UITapGestureRecognizer(ShowDialog);
            progressView.AddGestureRecognizer(tapGestureRecognizer);
        }

        public override void ViewDidAppear(bool animated)
        {
            
            base.ViewDidAppear(animated);
        }

        private static void ShowDialog()
        {
            var stream = NSUrl.FromFilename("TwitterHeart.json").DataRepresentation.AsStream();
            var url = NSUrl.FromFilename("TwitterHeart.json");
            LottieDialog.Instance.ShowDialog(url, true, new Progress<float>());
            LottieDialog.Instance.UpdateStatusText("Loading...");
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}