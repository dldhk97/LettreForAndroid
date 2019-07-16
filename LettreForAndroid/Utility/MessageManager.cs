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

        public MessageManager(Activity activity)
        {
            mActivity = activity;
        }
        public void GetTextMessage()
        {
            //기본앱으로 지정되있고, 권한이 있으면?
            if (DataStorageManager.loadBoolData(mActivity.BaseContext, "isDefaultPackage", false))
            {
                List<TextMessage> lst = getAllTextMessages();
            }
            else
            {
                //기본앱으로 설정해야하는 이유를 알려주고 표시해라.
                //SetAsDefaultApp();
            }
        }

        //GetTextMessage
        public List<TextMessage> getAllTextMessages()
        {
            List<TextMessage> lstSms = new List<TextMessage>();
            TextMessage objSms = new TextMessage();
            Uri message = Uri.Parse("content://sms/");
            ContentResolver cr = mActivity.BaseContext.ContentResolver;

            ICursor c = cr.Query(message, null, null, null, null);
            mActivity.StartManagingCursor(c);
            int totalSMS = c.Count;

            if (c.MoveToFirst())
            {
                for (int i = 0; i < totalSMS; i++)
                {
                    objSms = new TextMessage();
                    objSms.Id = c.GetString(c.GetColumnIndexOrThrow("_id"));
                    objSms.Address = c.GetString(c.GetColumnIndexOrThrow("address"));
                    objSms.Msg = c.GetString(c.GetColumnIndexOrThrow("body"));
                    objSms.ReadState = c.GetString(c.GetColumnIndex("read"));
                    objSms.Time = c.GetString(c.GetColumnIndexOrThrow("date"));
                    if (c.GetString(c.GetColumnIndexOrThrow("type")).Contains("1"))
                    {
                        objSms.Folder = "inbox";
                    }
                    else
                    {
                        objSms.Folder = "sent";
                    }

                    lstSms.Add(objSms);
                    c.MoveToNext();
                }
            }
            // else {
            // throw new RuntimeException("You have no SMS");
            // }
            c.Close();

            return lstSms;
        }
    }
}