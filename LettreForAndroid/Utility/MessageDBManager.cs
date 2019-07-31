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

                        //연락처에 있으면 대화로 분류
                        if (objDialogue.Contact != null)
                        {
                            objDialogue.DisplayName = objDialogue.Contact.Name;
                            objDialogue.MajorLable = (int)Dialogue.LableType.COMMON;
                        }
                        else
                        {
                            objDialogue.DisplayName = objSms.Address;
                            
                            //연락처에 없으면, Lable DB에 있는지, 없으면 미분류로 설정
                            int majorLable = LableDBManager.Get().GetMajorLable(objDialogue.Thread_id);
                            objDialogue.MajorLable = majorLable != -1 ? majorLable : (int)Dialogue.LableType.UNKNOWN;
                        }

                        _DialogueSets[objDialogue.MajorLable].Add(objDialogue);                           //알맞게 리스트에 추가
                        _DialogueSets[(int)Dialogue.LableType.ALL].Add(objDialogue);                      //전체 리스트에 추가

                        prevThreadId = objSms.Thread_id;
                    }
                    if (objSms.ReadState == "0")                                                          //읽지 않은 문자면, 카운트 추가
                        objDialogue.UnreadCnt++;

                    objDialogue.Add(objSms);
                }
            }
            cursor.Close();
        }

        public void Refresh()
        {
            Load();
            LableDBManager.Get().Load();
            Categorize();
        }

        //미분류에 저장된 대화를 레이블에 맞게 이동하여 탭에 넣음.
        //이것은 처음 어플 실행했을 때, 문자를 보냈을때, 받았을때 호출됨.
        public void Categorize()
        {
            //DB가 없으면 새로 만든다.
            if (!LableDBManager.Get().IsDBExist())
                LableDBManager.Get().CreateNewDB(_DialogueSets[(int)Dialogue.LableType.UNKNOWN]);

            List<Dialogue> deleteTarget = new List<Dialogue>();

            //미분류 대화들을 탐색.
            foreach(Dialogue objDialogue in _DialogueSets[(int)Dialogue.LableType.UNKNOWN].DialogueList.Values)
            {
                //서버에 보내서 결과값을 받는다.
                List<string[]> receivedData = NetworkManager.Get().GetLableFromServer(objDialogue);

                //서버에서 데이터 받았으면
                if(receivedData.Count > 0)
                {
                    //디버깅용 예외
                    if (receivedData[0][0] != objDialogue.Address)
                        throw new Exception("보낸 주소와 받은 주소가 다르다???");
                    
                    //미분류 목록에서 삭제할 대상으로 추가
                    deleteTarget.Add(objDialogue);

                    //레이블 누적하고 DB에 삽입
                    for (int i = 1; i < 7; i++)
                        objDialogue.Lables[i] += Convert.ToInt32(receivedData[0][i]);

                    int prevMajorLable = objDialogue.MajorLable;

                    objDialogue.MajorLable = objDialogue.Lables.ToList().IndexOf(objDialogue.Lables.Max());

                    LableDBManager.Get().InsertOrUpdate(objDialogue);

                    _DialogueSets[objDialogue.MajorLable].Add(objDialogue);                                 //이 대화를 알맞는 레이블에 추가
                }
            }

            foreach (Dialogue target in deleteTarget)
            {
                _DialogueSets[(int)Dialogue.LableType.UNKNOWN].DialogueList.Remove(target.Thread_id);           //미분류 대화 목록에서 삭제
            }
        }

        public long GetThreadId(string address)
        {
            return Telephony.Threads.GetOrCreateThreadId(Application.Context, address);      //make new Thread_id
        }
    }

    


}