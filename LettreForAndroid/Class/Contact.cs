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

namespace LettreForAndroid.Class
{
    public class ContactData
    {
        private long id;
        private long rawContact_id;
        private long contact_id;
        private string address;
        private string name;
        private int isPrimary;                  //1이면 기본, 0이면 기본아님
        private string photoThumbnail_uri;

        public ContactData(long id, long rawContact_id, long contact_id, string address, string name, int isPrimary, string photoThumbnail_uri)
        {
            this.id = id;
            this.rawContact_id = rawContact_id;
            this.contact_id = contact_id;
            this.address = address;
            this.name = name;
            this.isPrimary = isPrimary;
            this.photoThumbnail_uri = photoThumbnail_uri;
        }

        public long Id
        {
            get { return id; }
        }
        public long RawContact_id
        {
            get { return rawContact_id; }
        }
        public long Contact_id
        {
            get { return contact_id; }
        }
        public string Address
        {
            get { return address; }
        }

        public string Name
        {
            get { return name; }
        }

        public int IsPrimary
        {
            get { return isPrimary; }
        }

        public string PhotoThumnail_uri
        {
            get { return photoThumbnail_uri; }
        }
    };

    public class RawContact
    {
        private long id;                        //RawConatct 테이블에서 primary Key
        private long contact_id;                
        private string account_name;            //이것으로 구글계정에 연동됬는지, SIM에 저장됬는지 알 수 있다.
        private string display_name;            //대표이름(Primary Name)
        private ContactData primaryContact;

        List<ContactData> contactDatas = new List<ContactData>();

        public RawContact(long id, long contact_id, string account_name, string display_name)
        {
            this.id = id;
            this.contact_id = contact_id;
            this.account_name = account_name;
            this.display_name = display_name;
        }

        public long Contact_id
        {
            get { return contact_id; }
        }

        public string Account_name
        {
            get { return account_name; }
        }

        public string Display_name
        {
            get { return display_name; }
        }

        public ContactData PrimaryContact
        {
            set { primaryContact = value; }
            get
            {
                if (primaryContact != null)
                    return primaryContact;
                else if (contactDatas.Count > 0)
                    return contactDatas[0];
                else
                    return null;
            }
        }

        public List<ContactData> ContactDatas
        {
            get { return contactDatas; }
        }

        public int Count
        {
            get { return contactDatas.Count; }
        }

    }

    public class Contact
    {
        private long contact_id;                          //contact_id
        private Dictionary<long, RawContact> rawContacts; //rawContact_id를 Key로 가짐
        private ContactData primaryContactData;

        public Contact(long contact_id)
        {
            this.contact_id = contact_id;
            rawContacts = new Dictionary<long, RawContact>();
        }

        public Dictionary<long, RawContact> RawContacts
        {
            get { return rawContacts; }
        }

        public int Count
        {
            get { return rawContacts.Count; }
        }

        public long Contact_id
        {
            get { return contact_id; }
        }

        public ContactData PrimaryContactData
        {
            get
            {
                foreach(RawContact objRaw in rawContacts.Values)
                {
                    if(objRaw.PrimaryContact != null)
                        return objRaw.PrimaryContact;
                }
                return null;
            }
        }
    }
}