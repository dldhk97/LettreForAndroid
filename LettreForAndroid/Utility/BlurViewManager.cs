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
        Activity _Activity;
        public BlurViewManager(Activity iActivity)
        {
            _Activity = iActivity;
        }
        //BlurView 적용
        public void SetupBlurView()
        {
            ViewGroup root = _Activity.FindViewById<ViewGroup>(Resource.Id.root);
            BlurView mainBlurView = _Activity.FindViewById<BlurView>(Resource.Id.mainBlurView);

            float radius = 0.0001F;

            Drawable windowBackground = _Activity.Window.DecorView.Background;

            var topViewSettings = mainBlurView.SetupWith(root)
                .WindowBackground(windowBackground)
                .BlurAlgorithm(new RenderScriptBlur(_Activity))
                .BlurRadius(radius)
                .SetHasFixedTransformationMatrix(true);
        }
    }
}