using System;
using Airbnb.Lottie;
using Cirrious.FluentLayouts.Touch;
using LottieDialogs.Abstractions;
using UIKit;

namespace LottieDialogs.iOS.Extensions
{
    public static class LottieDialogViewExtensions
    {
        private static readonly UIColor SemiTransparentBlack =  new UIColor(0, 0, 0, 0.8f);
        private static readonly float CornerRadius = 15f;
        public static UIView TopHeaderDialog(this UIView parent, UIView backgroundView, LAAnimationView animationView, UILabel statusLabel, MaskType maskType)
        {
            parent.BackgroundColor = GetBackgroundColorFromMaskType(maskType);
            backgroundView.Layer.CornerRadius = CornerRadius;
            backgroundView.BackgroundColor = maskType == MaskType.Clear ? SemiTransparentBlack : UIColor.White;
            statusLabel.TextColor = maskType == MaskType.Clear ? UIColor.White : UIColor.Black;
            backgroundView.ClipsToBounds = true;
            parent.Add(backgroundView);
            backgroundView.Add(statusLabel);
            backgroundView.Add(animationView);

            parent.AddConstraints(new FluentLayout[]
            {
                backgroundView.Width().EqualTo(120),
                backgroundView.Height().EqualTo(120),
                backgroundView.WithSameCenterX(parent),
                backgroundView.WithSameCenterY(parent),
            });

            backgroundView.AddConstraints(new FluentLayout[]
            {
                statusLabel.Width().LessThanOrEqualTo(150),
                statusLabel.Height().LessThanOrEqualTo(40),
                statusLabel.WithSameCenterX(backgroundView),
                statusLabel.AtTopOf(backgroundView, 15),
                animationView.WithSameCenterX(backgroundView),
                animationView.Height().EqualTo(75),
                animationView.Width().EqualTo(75),
                animationView.Below(statusLabel, 0),
            });

            backgroundView.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
            parent.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
            return parent;
        }

        public static UIView BottomHeaderDialog(this UIView parent, UIView backgroundView, LAAnimationView animationView, UILabel statusLabel, MaskType maskType)
        {
            parent.BackgroundColor = GetBackgroundColorFromMaskType(maskType);
            backgroundView.Layer.CornerRadius = CornerRadius;
            backgroundView.BackgroundColor = maskType == MaskType.Clear ? SemiTransparentBlack : UIColor.White;
            statusLabel.TextColor = maskType == MaskType.Clear ? UIColor.White : UIColor.Black;
            backgroundView.ClipsToBounds = true;
            parent.Add(backgroundView);
            backgroundView.Add(statusLabel);
            backgroundView.Add(animationView);

            parent.AddConstraints(new FluentLayout[]
            {
                backgroundView.Width().EqualTo(120),
                backgroundView.Height().EqualTo(120),
                backgroundView.WithSameCenterX(parent),
                backgroundView.WithSameCenterY(parent),
            });

            backgroundView.AddConstraints(new FluentLayout[]
            {
                animationView.AtTopOf(backgroundView, 15),
                animationView.WithSameCenterX(backgroundView),
                animationView.Height().EqualTo(75),
                animationView.Width().EqualTo(75),
                statusLabel.Below(animationView, 0),
                statusLabel.Width().LessThanOrEqualTo(150),
                statusLabel.Height().LessThanOrEqualTo(40),
                statusLabel.WithSameCenterX(backgroundView),
            });

            backgroundView.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
            parent.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
            return parent;
        }

        public static UIView CenterHeaderDialog(this UIView parent, UIView backgroundView, UILabel statusLabel, MaskType maskType)
        {
            parent.BackgroundColor = GetBackgroundColorFromMaskType(maskType);
            backgroundView.Layer.CornerRadius = CornerRadius;
            backgroundView.BackgroundColor = maskType == MaskType.Clear ? SemiTransparentBlack : UIColor.White;
            statusLabel.TextColor = maskType == MaskType.Clear ? UIColor.White : UIColor.Black;
            backgroundView.ClipsToBounds = true;
            parent.Add(backgroundView);
            backgroundView.Add(statusLabel);

            parent.AddConstraints(new FluentLayout[]
            {
                backgroundView.Width().EqualTo(120),
                backgroundView.Height().EqualTo(120),
                backgroundView.WithSameCenterX(parent),
                backgroundView.WithSameCenterY(parent),
            });

            backgroundView.AddConstraints(new FluentLayout[]
            {
                statusLabel.Width().LessThanOrEqualTo(150),
                statusLabel.Height().LessThanOrEqualTo(40),
                statusLabel.WithSameCenterX(backgroundView),
                statusLabel.WithSameCenterY(backgroundView),
            });

            backgroundView.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
            parent.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
            return parent;
        }

        public static UIView AnimationOnlyDialog(this UIView parent, UIView backgroundView, LAAnimationView animationView, MaskType maskType)
        {
            parent.BackgroundColor = GetBackgroundColorFromMaskType(maskType);
            backgroundView.Layer.CornerRadius = CornerRadius;
            backgroundView.BackgroundColor = maskType == MaskType.Clear ? SemiTransparentBlack : UIColor.White;
            backgroundView.ClipsToBounds = true;
            parent.Add(backgroundView);
            backgroundView.Add(animationView);
            backgroundView.Add(animationView);

            parent.AddConstraints(new FluentLayout[]
            {
                backgroundView.Width().EqualTo(120),
                backgroundView.Height().EqualTo(120),
                backgroundView.WithSameCenterX(parent),
                backgroundView.WithSameCenterY(parent),
            });

            backgroundView.AddConstraints(new FluentLayout[]
            {
                animationView.WithSameCenterX(backgroundView),
                animationView.WithSameCenterY(backgroundView),
                animationView.Height().EqualTo(100),
                animationView.Width().EqualTo(100),
            });

            backgroundView.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
            parent.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
            return parent;
        }

        private static UIColor GetBackgroundColorFromMaskType(MaskType maskType)
        {
            switch (maskType)
            {
                case MaskType.None:
                    return UIColor.White;
                case MaskType.Clear:
                    return UIColor.Clear;
                case MaskType.Black:
                case MaskType.Gradient:
                    return SemiTransparentBlack;
                default:
                    throw new ArgumentOutOfRangeException(nameof(maskType), maskType, null);
            }
        }
    }
}
