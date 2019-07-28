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
using Android;
using Android.Provider;

namespace LettreForAndroid.Receivers
{
    [Activity(Label = "SmsSendActivity")]
    [IntentFilter(new[] { "android.intent.action.SEND", "android.intent.action.SENDTO" }, Categories = new[] { "android.intent.category.DEFAULT", "android.intent.category.BROWSABLE" }, DataSchemes = new[] { "sms", "smsto", "mms", "mmsto" })]
    public class SmsSendActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Toast.MakeText(this, "SmsSendActivity created!", ToastLength.Short).Show();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }
    }
}