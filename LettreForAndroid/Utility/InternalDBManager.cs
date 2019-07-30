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
    class InternalDBManager
    {
        private MySQLiteOpenHelper _Helper;
        private Context _Instance ;

        public InternalDBManager(Context context)
        {
            _Helper = new MySQLiteOpenHelper(context);
            _Instance = context;
        }

        //내장 DB에 thread_id, 레이블, 전화번호 저장
        public void InsertNewDialogue(Dialogue dialogue)
        {
            _Helper.InsertDialogue(_Instance, dialogue);
        }

        //내장 DB에서 thread_id로 탐색, 레이블(카테고리) 반환
        public DialogueSet LoadAllDialogues()
        {
            return _Helper.LoadAllDialogues(_Instance);
        }

        //모든 문자를 동기화
        //모든 문자를 읽고, 서버에 보내 lable을 받는다.
        public void SyncAllDialogue(DialogueSet dialogueSet)
        {

        }
    }
}