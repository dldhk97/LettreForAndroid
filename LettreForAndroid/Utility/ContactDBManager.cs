using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Telephony;
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
        private Dictionary<long, Contact> _ContactList = new Dictionary<long, Contact>();

        //객체 생성시 DB에서 연락처 다 불러옴
        ContactDBManager()
        {
            Refresh();
        }

        public Dictionary<long, Contact> ContactList
        {
            get { return _ContactList; }
        }

        public static ContactDBManager Get()
        {
            if (_Instance == null)
                _Instance = new ContactDBManager();
            return _Instance;
        }

        public int Count
        {
            get { return _ContactList.Count; }
        }

        public ContactData GetContactDataByAddress(string address, bool needRefresh)
        {
            if (needRefresh)
                Refresh();

            string formattedAddress = PhoneNumberUtils.FormatNumber(address);
            foreach (Contact objContact in _ContactList.Values)
            {
                string objAddress = objContact.PrimaryContactData.Address;
                objAddress = Regex.Replace(objAddress, "[^\\d]", "");
                string formattedObjAddress = PhoneNumberUtils.FormatNumber(objAddress);
                if (formattedAddress.CompareTo(formattedObjAddress) == 0)
                {
                    return objContact.PrimaryContactData;
                }
            }
            return null;
        }

        public void Refresh()
        {
            LoadRawContact();
            LoadContactData();
        }

        private void LoadContactData()
        {
            ContentResolver cr = Application.Context.ContentResolver;

            string column_id = ContactsContract.CommonDataKinds.StructuredPostal.InterfaceConsts.Id;
            string column_rawContact_id = ContactsContract.Contacts.InterfaceConsts.NameRawContactId;
            string column_contact_id = ContactsContract.CommonDataKinds.StructuredPostal.InterfaceConsts.ContactId;
            string column_address = ContactsContract.CommonDataKinds.Phone.Number;
            string column_name = ContactsContract.Contacts.InterfaceConsts.DisplayName;
            string column_isPrimary = ContactsContract.CommonDataKinds.StructuredPostal.InterfaceConsts.IsPrimary;
            string column_photoThumbnail_uri = ContactsContract.Contacts.InterfaceConsts.PhotoThumbnailUri;

            Uri uri = ContactsContract.CommonDataKinds.Phone.ContentUri;

            //SQLITE 조건문 설정
            string[] projection = { column_id, column_rawContact_id, column_contact_id, column_address, column_name, column_isPrimary, column_photoThumbnail_uri }; 

            ICursor cursor = cr.Query(uri, projection, null, null, null);   //쿼리

            if (cursor != null && cursor.Count > 0)
            {
                while(cursor.MoveToNext())
                {
                    long id = cursor.GetLong(cursor.GetColumnIndex(projection[0]));
                    long rawContact_id = cursor.GetLong(cursor.GetColumnIndex(projection[1]));
                    long contact_id = cursor.GetLong(cursor.GetColumnIndex(projection[2]));
                    string address = cursor.GetString(cursor.GetColumnIndex(projection[3]));
                    string name = cursor.GetString(cursor.GetColumnIndex(projection[4]));
                    int isPrimary = cursor.GetInt(cursor.GetColumnIndex(projection[5]));
                    string photoThumbnail_uri = cursor.GetString(cursor.GetColumnIndex(projection[6]));

                    ContactData objContact = new ContactData(id, rawContact_id, contact_id, address, name, isPrimary, photoThumbnail_uri);

                    //Android.Util.Log.Debug("Contact : ", id.ToString() + "," + rawContact_id.ToString() + "," + contact_id.ToString() + "," + address + "," + name);

                    if (isPrimary == 1)
                    {
                        _ContactList[contact_id].RawContacts[rawContact_id].PrimaryContact = objContact;
                    }

                    //_RawContactList[rawContact_id].Contacts.Add(objContact);
                    _ContactList[contact_id].RawContacts[rawContact_id].ContactDatas.Add(objContact);
                }
            }
            cursor.Close();

            //PrimaryContact가 없는 연락처는 제거한다.
            List<Contact> deleteTarget = new List<Contact>();
            foreach(Contact objContact in _ContactList.Values)
            {
                if(objContact.PrimaryContactData == null)
                {
                    deleteTarget.Add(objContact);
                }
            }
            foreach(Contact objContact in deleteTarget)
            {
                _ContactList.Remove(objContact.Contact_id);
            }
        }

        private void LoadRawContact()
        {
            if (_ContactList != null)
            {
                _ContactList = new Dictionary<long, Contact>();
            }

            ContentResolver cr = Application.Context.ContentResolver;

            Uri uri = ContactsContract.RawContacts.ContentUri;

            string column_id = ContactsContract.RawContacts.InterfaceConsts.Id;
            string column_contact_id = ContactsContract.RawContacts.InterfaceConsts.ContactId;
            string column_account_name = ContactsContract.RawContacts.InterfaceConsts.AccountName;
            string column_display_name = ContactsContract.RawContacts.InterfaceConsts.DisplayNamePrimary;

            //SQLITE 조건문 설정
            string[] projection = { column_id, column_contact_id, column_account_name, column_display_name,  };

            ICursor cursor = cr.Query(uri, projection, null, null, null);   //쿼리

            if (cursor != null && cursor.Count > 0)
            {
                while (cursor.MoveToNext())
                {
                    long id = cursor.GetLong(cursor.GetColumnIndex(projection[0]));
                    long contact_id = cursor.GetLong(cursor.GetColumnIndex(projection[1]));
                    string account_name = cursor.GetString(cursor.GetColumnIndex(projection[2]));
                    string display_name = cursor.GetString(cursor.GetColumnIndex(projection[3]));

                    //Contact가 신규면 생성
                    if(_ContactList.ContainsKey(contact_id) == false)
                    {
                        _ContactList.Add(contact_id, new Contact(contact_id));
                    }
                    _ContactList[contact_id].RawContacts.Add(id, new RawContact(id, contact_id, account_name, display_name));       //탐색한 RawContact삽입

                    //Android.Util.Log.Debug("Raw Contact : ", id.ToString() + "," + contact_id.ToString() + "," + account_name + "," + display_name);
                }
            }
            cursor.Close();
        }

    }
}