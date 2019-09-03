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

namespace LettreForAndroid.Utility
{
    public class DataStorageManager
    {
        const string DEFAULT_NAME = "Lettre";
        public static string loadStringData(Context context, string key, string defaultReturn)
        {
            //저장된 값을 불러오기 위해 같은 네임파일을 찾음.
            ISharedPreferences sf = context.GetSharedPreferences(DEFAULT_NAME, FileCreationMode.Private);
            //text라는 key에 저장된 값이 있는지 확인, 아무것도 없으면 defaultReturn 반환
            return sf.GetString(key, defaultReturn);
        }

        public static bool LoadBoolData(Context context, string key, bool defaultReturn)
        {
            ISharedPreferences sf = context.GetSharedPreferences(DEFAULT_NAME, FileCreationMode.Private);
            return sf.GetBoolean(key, defaultReturn);
        }
        public static int loadIntData(Context context, string key, int defaultReturn)
        {
            ISharedPreferences sf = context.GetSharedPreferences(DEFAULT_NAME, FileCreationMode.Private);
            return sf.GetInt(key, defaultReturn);
        }

        public static void saveStringData(Context context, string key, string value)
        {
            //Activity가 종료되기전에 저장
            ISharedPreferences sharedPreferences = context.GetSharedPreferences(DEFAULT_NAME, FileCreationMode.Private);

            //저장을 하기위해 editor을 이용해서 값 저장
            ISharedPreferencesEditor editor = sharedPreferences.Edit();
            editor.PutString(key, value);     //key, keyValue를 저장
            editor.Commit();        //커밋
        }
        public static void saveBoolData(Context context, string key, bool value)
        {
            ISharedPreferences sharedPreferences = context.GetSharedPreferences(DEFAULT_NAME, FileCreationMode.Private);

            ISharedPreferencesEditor editor = sharedPreferences.Edit();
            editor.PutBoolean(key, value);
            editor.Commit();
        }
        public static void saveIntData(Context context, string key, int value)
        {
            ISharedPreferences sharedPreferences = context.GetSharedPreferences(DEFAULT_NAME, FileCreationMode.Private);

            ISharedPreferencesEditor editor = sharedPreferences.Edit();
            editor.PutInt(key, value);
            editor.Commit();
        }
    }
}