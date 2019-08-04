using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.V4.App;
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
                TextMessage objMsg = ConvertToTM(msg);  //시스템 메시지 형식을 레뜨레 메시지 형식으로 변환

                UpdateMessage(context, objMsg);                 //DB에 저장

                string displayName = objMsg.Address;

                //연락처에 없으면 일상 대화가 아니므로, 서버에 보낸다.
                if (ContactDBManager.Get().getContactDataByAddress(objMsg.Address) == null)
                    LableDBManager.Get().AccumulateLableDB(objMsg);                                 //서버에서 데이터 받은 후 레이블 DB에 저장
                else
                    displayName = MessageDBManager.Get().DialogueSets[(int)Dialogue.LableType.COMMON][objMsg.Thread_id].DisplayName;       //연락처에 있으면 표시될 이름 변경

                NotificationHandler.Notification(context, "Lettre Channel 1", displayName, objMsg.Msg, "Ticker", 101);       //알림 표시
            }

            MessageDBManager.Get().Refresh();           //메세지 DB 새로고침

            RefreshUI();
        }

        private TextMessage ConvertToTM(SmsMessage msg)
        {
            TextMessage objMessage = new TextMessage();
            objMessage.Address = msg.OriginatingAddress;
            objMessage.Msg = msg.MessageBody;

            DateTimeUtillity dtu = new DateTimeUtillity();
            objMessage.Time = dtu.getCurrentMilTime();

            objMessage.ReadState = (int)TextMessage.MESSAGE_READSTATE.UNREAD;
            objMessage.Type = (int)TextMessage.MESSAGE_TYPE.RECEIVED;
            objMessage.Thread_id = MessageDBManager.Get().GetThreadId(msg.OriginatingAddress);

            return objMessage;
        }

        //문자를 DB에 저장
        private void UpdateMessage(Context context, TextMessage msg)
        {
            ContentValues values = new ContentValues();
            values.Put(Telephony.TextBasedSmsColumns.Address, msg.Address);
            values.Put(Telephony.TextBasedSmsColumns.Body, msg.Msg);
            values.Put(Telephony.TextBasedSmsColumns.Date, msg.Time);
            values.Put(Telephony.TextBasedSmsColumns.Read, msg.ReadState);
            values.Put(Telephony.TextBasedSmsColumns.Type, msg.Type);
            values.Put(Telephony.TextBasedSmsColumns.ThreadId, msg.Thread_id);
            context.ContentResolver.Insert(Telephony.Sms.Inbox.ContentUri, values);
        }

        //UI 새로고침
        private void RefreshUI()
        {
            //대화창 존재하면 새로고침
            if (DialogueActivity._Instance != null)
                DialogueActivity._Instance.RefreshRecyclerView();

            //탭, 메인 새로고침
            TabFragManager._Instance.RefreshLayout();
        }

        
    }
}