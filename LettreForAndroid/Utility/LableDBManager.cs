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
                int resultLable = objdialogue.Lables.Max();
                return resultLable;
            }
            return -1;
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
        public void Load()
        {
            _OnMemoryLables = _Helper.Load(Application.Context);
        }

        public bool IsDBExist()
        {
            return _OnMemoryLables.DialogueList.Count > 0;
        }

        //전체 대화목록을 받아 그것을 바탕으로 DB를 만든다.
        public void CreateNewDB(DialogueSet dialogues)
        {
            //대화가 하나도 없으면 그냥 놔둔다.
            if (dialogues.DialogueList.Count <= 0)
                return;

            List<string[]> receivedData = NetworkManager.Get().GetLablesFromServer(dialogues);

            //받은 결과값들을 하나하나 DB에 넣는다.
            foreach (string[] objStr in receivedData)
            {
                Dialogue newDialogue = new Dialogue();
                newDialogue.Address = objStr[0];

                for (int i = 1; i < 7; i++)
                    newDialogue.Lables[i] = Convert.ToInt32(objStr[i]);

                newDialogue.Thread_id = MessageDBManager.Get().GetThreadId(newDialogue.Address);

                InsertOrUpdate(newDialogue);
            }

            //DB를 메모리에 올림
            Load();
        }



    }
}