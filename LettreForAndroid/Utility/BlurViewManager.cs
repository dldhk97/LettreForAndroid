using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Com.EightbitLab.BlurViewBinding;

namespace LettreForAndroid.Utility
{
    class BlurViewManager
    {
        Activity activity;
        public BlurViewManager(Activity iActivity)
        {
            activity = iActivity;
        }
        //BlurView 적용
        public void SetupBlurView()
        {
            ViewGroup root = activity.FindViewById<ViewGroup>(Resource.Id.root);
            BlurView mainBlurView = activity.FindViewById<BlurView>(Resource.Id.mainBlurView);

            float radius = 0.0001F;

            Drawable windowBackground = activity.Window.DecorView.Background;

            var topViewSettings = mainBlurView.SetupWith(root)
                .WindowBackground(windowBackground)
                .BlurAlgorithm(new RenderScriptBlur(activity))
                .BlurRadius(radius)
                .SetHasFixedTransformationMatrix(true);
        }
    }
}