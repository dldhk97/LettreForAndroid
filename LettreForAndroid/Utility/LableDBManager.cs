using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using LettreForAndroid.Class;

namespace LettreForAndroid.Utility
{
    class LableDBManager
    {
        private static LableDBManager _Instance;
        private MySQLiteOpenHelper _Helper;
        private DialogueSet _OnMemoryLables;

        public LableDBManager()
        {
            _Helper = new MySQLiteOpenHelper(Application.Context);
            Load();
        }

        public static LableDBManager Get()
        {
            if (_Instance == null)
                _Instance = new LableDBManager();
            return _Instance;
        }

        //쓰레드 ID로 레이블을 찾는다. 현재 레이블 중 최대값을 반환함.
        public int GetMajorLable(long thread_id)
        {
            if(_OnMemoryLables.DialogueList.ContainsKey(thread_id))
            {
                Dialogue objdialogue = _OnMemoryLables[thread_id];
                int maxValue = objdialogue.Lables.Max();
                int resultLable = objdialogue.Lables.ToList().IndexOf(maxValue);
                return resultLable;
            }
            return (int)Dialogue.LableType.UNKNOWN;
        }

        public int[] GetLables(long thread_id)
        {
            if (_OnMemoryLables.DialogueList.ContainsKey(thread_id))
            {
                Dialogue objdialogue = _OnMemoryLables[thread_id];
                return objdialogue.Lables;
            }
            return null;
        }

        //내장 DB에 단일 데이터 삽입 혹은 교체(업데이트)
        public void InsertOrUpdate(Dialogue dialogue)
        {
            _Helper.InsertOrUpdate(Application.Context, dialogue);
        }

        //내장 DB에서 레이블 테이블 불러옴
        public void Load()
        {
            _OnMemoryLables = _Helper.Load(Application.Context);
        }

        public bool IsDBExist()
        {
            return _OnMemoryLables.DialogueList.Count > 0;
        }


        public void CreateLableDB(DialogueSet dialogueSet)
        {
            //대화가 하나도 없으면 아무것도 하지 않음.
            if (dialogueSet.DialogueList.Count <= 0)
                return;

            CreateDBProgressEvent("서버에 전송 및 수신하는 중...[2/4]");
            List<string[]> receivedData = NetworkManager.Get().GetLablesFromServer(dialogueSet);                  //서버에서 데이터를 받는다.

            //서버 통신 실패시 아무것도 하지 않음.
            if (receivedData == null || receivedData.Count == 0)
                return;

            CreateDBProgressEvent("수신 성공. 레이블을 로컬 DB에 삽입하는 중...[3/4]");
            //받은 결과값들을 하나하나 DB에 넣는다.
            foreach (string[] objStr in receivedData)
            {
                Dialogue newDialogue = new Dialogue();
                newDialogue.Address = objStr[0];

                for (int i = 0; i < Dialogue.Lable_COUNT; i++)
                    newDialogue.Lables[i] = Convert.ToInt32(objStr[i + 1]);

                newDialogue.Thread_id = MessageDBManager.Get().GetThreadId(newDialogue.Address);

                InsertOrUpdate(newDialogue);
            }

            CreateDBProgressEvent("레이블을 메모리에 올리는 중...[4/4]");
            //DB를 메모리에 올림
            Load();
        }

        //서버와 통신하여 레이블을 갱신함.
        public void UpdateLableDB(Dialogue dialogue)
        {
            if (dialogue.Count <= 0)
                return;

            List<string[]> receivedData = NetworkManager.Get().GetLablesFromServer(dialogue);                  //서버에서 데이터를 받는다.

            //서버 통신 실패시 아무것도 하지 않음.
            if (receivedData == null)
                return;

            //받은 결과값들을 하나하나 DB에 넣는다.
            foreach (string[] objStr in receivedData)
            {
                Dialogue newDialogue = new Dialogue();
                newDialogue.Address = objStr[0];

                for (int i = 0; i < Dialogue.Lable_COUNT; i++)
                    newDialogue.Lables[i] = Convert.ToInt32(objStr[i + 1]);

                newDialogue.Thread_id = MessageDBManager.Get().GetThreadId(newDialogue.Address);

                InsertOrUpdate(newDialogue);
            }

            //DB를 메모리에 올림
            Load();
        }

        //새로 받은 문자를 서버에 보내 레이블을 받고, 이를 누적시킨다.
        public void AccumulateLableDB(TextMessage textMessage)
        {
            if (textMessage == null)
                return;

            List<string[]> receivedDatas = NetworkManager.Get().GetLableFromServer(textMessage);                  //서버에서 데이터를 받는다.

            //서버 통신 실패시 아무것도 하지 않음.
            if (receivedDatas == null)
                return;

            string[] receivedData = receivedDatas[0];

            Dialogue newDialogue = new Dialogue();

            newDialogue.Address = receivedData[0];
            newDialogue.Thread_id = MessageDBManager.Get().GetThreadId(newDialogue.Address);

            int[] dbLables = GetLables(newDialogue.Thread_id);                              //레이블 DB의 레이블을 가져옴.
            if (dbLables != null)
                dbLables.CopyTo(newDialogue.Lables, 0);

            for (int i = 0; i < Dialogue.Lable_COUNT; i++)
                newDialogue.Lables[i] += Convert.ToInt32(receivedData[i + 1]);                 //서버에서 받은 레이블을 누적.

            InsertOrUpdate(newDialogue);
            
            //DB를 메모리에 올림
            Load();
        }

        public delegate void CreateDBEventHandler(string toastMsg);
        public event CreateDBEventHandler CreateDBProgressEvent;

    }
}