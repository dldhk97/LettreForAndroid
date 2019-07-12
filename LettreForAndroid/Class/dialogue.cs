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
    public class Dialogue
    {
        private string address;
        List<Sms> messages;
    }

    public class Sms
    {
        private string id;
        private string address;
        private string msg;
        private string readState;   //"0" for have not read sms and "1" for have read sms
        private string time;
        private string folderName;

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
        public string Msg
        {
            get { return msg; }
            set { msg = value; }
        }
        public string ReadState
        {
            get { return readState; }
            set { readState = value; }
        }
        public string Time
        {
            get { return time; }
            set { time = value; }
        }
        public string FolderName
        {
            get { return folderName; }
            set { FolderName = value; }
        }
    }

    public class Mms : Sms
    {
        private string smil;
    }

}