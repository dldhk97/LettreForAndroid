using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using LettreForAndroid.Class;

using Uri = Android.Net.Uri;

namespace LettreForAndroid.Utility
{
    //Singleton
    class ContactDBManager
    {

        private static ContactDBManager _Instance = null;
        private List<Contact> _ContactList = new List<Contact>();

        //객체 생성시 DB에서 연락처 다 불러옴
        ContactDBManager()
        {
            Load();
        }

        public static ContactDBManager Get()
        {
            if (_Instance == null)
                _Instance = new ContactDBManager();
            return _Instance;
        }
        public Contact this[int i]
        {
            get { return _ContactList[i]; }
        }

        public int Count
        {
            get { return _ContactList.Count; }
        }

        public Contact getContactByAddress(string address)
        {
            for(int i =0; i < _ContactList.Count; i++)
            {
                if(_ContactList[i].Address.Replace("-","") == address)
                {
                    return _ContactList[i];
                }
            }
            return null;
        }

        private void Load()
        {
            ContentResolver cr = Application.Context.ContentResolver;

            string id_code = ContactsContract.CommonDataKinds.StructuredPostal.InterfaceConsts.Id;
            string phoneNumber_code = ContactsContract.CommonDataKinds.Phone.Number;
            string name_code = ContactsContract.Contacts.InterfaceConsts.DisplayName;
            string photoThumbnail_uri_code = ContactsContract.Contacts.InterfaceConsts.PhotoThumbnailUri;

            Uri uri = ContactsContract.CommonDataKinds.Phone.ContentUri;

            //SQLITE 조건문 설정
            string[] projection = { id_code, phoneNumber_code, name_code, photoThumbnail_uri_code };      //연락처 DB에서 ID, 번호, 이름, 사진을 빼냄.

            ICursor cursor = cr.Query(uri, projection, null, null, null);   //쿼리

            Contact result = null;

            if (cursor != null && cursor.Count > 0)
            {
                while(cursor.MoveToNext())
                {
                    result = new Contact();
                    result.Id = cursor.GetString(cursor.GetColumnIndex(projection[0]));
                    result.Address = cursor.GetString(cursor.GetColumnIndex(projection[1]));
                    result.Name = cursor.GetString(cursor.GetColumnIndex(projection[2]));
                    result.PhotoThumnail_uri = cursor.GetString(cursor.GetColumnIndex(projection[3]));
                    _ContactList.Add(result);
                }
            }
        }

    }
}