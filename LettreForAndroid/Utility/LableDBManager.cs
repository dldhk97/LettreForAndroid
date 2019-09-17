using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            if (_OnMemoryLables == null)
                Load();

            if (_OnMemoryLables.DialogueList.ContainsKey(thread_id))
            {
                Dialogue objdialogue = _OnMemoryLables[thread_id];
                return objdialogue.Lables;
            }
            return null;
        }

        //내장 DB에 단일 데이터 삽입 혹은 교체(덮어씌움)
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
            if (_OnMemoryLables == null)
                return false;
            return _OnMemoryLables.DialogueList.Count > 0;
        }

        //WelcomeActivity에서 호출되는, 처음하는 레이블 분류.
        public void CreateLableDB(DialogueSet dialogueSet)
        {
            //대화가 하나도 없으면 아무것도 하지 않음.
            if (dialogueSet.DialogueList.Count <= 0)
                return;

            CreateDBProgressEvent("서버에 전송 및 수신하는 중...[2/4]");
            Dictionary<string, int[]> receivedData = NetworkManager.Get().GetLablesFromServer(dialogueSet);                  //서버에서 데이터를 받는다.

            //서버 통신 실패시 아무것도 하지 않음.
            if (receivedData == null || receivedData.Count == 0)
                return;

            CreateDBProgressEvent("수신 성공. 레이블을 로컬 DB에 삽입하는 중...[3/4]");
            //받은 결과값들을 하나하나 DB에 넣는다.
            foreach (KeyValuePair<string, int[]> data in receivedData)
            {
                Dialogue newDialogue = new Dialogue();
                newDialogue.Address = data.Key;

                for (int i = 0; i < Dialogue.Lable_COUNT; i++)
                    newDialogue.Lables[i] = data.Value[i];

                newDialogue.Thread_id = MessageDBManager.Get().GetThreadId(newDialogue.Address);

                InsertOrUpdate(newDialogue);
            }

            CreateDBProgressEvent("레이블을 메모리에 올리는 중...[4/4]");
            //DB를 메모리에 올림
            Load();
        }

		public void CreateLableDB_Offline(DialogueSet dialogueSet)
		{
			//대화가 하나도 없으면 아무것도 하지 않음.
			if (dialogueSet.DialogueList.Count <= 0)
				return;

			CreateDBProgressEvent("내장 분석기로 예측하는 중...[2/4]");

            //PredictionEngine을 통해 dialogue의 레이블을 예측
            PredictionEngine predEngine;
            try
            {
                predEngine = new PredictionEngine();
            }
			catch
            {
                CreateDBProgressEvent("[ERROR] 내장 분석기 오류 발생! 분류를 중단합니다. (PredictEngine)");
                return;
            }

            Dictionary<string, int[]> receivedData;
            
            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            try
            {
                receivedData = predEngine.Predict(dialogueSet);
            }
			catch(Exception e)
            {
                CreateDBProgressEvent("[ERROR] 내장 분석기 오류 발생! 분류를 중단합니다. (Predict)");
                return;
            }

			//sw.Stop();
			//Console.WriteLine("receivedData 예측 시간 : " + sw.ElapsedMilliseconds.ToString() + "ms");

			CreateDBProgressEvent("예측 성공. 레이블을 로컬 DB에 삽입하는 중...[3/4]");
			//받은 결과값들을 하나하나 DB에 넣는다.
			foreach (KeyValuePair<string, int[]> data in receivedData)
			{
				Dialogue newDialogue = new Dialogue();
				newDialogue.Address = data.Key;

				for (int i = 0; i < Dialogue.Lable_COUNT; i++)
					newDialogue.Lables[i] = data.Value[i];

				newDialogue.Thread_id = MessageDBManager.Get().GetThreadId(newDialogue.Address);

				InsertOrUpdate(newDialogue);
			}

			CreateDBProgressEvent("레이블을 메모리에 올리는 중...[4/4]");
			//DB를 메모리에 올림
			Load();
		}

		//서버와 통신하여 레이블을 갱신함(덮어씌움).
		public void UpdateLableDB(Dialogue dialogue)
        {
            if (dialogue.Count <= 0)
                return;

            Dictionary<string, int[]> receivedData = NetworkManager.Get().GetLablesFromServer(dialogue);                  //서버에서 데이터를 받는다.

            //서버 통신 실패시 아무것도 하지 않음.
            if (receivedData == null || receivedData.Count == 0)
                return;

            //받은 결과값들을 하나하나 DB에 넣는다.
            foreach (KeyValuePair<string, int[]> data in receivedData)
            {
                Dialogue newDialogue = new Dialogue();
                newDialogue.Address = data.Key;

                for (int i = 0; i < Dialogue.Lable_COUNT; i++)
                    newDialogue.Lables[i] = data.Value[i];

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

            if(_OnMemoryLables == null)
                Load();

            Dictionary<string, int[]> receivedDatas = NetworkManager.Get().GetLableFromServer(textMessage);                  //서버에서 데이터를 받는다.

            //서버 통신 실패시 아무것도 하지 않음.
            if (receivedDatas == null || receivedDatas.Count == 0)
                return;

            int[] receivedLable = receivedDatas[textMessage.Address];

            Dialogue newDialogue = new Dialogue();

            newDialogue.Address = textMessage.Address;
            newDialogue.Thread_id = MessageDBManager.Get().GetThreadId(newDialogue.Address);

            int[] dbLables = GetLables(newDialogue.Thread_id);                              //레이블 DB의 레이블을 가져옴.
            if (dbLables != null)
                dbLables.CopyTo(newDialogue.Lables, 0);

            for (int i = 0; i < Dialogue.Lable_COUNT; i++)
                newDialogue.Lables[i] += receivedLable[i];                 //서버에서 받은 레이블을 누적.

            InsertOrUpdate(newDialogue);
            
            //레이블 DB를 메모리에 올림
            Load();
        }

        public delegate void CreateDBEventHandler(string toastMsg);
        public event CreateDBEventHandler CreateDBProgressEvent;

    }
}