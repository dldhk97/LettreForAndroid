using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android;
using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Telephony;
using Android.Views;
using Android.Widget;
using LettreForAndroid.Class;
using LettreForAndroid.Receivers;
using Uri = Android.Net.Uri;

namespace LettreForAndroid.Utility
{
    //싱글톤 사용.
    //리사이클 뷰에서 메세지를 가져올때마다 만들면 너무 비효율적이라 판단하여 사용했음.
    //사용할때는 MessageManager.Get().refreshMessages()이런식으로 사용하면 됨.

    public class MessageManager
    {
        private SmsManager _smsManager;

        private static MessageManager _Instance = null;
        private static List<DialogueSet> _DialogueSets;     //인덱스 = 카테고리인 총 문자 집합, 0번 인덱스는 비어있다.

        public static MessageManager Get()
        {
            if (_Instance == null)
                _Instance = new MessageManager();
            return _Instance;
        }

        public List<DialogueSet> DialogueSets
        {
            get { return _DialogueSets; }
        }

        public void Initialization()
        {
            _smsManager = SmsManager.Default;

            _DialogueSets = new List<DialogueSet>();
            for (int i = 0; i < Dialogue.Lable_COUNT; i++)
            {
                _DialogueSets.Add(new DialogueSet());
                _DialogueSets[i].Category = i;
            }
        }

        //모든 문자메세지를 thread_id별로 묶어 mAllDialgoues에 저장
        public void refreshMessages()
        {
            TextMessage objSms = new TextMessage();

            ContentResolver cr = Application.Context.ContentResolver;

            //DB 탐색 SQL문 설정
            Uri uri = Uri.Parse("content://sms/");
            string[] projection = new string[] {"_id", "address", "thread_id", "person", "body", "read", "date", "type" };  //SELECT 절에 해당함. DISTINCT는 반드시 배열 앞부분에 등장해야함.
            string sortOrder = "thread_id asc, date desc";                                                                  //정렬조건
            ICursor cursor = cr.Query(uri, projection, null, null, sortOrder);

            int totalSMS = cursor.Count;

            //탐색 시작
            if (cursor.MoveToFirst())
            {
                long prevThreadId = -1;
                Dialogue objDialogue = new Dialogue();

                for (int i = 0; i < totalSMS; i++)
                {
                    objSms = new TextMessage();
                    objSms.Id = cursor.GetString(cursor.GetColumnIndexOrThrow("_id"));
                    objSms.Address = cursor.GetString(cursor.GetColumnIndexOrThrow("address"));
                    if (objSms.Address == "")
                        objSms.Address = "Unknown";
                    objSms.Msg = cursor.GetString(cursor.GetColumnIndexOrThrow("body"));
                    objSms.ReadState = cursor.GetString(cursor.GetColumnIndex("read"));
                    objSms.Time = cursor.GetLong(cursor.GetColumnIndexOrThrow("date"));
                    objSms.Thread_id = cursor.GetLong(cursor.GetColumnIndexOrThrow("thread_id"));
                    objSms.Type = cursor.GetInt(cursor.GetColumnIndexOrThrow("type"));

                    //탐색한 메세지의 Thread_id가 이전과 다르다면 새 대화임.
                    if(objSms.Thread_id != prevThreadId)
                    {
                        objDialogue = new Dialogue();                                                         //대화를 새로 만듬.
                        objDialogue.Contact = ContactManager.Get().getContactByAddress(objSms.Address);
                        objDialogue.Thread_id = objSms.Thread_id;
                        objDialogue.Address = objSms.Address;

                        //카테고리 분류
                        if (objDialogue.Contact != null)                            //연락처에 있으면 대화로 분류
                        {
                            objDialogue.MajorLable = 1;
                            objDialogue.DisplayName = objDialogue.Contact.Name;
                        }
                        else
                        {
                            objDialogue.MajorLable = 2;                              //DEBUG 임시로 2로 설정, 서버와 통신해서 카테고리 분류를 받는다.
                            objDialogue.DisplayName = objSms.Address;
                            if (objSms.Address == "#CMAS#CMASALL")
                                objDialogue.DisplayName = "긴급 재난 문자";
                        }

                        if (objSms.ReadState == "0")                               //읽지 않은 문자면, 대화에 읽지않은 문자가 존재한다고 체크함.
                            objDialogue.UnreadCnt++;

                        _DialogueSets[objDialogue.MajorLable].Add(objDialogue);       //카테고리 알맞게 대화 집합에 추가
                        _DialogueSets[0].Add(objDialogue);                           //전체 카테고리에도 추가

                        prevThreadId = objSms.Thread_id;
                    }
                    objDialogue.Add(objSms);
                    cursor.MoveToNext();
                }
            }
            cursor.Close();

            //생성된 카테고리들 정렬
            for(int i = 0; i < Dialogue.Lable_COUNT; i++)
            {
                _DialogueSets[i].SortByLastMessageTime();
            }
        }

        public long getThreadId(Context context, string address)
        {
            return Telephony.Threads.GetOrCreateThreadId(context, address);      //make new Thread_id
        }
    }

    


}