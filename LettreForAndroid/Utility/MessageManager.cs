using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using LettreForAndroid.Class;

using Uri = Android.Net.Uri;

namespace LettreForAndroid.Utility
{
    public class MessageManager
    {
        private static Activity mActivity;
        private static List<TextMessage> mAllMessages;
        private static MessageManager mInstance = null;

        public static MessageManager Get()
        {
            if (mInstance == null)
                mInstance = new MessageManager();
            return mInstance;
        }
        public void Initialization(Activity iActivity)
        {
            mActivity = iActivity;
            refreshMessages();
        }

        public int Count
        {
            get { return mAllMessages.Count; }
        }

        //객체 생성 시 모든 문자메세지를 가져옴.

        private static void refreshMessages()
        {
            mAllMessages = new List<TextMessage>();
            TextMessage objSms = new TextMessage();
            Uri message = Uri.Parse("content://sms/");
            ContentResolver cr = mActivity.BaseContext.ContentResolver;

            ICursor c = cr.Query(message, null, null, null, "thread_id asc, date desc");
            mActivity.StartManagingCursor(c);
            int totalSMS = c.Count;

            if (c.MoveToFirst())
            {
                for (int i = 0; i < totalSMS; i++)
                {
                    objSms = new TextMessage();
                    objSms.Id = c.GetString(c.GetColumnIndexOrThrow("_id"));
                    objSms.Address = c.GetString(c.GetColumnIndexOrThrow("address"));
                    objSms.Person = c.GetString(c.GetColumnIndexOrThrow("person"));     //현재 null값만 있는거같음
                    objSms.Msg = c.GetString(c.GetColumnIndexOrThrow("body"));
                    objSms.ReadState = c.GetString(c.GetColumnIndex("read"));
                    objSms.Time = c.GetLong(c.GetColumnIndexOrThrow("date"));
                    objSms.Thread_id = c.GetString(c.GetColumnIndexOrThrow("thread_id"));
                    objSms.Folder = c.GetString(c.GetColumnIndexOrThrow("type"));

                    mAllMessages.Add(objSms);
                    c.MoveToNext();
                }
            }
            // else {
            // throw new RuntimeException("You have no SMS");
            // }
            c.Close();
        }

        //모든 메세지를 표시하는데, 한 연락처당 하나씩만 표시(클릭하면 대화창뜨게)
        public List<TextMessage> getAllMessages()
        {
            string prevThreadId = "NULL";
            SortedList<long, TextMessage> resultMessages = new SortedList<long, TextMessage>();
            for(int i = 0; i < mAllMessages.Count; i++)
            {
                if(mAllMessages[i].Thread_id != prevThreadId)
                {
                    resultMessages.Add(mAllMessages[i].Time, mAllMessages[i]);
                    prevThreadId = mAllMessages[i].Thread_id;
                }
            }

            var desc = resultMessages.Values.ToList();
            desc.Reverse();
            return desc;
        }

        public List<TextMessage> getMessages(int category)
        {
            string prevThreadId = "NULL";
            SortedList<long, TextMessage> resultMessages = new SortedList<long, TextMessage>();
            for (int i = 0; i < mAllMessages.Count; i++)
            {
                if (mAllMessages[i].Thread_id != prevThreadId)
                {
                    //if 연락처_카테고리 DB와 비교해서, 해당 문자의 카테고리가 int category와같으면 list에 넣고 똑같이 반환!
                    resultMessages.Add(mAllMessages[i].Time, mAllMessages[i]);
                    prevThreadId = mAllMessages[i].Thread_id;
                }
            }

            var desc = resultMessages.Values.ToList();
            desc.Reverse();
            return desc;
        }
    }
}