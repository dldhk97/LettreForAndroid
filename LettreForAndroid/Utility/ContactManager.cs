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

using Uri = Android.Net.Uri;

namespace LettreForAndroid.Utility
{
    class Contact
    {
        private string id;
        private string phoneNumber;
        private string name;
        private string photo_id;

        public string Id
        {
            get { return id; }
            set { id = value; }
        }
        public string PhoneNumber
        {
            get { return phoneNumber; }
            set { phoneNumber = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Photo_id
        {
            get { return photo_id; }
            set { photo_id = value; }
        }
    };

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

        public string getContactbyPhoneNumber(string phoneNumber)
        {

            Uri uri = Uri.WithAppendedPath(ContactsContract.Contacts.ContentFilterUri, Uri.Encode(phoneNumber));
            string[] projection = { ContactsContract.Contacts.InterfaceConsts.DisplayName };

            ContentResolver cr = mActivity.BaseContext.ContentResolver;
            ICursor cursor = cr.Query(uri, projection, null, null, null);

            if (cursor == null)
            {
                return phoneNumber;
            }
            else
            {
                string name = phoneNumber;
                try
                {
                    if (cursor.MoveToFirst())
                    {
                        name = cursor.GetString(cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName));
                    }
                }
                finally
                {
                    cursor.Close();
                }

                return name;
            }
        }
    }
}