using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Database;
using Android.Database.Sqlite;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using LettreForAndroid.Class;

namespace LettreForAndroid.Utility
{
    public class MySQLiteOpenHelper : SQLiteOpenHelper
    {
        private const string _DB_NAME = "LettreDB";
        private const int _DB_VERSION = 1;
        private const string _TABLE_NAME = "DialogueTable";

        private const string COLUMN_THREAD_ID = "thread_id";
        private const string COLUMN_ADDRESS = "address";
        private const string COLUMN_Lable_COMMON = "lable_common";
        private const string COLUMN_Lable_DELIVERY = "lable_delivery";
        private const string COLUMN_Lable_CARD = "lable_card";
        private const string COLUMN_Lable_IDENTIFICATION = "lable_identification";
        private const string COLUMN_Lable_PUBLIC = "lable_public";
        private const string COLUMN_Lable_AGENCY = "lable_agency";
        //private const string COLUMN_Lable_SPAM = "lable_spam";

        public MySQLiteOpenHelper(Context context) : base(context, _DB_NAME, null, _DB_VERSION) { }

        public override void OnCreate(SQLiteDatabase db)
        {
            db.ExecSQL(CreateQuery);
        }

        public override void OnUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
        {
            db.ExecSQL(DeleteQuery);
            OnCreate(db);
        }

        public const string CreateQuery = "create table if not exists " + _TABLE_NAME + "("
            + COLUMN_THREAD_ID + " integer primary key, "                 //안드로이드 SQLite에서는 long 이 integer로 동작하는듯
            + COLUMN_ADDRESS + " text not null , "
            + COLUMN_Lable_COMMON + " integer not null , "
            + COLUMN_Lable_DELIVERY + " integer not null , "
            + COLUMN_Lable_CARD + " integer not null , "
            + COLUMN_Lable_IDENTIFICATION + " integer not null , "
            + COLUMN_Lable_PUBLIC + " integer not null , "
            + COLUMN_Lable_AGENCY + " integer not null ); ";
            //+ COLUMN_Lable_SPAM + " integer not null );";

        public const string DeleteQuery = "drop table if exists " + _TABLE_NAME;

        public void InsertOrUpdate(Context context, Dialogue dialogue)
        {
            SQLiteDatabase db = new MySQLiteOpenHelper(context).WritableDatabase;

            ContentValues values = new ContentValues();
            values.Put(COLUMN_THREAD_ID, dialogue.Thread_id);
            values.Put(COLUMN_ADDRESS, dialogue.Address);
            values.Put(COLUMN_Lable_COMMON, dialogue.Lables[1]);
            values.Put(COLUMN_Lable_DELIVERY, dialogue.Lables[2]);
            values.Put(COLUMN_Lable_CARD, dialogue.Lables[3]);
            values.Put(COLUMN_Lable_IDENTIFICATION, dialogue.Lables[4]);
            values.Put(COLUMN_Lable_PUBLIC, dialogue.Lables[5]);
            values.Put(COLUMN_Lable_AGENCY, dialogue.Lables[6]);
            //values.Put(COLUMN_Lable_SPAM, dialogue.Lables[7]);

            db.InsertWithOnConflict(_TABLE_NAME, null, values, Conflict.Replace);

            db.Close();
        }

        //모든 대화 메타데이터를 DB에서 불러온다.
        public DialogueSet Load(Context context)
        {
            SQLiteDatabase db = new MySQLiteOpenHelper(context).ReadableDatabase;

            ICursor cursor = db.Query(_TABLE_NAME, new string[]
            {
                COLUMN_THREAD_ID,
                COLUMN_ADDRESS,
                COLUMN_Lable_COMMON,
                COLUMN_Lable_DELIVERY,
                COLUMN_Lable_CARD,
                COLUMN_Lable_IDENTIFICATION,
                COLUMN_Lable_PUBLIC,
                COLUMN_Lable_AGENCY,
                //COLUMN_Lable_SPAM
            },
            null, null, null, null, null);

            DialogueSet result = null;
            if (cursor != null)
            {
                result = new DialogueSet();
                while(cursor.MoveToNext())
                {
                    Dialogue objDialogue = new Dialogue();
                    objDialogue.Thread_id = cursor.GetLong(0);
                    objDialogue.Address = cursor.GetString(1);
                    for(int i = 2; i < Dialogue.Lable_COUNT; i++)   //DB의 2~7행까지 데이터를 1~6번 레이블에 저장, 0번과 7번레이블은 사용안됨.
                        objDialogue.Lables[i-1] = cursor.GetInt(i);
                    result.Add(objDialogue);
                }
            }

            cursor.Close();
            db.Close();
            return result;
        }
    }

    
}