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
    [BroadcastReceiver(Label = "WAPPushDeliverer", Permission = "android.permission.BROADCAST_WAP_PUSH")]
    [IntentFilter(new string[] { "android.provider.Telephony.WAP_PUSH_DELIVER" }, DataMimeType = "application/vnd.wap.mms-message")]
    public class WAPPushDeliverer : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Toast.MakeText(context, "MMS를 수신했지만 저장하지 못했습니다!", ToastLength.Short).Show();
        }
    }
}