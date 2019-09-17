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
        private const int _DB_VERSION = 2;
        private const string _TABLE_NAME = "DialogueTable";

        private static string[] _ColumnNameStr = { "thread_id", "address", "lable_common", "lable_delivery", "lable_card", "lable_identification", "lable_public", "lable_agency", "lable_etc" };
        private enum COLUMN_NAME { THREAD_ID = 0, ADDRESS, LABLE_COMMON, LABLE_DELIVERY, LABLE_CARD, LABLE_IDENTIFICATION, LABLE_PUBLIC, LABLE_AGENCY, LABLE_ETC };

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

        public string CreateQuery = "create table if not exists " + _TABLE_NAME + "("
            + _ColumnNameStr[(int)COLUMN_NAME.THREAD_ID] + " integer primary key, "                 //안드로이드 SQLite에서는 long 이 integer로 동작하는듯
            + _ColumnNameStr[(int)COLUMN_NAME.ADDRESS] + " text not null , "
            + _ColumnNameStr[(int)COLUMN_NAME.LABLE_COMMON] + " integer not null , "
            + _ColumnNameStr[(int)COLUMN_NAME.LABLE_DELIVERY] + " integer not null , "
            + _ColumnNameStr[(int)COLUMN_NAME.LABLE_CARD] + " integer not null , "
            + _ColumnNameStr[(int)COLUMN_NAME.LABLE_IDENTIFICATION] + " integer not null , "
            + _ColumnNameStr[(int)COLUMN_NAME.LABLE_PUBLIC] + " integer not null , "
            + _ColumnNameStr[(int)COLUMN_NAME.LABLE_AGENCY] + " integer not null , "
            + _ColumnNameStr[(int)COLUMN_NAME.LABLE_ETC] + " integer not null );";

        public const string DeleteQuery = "drop table if exists " + _TABLE_NAME;

        //DB에 대화가 있으면 교체, 없으면 삽입
        public void InsertOrUpdate(Context context, Dialogue dialogue)
        {
            SQLiteDatabase db = new MySQLiteOpenHelper(context).WritableDatabase;

            ContentValues values = new ContentValues();
            values.Put(_ColumnNameStr[(int)COLUMN_NAME.THREAD_ID], dialogue.Thread_id);
            values.Put(_ColumnNameStr[(int)COLUMN_NAME.ADDRESS], dialogue.Address);
            values.Put(_ColumnNameStr[(int)COLUMN_NAME.LABLE_COMMON], dialogue.Lables[0]);
            values.Put(_ColumnNameStr[(int)COLUMN_NAME.LABLE_DELIVERY], dialogue.Lables[1]);
            values.Put(_ColumnNameStr[(int)COLUMN_NAME.LABLE_CARD], dialogue.Lables[2]);
            values.Put(_ColumnNameStr[(int)COLUMN_NAME.LABLE_IDENTIFICATION], dialogue.Lables[3]);
            values.Put(_ColumnNameStr[(int)COLUMN_NAME.LABLE_PUBLIC], dialogue.Lables[4]);
            values.Put(_ColumnNameStr[(int)COLUMN_NAME.LABLE_AGENCY], dialogue.Lables[5]);
            values.Put(_ColumnNameStr[(int)COLUMN_NAME.LABLE_ETC], dialogue.Lables[6]);

            db.InsertWithOnConflict(_TABLE_NAME, null, values, Conflict.Replace);

            db.Close();
        }

        //모든 Lable 데이터를 DB에서 불러온다.
        public DialogueSet Load(Context context)
        {
            SQLiteDatabase db = new MySQLiteOpenHelper(context).ReadableDatabase;

            ICursor cursor = db.Query(_TABLE_NAME, new string[]
            {
                _ColumnNameStr[(int)COLUMN_NAME.THREAD_ID],
                _ColumnNameStr[(int)COLUMN_NAME.ADDRESS],
                _ColumnNameStr[(int)COLUMN_NAME.LABLE_COMMON],
                _ColumnNameStr[(int)COLUMN_NAME.LABLE_DELIVERY],
                _ColumnNameStr[(int)COLUMN_NAME.LABLE_CARD],
                _ColumnNameStr[(int)COLUMN_NAME.LABLE_IDENTIFICATION],
                _ColumnNameStr[(int)COLUMN_NAME.LABLE_PUBLIC],
                _ColumnNameStr[(int)COLUMN_NAME.LABLE_AGENCY],
                _ColumnNameStr[(int)COLUMN_NAME.LABLE_ETC],
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

                    for (int i = 0; i < Dialogue.Lable_COUNT; i++)   //DB의 2~8행까지 데이터를 0~6번 레이블에 저장
                        objDialogue.Lables[i] = cursor.GetInt(i + 2);
                    result.InsertOrUpdate(objDialogue);
                }
            }

            cursor.Close();
            db.Close();
            return result;
        }

        public void DropAndCreate(Context context)
        {
            SQLiteDatabase db = new MySQLiteOpenHelper(context).ReadableDatabase;
            db.ExecSQL(DeleteQuery);
            OnCreate(db);
        }
    }

    
}