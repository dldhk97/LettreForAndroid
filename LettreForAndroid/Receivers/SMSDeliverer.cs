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
using Java.Lang;
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

            string displayName = string.Empty;
            int unreadCnt = 0;
            TextMessage objMsg = null;

            foreach (var msg in msgs)
            {
                objMsg = ConvertToCustomMessageType(msg);           //시스템 메시지 형식을 레뜨레 메시지 형식으로 변환

                UpdateMessage(context, objMsg);                     //메시지를 안드로이드 메시지 DB에 삽입

                ContactData objContact = ContactDBManager.Get().GetContactDataByAddress(objMsg.Address, true);      //연락처에 있는지 조회

                //연락처에 없으면 서버에 전송.
                if (objContact == null)
                {
                    LableDBManager.Get().AccumulateLableDB(objMsg);                                 //서버에서 레이블 데이터 받은 후 레이블 DB에 저장
                    displayName = objMsg.Address;                                                   //연락처에 없으면 전화번호로 이름 표시
                }
                else
                {
                    displayName = objContact.Name;                                                  //연락처에 있으면 표시될 이름 변경
                }

                unreadCnt++;                    //읽지않은 개수 카운트

                NotificationHandler.Notification(context, "Lettre Channel 1", displayName, objMsg.Msg, objMsg.Address, "Ticker", 101, unreadCnt);       //알림 표시

                //IsLoaded가 true이면 어플이 포그라운드, 메시지 목록이 메모리에 올라와있는 상태임.
                //false이면 최근 메시지목록을 불러오지 않는다. (메모리에 올라간 메시지가 없기 때문에 Refresh하려면 크래시남)
                if (MessageDBManager.Get().IsLoaded == true)
                    MessageDBManager.Get().RefreshLastMessage(MessageDBManager.Get().GetThreadId(objMsg.Address));  //해당 메시지가 속하는 대화를 찾아 최신문자를 새로고침함.
            }

            MainFragActivity.RefreshUI();               //UI 새로고침
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


        private TextMessage ConvertToCustomMessageType(SmsMessage msg)
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

    }
}