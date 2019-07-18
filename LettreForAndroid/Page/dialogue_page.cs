using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using LettreForAndroid.Class;

namespace LettreForAndroid
{
    [Activity(Label = "dialogue_page")]
    public class dialogue_page : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.dialogue_page);

            Button button1 = FindViewById<Button>(Resource.Id.button1);

            button1.Click += (obj, sender) =>
            {
                int position = Intent.GetIntExtra("position", -1);
                int category = Intent.GetIntExtra("category", -1);
                Finish();
            };

            
        }
    }
}