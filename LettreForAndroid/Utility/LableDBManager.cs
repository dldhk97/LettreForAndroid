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
            Load();                                                         //객체 생성시 메모리에 레이블 테이블 올림.
        }

        public static LableDBManager Get()
        {
            if (_Instance == null)
                _Instance = new LableDBManager();
            return _Instance;
        }

        //쓰레드 ID로 레이블을 찾는다. 현재 레이블 중 최대값을 반환함.
        public int GetLable(long thread_id)
        {
            Dialogue objdialogue = _OnMemoryLables[thread_id];
            int resultLable = objdialogue.Lables.Max();
            return resultLable;
        }

        //내장 DB에 단일 데이터 삽입 혹은 업데이트
        public void InsertOrUpdate(Dialogue dialogue)
        {
            Dialogue newDialogue = dialogue;

            //이미 thread_id가 존재하면
            if (_OnMemoryLables.DialogueList.ContainsKey(dialogue.Thread_id))
            {
                //기존 DB 데이터에, 새로받은 데이터 누적
                newDialogue = _OnMemoryLables[dialogue.Thread_id];
                for(int i = 1; i < Dialogue.Lable_COUNT - 1; i++)
                {
                    newDialogue.Lables[i] += dialogue.Lables[i];
                }
            }
            _Helper.InsertOrUpdate(Application.Context, newDialogue);
        }

        //내장 DB에서 레이블 테이블 불러옴
        private void Load()
        {
            _OnMemoryLables = _Helper.Load(Application.Context);
        }

        //전체 대화목록을 받아 그것을 바탕으로 DB를 만든다.
        public void CreateNewDB(DialogueSet dialogues)
        {
            List<string[]> toSendData = new List<string[]>();

            //대화목록에서 대화 각각을 살펴본다.
            foreach (Dialogue objDialogue in dialogues.DialogueList.Values)
            {
                //메시지 목록을 만들기 위해 Foreach로 메시지 각각 탐색
                foreach (TextMessage objMessage in objDialogue.TextMessageList)
                {
                    toSendData.Add(new string[] { objMessage.Address, objMessage.Msg });
                }

            }

            //모든 메시지가 toSendData 배열에 들어갔음. 이것을 서버로 전송하고, 결과값을 받는다.
            List<string[]> receivedData = NetworkManager.Get().SendAndReceiveData(toSendData);

            //받은 결과값들을 하나하나 DB에 넣는다.
            foreach(string[] objStr in receivedData)
            {
                Dialogue a = new Dialogue();
                a.Address = objStr[0];
                a.Lables[1] = Convert.ToInt32(objStr[0]); 
                a.Lables[2] = Convert.ToInt32(objStr[1]);
                a.Lables[3] = Convert.ToInt32(objStr[2]);
                a.Lables[4] = Convert.ToInt32(objStr[3]);
                a.Lables[5] = Convert.ToInt32(objStr[4]);
                a.Lables[6] = Convert.ToInt32(objStr[5]);

                a.Thread_id = MessageManager.Get().getThreadId(Application.Context, a.Address);

                InsertOrUpdate(a);
            }
        }

    }
}