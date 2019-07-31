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

    public class MessageDBManager
    {
        private SmsManager _smsManager;

        private static MessageDBManager _Instance = null;
        private static List<DialogueSet> _DialogueSets;

        //객체 생성시 DB에서 문자 다 불러옴
        MessageDBManager()
        {
            _smsManager = SmsManager.Default;

            _DialogueSets = new List<DialogueSet>();
            for (int i = 0; i < Dialogue.Lable_COUNT; i++)
            {
                _DialogueSets.Add(new DialogueSet());
                _DialogueSets[i].Lable = i;                 //고유 레이블 붙여줌
            }

            Load();
        }

        public static MessageDBManager Get()
        {
            if (_Instance == null)
                _Instance = new MessageDBManager();
            return _Instance;
        }

        public List<DialogueSet> DialogueSets
        {
            get { return _DialogueSets; }
        }

        //모든 문자메세지를 대화로 묶어 _DialogueSet[0] = 전체에만 저장
        private void Load()
        {
            TextMessage objSms = new TextMessage();

            ContentResolver cr = Application.Context.ContentResolver;

            //DB 탐색 SQL문 설정
            Uri uri = Uri.Parse("content://sms/");
            string[] projection = new string[] {"_id", "address", "thread_id", "body", "read", "date", "type" };  //SELECT 절에 해당함. DISTINCT는 반드시 배열 앞부분에 등장해야함.
            string sortOrder = "thread_id asc, date desc";                                                                  //정렬조건
            ICursor cursor = cr.Query(uri, projection, null, null, sortOrder);

            //탐색 시작
            if (cursor != null && cursor.Count > 0)
            {
                long prevThreadId = -1;
                Dialogue objDialogue = new Dialogue();

                while(cursor.MoveToNext())
                {
                    objSms = new TextMessage();
                    objSms.Id = cursor.GetString(cursor.GetColumnIndexOrThrow("_id"));
                    string address = cursor.GetString(cursor.GetColumnIndexOrThrow("address"));
                    objSms.Address = address != "" ? address : "Unknown";
                    objSms.Msg = cursor.GetString(cursor.GetColumnIndexOrThrow("body"));
                    objSms.ReadState = cursor.GetString(cursor.GetColumnIndex("read"));
                    objSms.Time = cursor.GetLong(cursor.GetColumnIndexOrThrow("date"));
                    objSms.Thread_id = cursor.GetLong(cursor.GetColumnIndexOrThrow("thread_id"));
                    objSms.Type = cursor.GetInt(cursor.GetColumnIndexOrThrow("type"));

                    //탐색한 메세지의 Thread_id가 이전과 다르다면 새 대화임.
                    if(objSms.Thread_id != prevThreadId)
                    {
                        objDialogue = new Dialogue();
                        objDialogue.Contact = ContactDBManager.Get().getContactByAddress(objSms.Address);
                        objDialogue.Thread_id = objSms.Thread_id;
                        objDialogue.Address = objSms.Address;

                        objDialogue.DisplayName = objDialogue.Contact != null ? objDialogue.Contact.Name : objSms.Address;

                        if (objSms.ReadState == "0")                                                          //읽지 않은 문자면, 대화에 읽지않은 문자가 존재한다고 체크함.
                            objDialogue.UnreadCnt++;

                        _DialogueSets[0].Add(objDialogue);                                                  //전체에 추가

                        prevThreadId = objSms.Thread_id;
                    }
                    objDialogue.Add(objSms);
                }
            }
            cursor.Close();
        }

        public void Refresh()
        {
            Load();
            Categorize();
        }

        //_DialogueSet[0]에 저장된 대화를 레이블에 맞게 복사하여 탭에 넣음.
        public void Categorize()
        {
            foreach(Dialogue objdialogue in _DialogueSets[0].DialogueList.Values)
            {
                int majorLable = LableDBManager.Get().GetMajorLable(objdialogue.Thread_id);
                if(majorLable == -1)
                {
                    //DB에 레이블이 없음.
                    //서버랑 통신해보고, 서버랑 통신이 안되면 7번으로 분류
                    objdialogue.MajorLable = 7;
                }
                else
                {
                    objdialogue.MajorLable = majorLable;
                }
                objdialogue.Lables = LableDBManager.Get().GetLables(objdialogue.Thread_id);
                _DialogueSets[objdialogue.MajorLable].Add(objdialogue);

            }
        }

        public long GetThreadId(string address)
        {
            return Telephony.Threads.GetOrCreateThreadId(Application.Context, address);      //make new Thread_id
        }
    }

    


}