using System;
using Foundation;
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
            LottieProgressView  progressView = new LottieProgressView(View.Frame, NSUrl.FromFilename("TwitterHeart.json"));
            progressView.HidesWhenStopped = true;
            Add(progressView);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}