using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Telephony;
using Android.Views;
using Android.Widget;
using LettreForAndroid.Utility;

namespace LettreForAndroid.Receivers
{
    [BroadcastReceiver(Label = "SmsSendReceiver", Permission = Manifest.Permission.SendSms)]
    [IntentFilter(new string[] { FILTER_SENT }, Categories = new[] { "android.intent.category.DEFAULT" })]
    public class SmsSentReceiver : BroadcastReceiver
    {
        public const string FILTER_SENT = "SMS_SENT";

        public override void OnReceive(Context context, Intent intent)
        {
            switch ((int)ResultCode)
            {
                case (int)Result.Ok:
                    Toast.MakeText(Application.Context, "메시지가 발송되었습니다.", ToastLength.Short).Show();
                    break;
                case (int)SmsResultError.GenericFailure:
                    Toast.MakeText(Application.Context, "Generic Failure", ToastLength.Short).Show();
                    break;
                case (int)SmsResultError.NoService:
                    Toast.MakeText(Application.Context, "No Service", ToastLength.Short).Show();
                    break;
                case (int)SmsResultError.NullPdu:
                    Toast.MakeText(Application.Context, "Null PDU", ToastLength.Short).Show();
                    break;
                case (int)SmsResultError.RadioOff:
                    Toast.MakeText(Application.Context, "Radio Off", ToastLength.Short).Show();
                    break;
            }

            SentCompleteEvent((int)ResultCode);
        }

        public delegate void SentEventHandler(int resultCode);
        public event SentEventHandler SentCompleteEvent;
    }
}