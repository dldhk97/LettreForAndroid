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
using LettreForAndroid.Class;
using LettreForAndroid.UI;
using LettreForAndroid.Utility;

namespace LettreForAndroid.Receivers
{
    [BroadcastReceiver(Label = "SmsDeliverer", Permission = "android.permission.BROADCAST_SMS")]
    [IntentFilter(new string[] { "android.provider.Telephony.SMS_DELIVER" }, Categories = new[] { "android.intent.category.DEFAULT" })]
    public class SmsDeliverer : BroadcastReceiver
    {
        public const string FILTER_DELIVERED = "android.provider.Telephony.SMS_DELIVER";

        public override void OnReceive(Context context, Intent intent)
        {
            var msgs = Telephony.Sms.Intents.GetMessagesFromIntent(intent);

            foreach (var msg in msgs)
            {
                Toast.MakeText(context, "FROM : " + msg.DisplayOriginatingAddress + "\nMSG : " + msg.MessageBody, ToastLength.Short).Show();

                //문자를 DB에 저장
                ContentValues values = new ContentValues();
                values.Put(Telephony.TextBasedSmsColumns.Address, msg.OriginatingAddress);
                values.Put(Telephony.TextBasedSmsColumns.Body, msg.MessageBody);
                DateTimeUtillity dtu = new DateTimeUtillity();
                values.Put(Telephony.TextBasedSmsColumns.Date, dtu.getCurrentMilTime());
                values.Put(Telephony.TextBasedSmsColumns.Read, 0);
                values.Put(Telephony.TextBasedSmsColumns.Type, 0);
                values.Put(Telephony.TextBasedSmsColumns.ThreadId, MessageManager.Get().getThreadId(context, msg.OriginatingAddress));

                context.ContentResolver.Insert(Telephony.Sms.Inbox.ContentUri, values);

                //메세지 DB 불러오기
                MessageManager.Get().refreshMessages();

                //UI 업데이트
                if(DialogueActivity._Instance != null)
                    DialogueActivity._Instance.RefreshRecyclerView();

                for(int i = 0; i < CustomPagerAdapter.pages.Count; i++)
                {
                    CustomPagerAdapter.pages[i].refreshRecyclerView();
                }
                    
            }
        }


        public delegate void DeliverEventHandler();
        public event DeliverEventHandler DeliverCompleteEvent;
    }
}