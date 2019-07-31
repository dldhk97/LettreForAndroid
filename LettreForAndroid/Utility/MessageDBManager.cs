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
                        objDialogue.MajorLable = 7;                                                         //일단 모두 미분류로 설정

                        objDialogue.DisplayName = objDialogue.Contact != null ? objDialogue.Contact.Name : objSms.Address;

                        if (objSms.ReadState == "0")                                                          //읽지 않은 문자면, 대화에 읽지않은 문자가 존재한다고 체크함.
                            objDialogue.UnreadCnt++;

                        _DialogueSets[(int)Dialogue.LableType.ALL].Add(objDialogue);                                                  //전체에 추가
                        _DialogueSets[(int)Dialogue.LableType.UNKNOWN].Add(objDialogue);                                                  //미분류에 추가

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

        //미분류에 저장된 대화를 레이블에 맞게 이동하여 탭에 넣음.
        //이것은 처음 어플 실행했을 때, 문자를 보냈을때, 받았을때 호출됨.
        public void Categorize()
        {
            if (!LableDBManager.Get().IsDBExist())
                LableDBManager.Get().CreateNewDB(_DialogueSets[(int)Dialogue.LableType.UNKNOWN]);

            List<Dialogue> deleteTarget = new List<Dialogue>();

            foreach(Dialogue objDialogue in _DialogueSets[(int)Dialogue.LableType.UNKNOWN].DialogueList.Values)
            {
                int majorLable = LableDBManager.Get().GetMajorLable(objDialogue.Thread_id);
                if(majorLable == -1)
                {
                    //DB에 레이블이 없음.
                    //서버랑 통신해보고, 서버랑 통신이 안되면 7번으로 분류
                    List<string[]> receivedData = NetworkManager.Get().GetLableFromServer(objDialogue);

                    //서버와 통신해서 레이블을 받았다면
                    if(receivedData != null)
                    {
                        //레이블 DB에 추가
                        Dialogue newDialogue = new Dialogue();
                        newDialogue.Address = receivedData[0][0];

                        for (int i = 1; i < 7; i++)
                            newDialogue.Lables[i] = Convert.ToInt32(receivedData[0][i]);

                        newDialogue.Thread_id = GetThreadId(newDialogue.Address);

                        LableDBManager.Get().InsertOrUpdate(newDialogue);

                        //메모리에서 세팅
                        deleteTarget.Add(objDialogue);                          //삭제대상에 추가
                        objDialogue.MajorLable = newDialogue.Lables.Max();
                        objDialogue.Lables = newDialogue.Lables;                //이거 완전복사 안되면 해주어야함.
                        _DialogueSets[objDialogue.MajorLable].Add(objDialogue);                                     //알맞는 레이블에 추가
                    }
                    else
                    {
                        //서버와 통신 실패시
                        //아무것도 안하나?
                    }
                    
                }
                else
                {
                    //내장 DB에서 레이블을 구했음.
                    deleteTarget.Add(objDialogue);                                                                //삭제대상에 추가

                    objDialogue.MajorLable = majorLable;
                    objDialogue.Lables = LableDBManager.Get().GetLables(objDialogue.Thread_id);
                    _DialogueSets[objDialogue.MajorLable].Add(objDialogue);                                     //알맞는 레이블에 추가
                }
            }

            foreach (Dialogue target in deleteTarget)
            {
                _DialogueSets[(int)Dialogue.LableType.UNKNOWN].DialogueList.Remove(target.Thread_id);           //미분류에서 삭제
            }
        }

        public long GetThreadId(string address)
        {
            return Telephony.Threads.GetOrCreateThreadId(Application.Context, address);      //make new Thread_id
        }
    }

    


}