﻿using System;
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

        private static ContactManager mInstance = null;
        private List<Contact> contactList = new List<Contact>();

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
        public void Initialization()
        {
            //refreshContacts();
        }

        public int Count
        {
            get { return contactList.Count; }
        }

        public Contact getContactByAddress(string address)
        {
            for(int i =0; i < contactList.Count; i++)
            {
                if(contactList[i].Address.Replace("-","") == address)
                {
                    return contactList[i];
                }
            }
            return null;
        }

        public void refreshContacts(Activity activity)
        {
            ContentResolver cr = activity.BaseContext.ContentResolver;

            string id_code = ContactsContract.CommonDataKinds.StructuredPostal.InterfaceConsts.Id;
            string phoneNumber_code = ContactsContract.CommonDataKinds.Phone.Number;
            string name_code = ContactsContract.Contacts.InterfaceConsts.DisplayName;
            string photoThumbnail_uri_code = ContactsContract.Contacts.InterfaceConsts.PhotoThumbnailUri;

            Uri uri = ContactsContract.CommonDataKinds.Phone.ContentUri;

            //string hipenAddress = Android.Telephony.PhoneNumberUtils.FormatNumber(address);    //하이픈 붙이기
            //address = address.Replace("-", "");                                                  //하이픈을 제거

            //SQLITE 조건문 설정
            string[] projection = { id_code, phoneNumber_code, name_code, photoThumbnail_uri_code };      //연락처 DB에서 ID, 번호, 이름, 사진을 빼냄.
            string selectionClause = "REPLACE( " + phoneNumber_code + ",'-' ,'')";       //?는 현재 찾고자 하는 값, phoneNumber_code에는 DB값이 들어간다.

            ICursor cursor = cr.Query(uri, projection, selectionClause, null, null);   //쿼리

            Contact result = null;

            if (cursor.MoveToFirst())
            {
                for(int i = 0; i < cursor.Count; i++)
                {
                    result = new Contact();
                    result.Id = cursor.GetString(cursor.GetColumnIndex(projection[0]));
                    result.Address = cursor.GetString(cursor.GetColumnIndex(projection[1]));
                    result.Name = cursor.GetString(cursor.GetColumnIndex(projection[2]));
                    result.PhotoThumnail_uri = cursor.GetString(cursor.GetColumnIndex(projection[3]));
                    contactList.Add(result);
                    cursor.MoveToNext();
                }
            }
        }

    }
}