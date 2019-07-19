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
    class ContactManager
    {

        private static Activity mActivity;
        private static ContactManager mInstance = null;
        private List<Contact> contactList;

        public static ContactManager Get()
        {
            if (mInstance == null)
                mInstance = new ContactManager();
            return mInstance;
        }
        public Contact this[int i]
        {
            get { return contactList[i]; }
        }

        //activity가 있어야 하기 때문에 처음 한번만 이 메소드로 activity를 설정해줘야 함.
        public void Initialization(Activity iActivity)
        {
            mActivity = iActivity;
            //refreshContact();
        }

        public int Count
        {
            get { return contactList.Count; }
        }

        //연락처 DB에서 번호로 탐색함.
        public Contact getContactIdByPhoneNumber(string address)
        {
            ContentResolver cr = mActivity.BaseContext.ContentResolver;

            string id_code = ContactsContract.CommonDataKinds.StructuredPostal.InterfaceConsts.Id;
            string phoneNumber_code = ContactsContract.CommonDataKinds.Phone.Number;
            string name_code = ContactsContract.Contacts.InterfaceConsts.DisplayName;
            string photo_uri_code = ContactsContract.Contacts.InterfaceConsts.PhotoUri;

            Uri uri = ContactsContract.CommonDataKinds.Phone.ContentUri;

            //string hipenAddress = Android.Telephony.PhoneNumberUtils.FormatNumber(address);    //하이픈 붙이기
            address = address.Replace("-", "_");                                                 //하이픈을 _로 치환, _는 SQLite에서 임의의 값을 의미함.

            //SQLITE 조건문 설정
            string[] projection = {id_code, phoneNumber_code, name_code, photo_uri_code };      //연락처 DB에서 ID, 번호, 이름, 사진을 빼냄.
            string selectionClause = phoneNumber_code + " LIKE ?";                              //이 때 변수 탐색한 address_code행 값이
            string[] selectionArgs = { address };                                               //address가 반드시 포함된 것을 찾는다.

            ICursor cursor = cr.Query(uri, projection, selectionClause, selectionArgs, null);   //쿼리

            Contact result = null;

            if (cursor == null || cursor.Count == 0)
            {
                return result;
            }
            else
            {
                try
                {
                    if(cursor.MoveToFirst())
                    {
                        result = new Contact();
                        result.Id = cursor.GetString(cursor.GetColumnIndex(projection[0]));
                        result.Address = cursor.GetString(cursor.GetColumnIndex(projection[1]));
                        result.Name = cursor.GetString(cursor.GetColumnIndex(projection[2]));
                        result.Photo_uri = cursor.GetString(cursor.GetColumnIndex(projection[3]));
                    }
                }
                finally
                {
                    cursor.Close();
                }

                return result;
            }
        }


        //public void refreshContact()
        //{
        //    Uri uri = ContactsContract.CommonDataKinds.Phone.ContentUri;

        //    string[] projection = new string[] {
        //        ContactsContract.Contacts.InterfaceConsts.Id,            // 연락처 ID -> 사진 정보 가져오는데 사용
        //        ContactsContract.CommonDataKinds.Phone.Number,           // 번호
        //        ContactsContract.Contacts.InterfaceConsts.DisplayName,   // 이름.
        //        ContactsContract.Contacts.InterfaceConsts.PhotoId };     // 사진

        //    string[] selectionArgs = null;

        //    string sortOrder = ContactsContract.Contacts.InterfaceConsts.DisplayName
        //            + " COLLATE LOCALIZED ASC";

        //    ContentResolver cr = mActivity.BaseContext.ContentResolver;
        //    ICursor cursor = cr.Query(uri, projection, "DISTINCT", selectionArgs, sortOrder);

        //    contactList = new List<Contact>();

        //    if (cursor.MoveToFirst())
        //    {
        //        do
        //        {
        //            Contact contact = new Contact();
        //            contact.Id = cursor.GetString(0);
        //            contact.PhoneNumber = cursor.GetString(1);
        //            contact.Name = cursor.GetString(2);
        //            contact.Photo_id = cursor.GetString(3);
        //            contactList.Add(contact);

        //        } while (cursor.MoveToNext());
        //    }
        //}
    }
}