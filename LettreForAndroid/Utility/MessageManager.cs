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
            string[] projection = new string[] {"_id", "address", "thread_id", "person", "body", "read", "date", "type" };  //SELECT 절에 해당함. DISTINCT는 반드시 배열 앞부분에 등장해야함.
            //string selectionClause = "address = ?";                //WHERE 절에 해당함
            //string[] selectionArgs = {"114"};                     //Selection을 지정했을 때 Where절에 해당하는 값들을 배열로 적어야댐.
            string sortOrder = "thread_id asc, date desc";                   //정렬조건
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
                        objDialogue = new Dialogue();                                                         //대화를 새로 만듬.
                        objDialogue.Contact = ContactManager.Get().getContactIdByPhoneNumber(objSms.Address); //연락처 가져와 저장

                        if (objDialogue.Contact != null)                                                      //연락처가 존재하면, 카테고리 1로 분류
                            objDialogue.Category = 1;
                        else
                            objDialogue.Category = 2;                              //DEBUG 임시로 2로 설정, 서버와 통신해서 카테고리 분류를 받는다.

                        mDialogueList.Add(objDialogue);                                                     //대화리스트에 추가

                        prevThreadId = objSms.Thread_id;
                    }
                    objDialogue.Add(objSms);
                    cursor.MoveToNext();
                }
            }
            // else {
            // throw new RuntimeException("You have no SMS");
            // }
            mActivity.StopManagingCursor(cursor);
            cursor.Close();

        }


        //카테고리별 대화내역 가져옴
        public List<Dialogue> getAllMessages(int category)
        {
            List<Dialogue> resultMessageList = new List<Dialogue>();
            for (int i = 0; i < mDialogueList.Count; i++)
            {
                Dialogue currentDialgoue = mDialogueList[i];

                if(category == (int)TabFrag.CATEGORY.ALL)           //전체 보기인 경우
                {
                    resultMessageList.Add(currentDialgoue);
                }
                else if (currentDialgoue.Category == category)     //카테고리가 동일하면 결과리스트에 추가
                {
                    resultMessageList.Add(currentDialgoue);
                }
            }

            resultMessageList.Sort(delegate (Dialogue A, Dialogue B)    //각 대화별로, 가장 최신 문자의 날짜별로 정렬
            {
                if (A[0].Time < B[0].Time) return 1;
                else if (A[0].Time > B[0].Time) return -1;
                return 0;
            });

            return resultMessageList;
        }

    }
}