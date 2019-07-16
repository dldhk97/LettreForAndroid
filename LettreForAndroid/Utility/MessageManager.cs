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
    class MessageManager
    {
        readonly Activity mActivity;
        private List<TextMessage> mAllMessages;

        public List<TextMessage> AllMessages
        {
            get { return mAllMessages; }
        }

        //객체 생성 시 모든 문자메세지를 가져옴.
        public MessageManager(Activity iActivity)
        {
            mActivity = iActivity;
            refreshMessages();
        }

        private void refreshMessages()
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
                    objSms.Time = c.GetString(c.GetColumnIndexOrThrow("date"));
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
    }
}