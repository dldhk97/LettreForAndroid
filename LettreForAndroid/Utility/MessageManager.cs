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
    //싱글톤 사용.
    //리사이클 뷰에서 메세지를 가져올때마다 만들면 너무 비효율적이라 판단하여 사용했음.
    //사용할때는 MessageManager.Get().refreshMessages()이런식으로 사용하면 됨.
    public class MessageManager
    {
        private static Activity mActivity;
        private static MessageManager mInstance = null;
        private static List<Dialogue> mDialogueList;

        public static MessageManager Get()
        {
            if (mInstance == null)
                mInstance = new MessageManager();
            return mInstance;
        }
        //activity가 있어야 하기 때문에 처음 한번만 이 메소드로 activity를 설정해줘야 함.
        public void Initialization(Activity iActivity)
        {
            mActivity = iActivity;
            //refreshMessages();
            refreshMessages();
        }

        public int Count
        {
            get { return mDialogueList.Count; }
        }

        //모든 문자메세지를 thread_id별로 묶어 mAllDialgoues에 저장
        public void refreshMessages()
        {
            mDialogueList = new List<Dialogue>();
            TextMessage objSms = new TextMessage();

            ContentResolver cr = mActivity.BaseContext.ContentResolver;

            Uri uri = Uri.Parse("content://sms/");
            string[] projection = new string[] { "DISTINCT thread_id", "_id", "address", "person", "body", "read", "date", "type" };  //SELECT 절에 해당함. DISTINCT는 반드시 배열 앞부분에 등장해야함.
            string selectionClause = "address = ?";                //WHERE 절에 해당함
            string[] selectionArgs = {"114"};                     //Selection을 지정했을 때 Where절에 해당하는 값들을 배열로 적어야댐.
            string sortOrder = "thread_id asc";                   //정렬조건
            ICursor cursor = cr.Query(uri, projection, null, null, sortOrder);

            mActivity.StartManagingCursor(cursor);
            int totalSMS = cursor.Count;

            if (cursor.MoveToFirst())
            {
                string prevThreadId = "NULL";
                Dialogue objDialogue = new Dialogue();
                for (int i = 0; i < totalSMS; i++)
                {
                    objSms = new TextMessage();
                    objSms.Id = cursor.GetString(cursor.GetColumnIndexOrThrow("_id"));
                    objSms.Address = cursor.GetString(cursor.GetColumnIndexOrThrow("address"));
                    objSms.Msg = cursor.GetString(cursor.GetColumnIndexOrThrow("body"));
                    objSms.ReadState = cursor.GetString(cursor.GetColumnIndex("read"));
                    objSms.Time = cursor.GetLong(cursor.GetColumnIndexOrThrow("date"));
                    objSms.Thread_id = cursor.GetString(cursor.GetColumnIndexOrThrow("thread_id"));
                    objSms.Type = cursor.GetString(cursor.GetColumnIndexOrThrow("type"));

                    //탐색한 메세지의 Thread_id가 이전과 다르다면
                    if(objSms.Thread_id != prevThreadId)
                    {
                        objDialogue = new Dialogue();               //대화를 새로 만듬.
                        mDialogueList.Add(objDialogue);
                        objDialogue.Thread_id = objSms.Thread_id;   //Thread_id 설정
                        objDialogue.Category = 1;                   //DEBUG 테스트로 1로 설정
                        prevThreadId = objDialogue.Thread_id;       //마지막 Thread_id로 설정
                    }
                    objDialogue.Add(objSms);                    //대화에 이번에 탐색한 메세지 추가
                    cursor.MoveToNext();
                }
            }
            // else {
            // throw new RuntimeException("You have no SMS");
            // }
            mActivity.StopManagingCursor(cursor);
            cursor.Close();

        }

        //public void refreshMessages2()
        //{
        //    mDialogueList = new List<Dialogue>();
        //    MultimediaMessage objMms = new MultimediaMessage();
        //    Uri message = Uri.Parse("content://mms/");
        //    ContentResolver cr = mActivity.BaseContext.ContentResolver;

        //    ICursor cursor = cr.Query(message, null, null, null, "thread_id asc, date desc");
        //    mActivity.StartManagingCursor(cursor);
        //    int totalMMS = cursor.Count;

        //    if (cursor.MoveToFirst())
        //    {
        //        string prevThreadId = "NULL";
        //        Dialogue objDialogue = new Dialogue();
        //        for (int i = 0; i < totalMMS; i++)
        //        {
        //            objMms = new MultimediaMessage();
        //            objMms.Id = cursor.GetString(cursor.GetColumnIndexOrThrow("_id"));
        //            //objSms.Address = cursor.GetString(cursor.GetColumnIndexOrThrow("address"));
        //            //objSms.Person = cursor.GetString(cursor.GetColumnIndexOrThrow("person"));     //현재 null값만 있는거같음
        //            //objSms.Msg = cursor.GetString(cursor.GetColumnIndexOrThrow("body"));
        //            objMms.ReadState = cursor.GetString(cursor.GetColumnIndex("read"));
        //            objMms.Time = cursor.GetLong(cursor.GetColumnIndexOrThrow("date"));
        //            objMms.Thread_id = cursor.GetString(cursor.GetColumnIndexOrThrow("thread_id"));
        //            objMms.Sub = cursor.GetString(cursor.GetColumnIndexOrThrow("sub"));
        //            objMms.Type = cursor.GetString(cursor.GetColumnIndexOrThrow("m_type"));
        //            objMms.Msg = cursor.GetString(cursor.GetColumnIndexOrThrow("msg_box"));
        //            //objSms.Folder = cursor.GetString(cursor.GetColumnIndexOrThrow("type"));

        //            //탐색한 메세지의 Thread_id가 이전과 다르다면
        //            if (objMms.Thread_id != prevThreadId)
        //            {
        //                objDialogue = new Dialogue();               //대화를 새로 만듬.
        //                mDialogueList.Add(objDialogue);
        //                objDialogue.Thread_id = objMms.Thread_id;   //Thread_id 설정
        //                objDialogue.Category = 1;                   //DEBUG 테스트로 1로 설정
        //                prevThreadId = objDialogue.Thread_id;       //마지막 Thread_id로 설정
        //            }
        //            objDialogue.Add(objMms);                    //대화에 이번에 탐색한 메세지 추가
        //            cursor.MoveToNext();
        //        }
        //        //mAllDialogues.Add(objDialogue);                 //이전 대화는 저장저장
        //    }
        //    // else {
        //    // throw new RuntimeException("You have no SMS");
        //    // }
        //    mActivity.StopManagingCursor(cursor);
        //    cursor.Close();
        //}


        //카테고리별 대화내역 가져옴
        public List<Dialogue> getAllMessages(int category)
        {
            string prevThreadId = "NULL";
            List<Dialogue> resultMessages = new List<Dialogue>();
            for (int i = 0; i < mDialogueList.Count; i++)
            {
                Dialogue currentDialgoue = mDialogueList[i];
                if (currentDialgoue.Category == category)     //카테고리가 동일하면
                {
                    resultMessages.Add(currentDialgoue);
                    prevThreadId = currentDialgoue.Thread_id;
                }
            }

            return resultMessages;
        }

        //특정 카테고리인 문자를 가져오는건데, category 0으로 설정하면 위에있는 getAllMessages와 합칠 수 있을듯.
        //public List<TextMessage> getMessages(int category)
        //{
        //    string prevThreadId = "NULL";
        //    SortedList<long, TextMessage> resultMessages = new SortedList<long, TextMessage>();
        //    for (int i = 0; i < mAllMessages.Count; i++)
        //    {
        //        if (mAllMessages[i].Thread_id != prevThreadId)
        //        {
        //            //if 연락처_카테고리 DB와 비교해서, 해당 문자의 카테고리가 int category와같으면 list에 넣고 똑같이 반환!
        //            resultMessages.Add(mAllMessages[i].Time, mAllMessages[i]);
        //            prevThreadId = mAllMessages[i].Thread_id;
        //        }
        //    }

        //    var desc = resultMessages.Values.ToList();
        //    desc.Reverse();
        //    return desc;
        //}
    }
}