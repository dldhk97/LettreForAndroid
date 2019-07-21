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
    public class Contact
    {
        private string id;
        private string address;
        private string name;
        private string photoThumbnail_uri;

        public string Id
        {
            get { return id; }
            set { id = value; }
        }
        public string Address
        {
            get { return address; }
            set { address = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string PhotoThumnail_uri
        {
            get { return photoThumbnail_uri; }
            set { photoThumbnail_uri = value; }
        }
    };
}