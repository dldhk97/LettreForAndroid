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

        private void ResetDialogueSet()
        {
            if(_DialogueSets != null)
            {
                foreach (DialogueSet objSet in _DialogueSets)
                {
                    objSet.Clear();
                }
                _DialogueSets.Clear();
            }

            _DialogueSets = new List<DialogueSet>();
            for (int i = 0; i < Dialogue.Lable_COUNT; i++)
            {
                _DialogueSets.Add(new DialogueSet());
                _DialogueSets[i].Lable = i;                 //고유 레이블 붙여줌
            }
        }

        //모든 문자메세지를 대화로 묶어 _DialogueSet[0] = 전체에만 저장
        public void Load()
        {
            ResetDialogueSet();
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
                    objSms.ReadState = cursor.GetInt(cursor.GetColumnIndex("read"));
                    objSms.Time = cursor.GetLong(cursor.GetColumnIndexOrThrow("date"));
                    objSms.Thread_id = cursor.GetLong(cursor.GetColumnIndexOrThrow("thread_id"));
                    objSms.Type = cursor.GetInt(cursor.GetColumnIndexOrThrow("type"));

                    //탐색한 메세지의 Thread_id가 이전과 다르다면 새 대화임.
                    if(objSms.Thread_id != prevThreadId)
                    {
                        objDialogue = new Dialogue();
                        objDialogue.Contact = ContactDBManager.Get().getContactDataByAddress(objSms.Address);
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
                            //연락처에 없는 대화는, 레이블 분석에 따라 MajorLable이 변경될수 있으므로 여기서 MajorLable을 결정하지 않음. 미분류로 보냄.
                            objDialogue.DisplayName = objSms.Address;
                            objDialogue.MajorLable = (int)Dialogue.LableType.UNKNOWN;
                        }

                        _DialogueSets[objDialogue.MajorLable].InsertOrUpdate(objDialogue);                           //알맞게 리스트에 추가
                        _DialogueSets[(int)Dialogue.LableType.ALL].InsertOrUpdate(objDialogue);                      //전체 리스트에 추가

                        prevThreadId = objSms.Thread_id;
                    }
                    if (objSms.ReadState == (int)TextMessage.MESSAGE_READSTATE.UNREAD)                              //읽지 않은 문자면, 카운트 추가
                        objDialogue.UnreadCnt++;

                    objDialogue.Add(objSms);
                }
            }
            cursor.Close();

            foreach(DialogueSet dialgoueSet in _DialogueSets)
            {
                dialgoueSet.SortByLastMessageTime();
            }
        }

        public long GetThreadId(string address)
        {
            return Telephony.Threads.GetOrCreateThreadId(Application.Context, address);      //make new Thread_id
        }

        public void ChangeReadState(TextMessage msg, int readState)
        {
            ContentValues values = new ContentValues();
            values.Put("read", readState);

            ContentResolver cr = Application.Context.ContentResolver; 
            cr.Update(Uri.Parse("content://sms/"), values, "_id=" + msg.Id, null);
        }

        public void Refresh()
        {
            Load();
            CategorizeLocally(_DialogueSets[(int)Dialogue.LableType.ALL]);
        }
        
        //해당 대화집합이 모두 LableDB에 포함되어있다는 가정 하에 문자를 분류해서 메모리에 올림. 네트워크에 연결하지 않음.
        public void CategorizeLocally(DialogueSet dialogueSet)
        {
            List<Dialogue> unlabledTarget = new List<Dialogue>();                 //연락처에 없는 대화들은 모두 미분류 카테고리에 있음. 분류가 되면 미분류탭에서 삭제해야함.
            List<KeyValuePair<int, Dialogue>> lableChangedTarget = new List<KeyValuePair<int, Dialogue>>();          //분류가 변경된 대화들

            //대화들을 탐색. Lable DB에 있는지 확인함.
            foreach (Dialogue objDialogue in dialogueSet.DialogueList.Values)
            {
                //연락처가 있으면 대화이므로 패스
                if (objDialogue.Contact != null)
                    continue;

                int prevMajorLable = objDialogue.MajorLable;
                int majorLable = LableDBManager.Get().GetMajorLable(objDialogue.Thread_id);
                int[] lables = LableDBManager.Get().GetLables(objDialogue.Thread_id);

                if (majorLable == -1 || lables == null)                                 //레이블 DB에 없는것이 있다? -> 서버와 통신이 안됬을때 새 문자가 생긴 경우. 미분류로 처리
                {
                    continue;
                    //throw new Exception("레이블 DB에 없는 데이터가 있는데 CategorizeLocally 호출됨. 왜 호출됬는지 알아봐라");
                }
                    

                objDialogue.MajorLable = majorLable;                                    //대표 레이블 설정
                lables.CopyTo(objDialogue.Lables, 0);                                   //레이블 DB에서 레이블 배열을 메모리에 복사
                _DialogueSets[majorLable].InsertOrUpdate(objDialogue);                  //레이블에 맞는 셋에 대화 추가

                unlabledTarget.Add(objDialogue);                                  //미분류탭 삭제대상으로 선정

                if (prevMajorLable != majorLable)                                       //분류가 레이블 DB갱신으로 인해 변경된 경우 이전 탭에서 삭제대상으로 선정
                    lableChangedTarget.Add(new KeyValuePair<int, Dialogue>(prevMajorLable, objDialogue));
            }

            //미분류 탭에 남아있는 대화를 삭제
            foreach(Dialogue objDialogue in unlabledTarget)
            {
                _DialogueSets[(int)Dialogue.LableType.UNKNOWN].DialogueList.Remove(objDialogue.Thread_id);
            }

            //분류가 변경된 대화들은 이전 탭에서 삭제
            foreach(KeyValuePair<int, Dialogue> objDialogue in lableChangedTarget)
            {
                _DialogueSets[objDialogue.Key].DialogueList.Remove(objDialogue.Value.Thread_id);
            }
        }

        //어플이 실행했됬을 때, 네트워크에 다시 연결됬을 때 분류되지 않은 메시지가 있으면 그것을 분류함. 네트워크에 연결함.
        public void CategorizeNewMsg()
        {
            bool isNetworkConnected = true;
            foreach(Dialogue objDialogue in _DialogueSets[0].DialogueList.Values)
            {
                //연락처가 있는 대화는 분류하지 않는다.
                if (objDialogue.Contact != null)
                    continue;

                int[] objLables = LableDBManager.Get().GetLables(objDialogue.Thread_id);

                //레이블 DB에 이 대화가 없으면. 새 대화로 간주, 레이블DB에 새 값을 넣음.
                if (objLables == null)
                {
                    if (ReCategorize(objDialogue) == false)     //만약 서버 통신 실패면 중단
                    {
                        isNetworkConnected = false;
                        break;
                    }
                }
                else
                {
                    //이 대화에서 레이블이 붙여진 문자의 수를 센다.
                    int msgCnt = 0;
                    for (int i = 0; i < objLables.Length; i++)
                        msgCnt += objLables[i];
                    
                    //실제 존재하는 문자수와 레이블 붙여진 문자수가 다르면, 레이블이 붙여지지 않은 문자가 존재한다는 의미.
                    if(msgCnt != objDialogue.Count)
                    {
                        //신규메시지 존재하는 경우 이 대화를 다시 레이블 매김.
                        //한 메세지만 보내는게 아니라 대화 전체를 보내는데, 이는 어느 메세지가 분류되지 않은것인지 모르기 때문.
                        if (ReCategorize(objDialogue) == false) 
                        {
                            isNetworkConnected = false;
                            break;      //서버 통신 실패시 중단함.
                        }
                    }
                }
            }

            if(isNetworkConnected == false)
            {
                Toast.MakeText(Application.Context, "서버와 통신이 원할하지 않아 미분류된 문자가 존재합니다.", ToastLength.Long).Show();
            }
        }

        //서버로부터 이 대화의 레이블을 받고, 레이블 DB에 업데이트 한 뒤, 이 대화를 메모리에 올림
        public bool ReCategorize(Dialogue dialogue)
        {
            if (dialogue.Count <= 0)
                return true;

            int majorLable = LableDBManager.Get().GetMajorLable(dialogue.Thread_id);    //레이블 DB에 저장되있는 대표 레이블 가져옴 
            int[] lables = LableDBManager.Get().GetLables(dialogue.Thread_id);          //레이블 DB에 저장되있는 레이블들 가져옴

            LableDBManager.Get().UpdateLableDB(dialogue);
            majorLable = LableDBManager.Get().GetMajorLable(dialogue.Thread_id); //업데이트된 대표 레이블 가져옴
            lables = LableDBManager.Get().GetLables(dialogue.Thread_id);         //업데이트된 레이블 가져옴

            if (majorLable == -1 || lables == null)                             //서버 통신 실패면 아무것도 하지 않음.
                return false;

            dialogue.MajorLable = majorLable;                                    //대표 레이블 설정
            lables.CopyTo(dialogue.Lables, 0);                                   //레이블 DB에서 레이블 배열을 메모리에 복사
            _DialogueSets[majorLable].InsertOrUpdate(dialogue);                  //레이블에 맞는 셋에 대화 추가

            _DialogueSets[(int)Dialogue.LableType.UNKNOWN].DialogueList.Remove(dialogue.Thread_id); //미분류 탭에 남아있는 대화를 삭제함.
            return true;
        }

    }

    


}