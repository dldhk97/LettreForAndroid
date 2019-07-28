using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Telephony;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace LettreForAndroid.Receivers
{
    [BroadcastReceiver(Label = "SmsDeliverer", Permission = "android.permission.BROADCAST_SMS")]
    [IntentFilter(new string[] { "android.provider.Telephony.SMS_DELIVER" }, Categories = new[] { "android.intent.category.DEFAULT" })]
    public class SmsDeliverer : BroadcastReceiver
    {
        const string TAG = "DEBUGTAG1";
        public override void OnReceive(Context context, Intent intent)
        {
            var msgs = Telephony.Sms.Intents.GetMessagesFromIntent(intent);

            foreach (var msg in msgs)
            {
                Log.Debug(TAG, msg.MessageBody);
                Log.Debug(TAG, msg.DisplayOriginatingAddress);
                Toast.MakeText(context, "FROM : " + msg.DisplayOriginatingAddress + "\nBODY : " + msg.MessageBody, ToastLength.Short).Show();
            }
        }
    }
}