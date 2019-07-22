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

    public class RecyclerItem
    {
        public enum TYPE { HEADER = 1, MESSAGE }
        public virtual int Type
        {
            get { return -1; }
        }
    }


    public class HeaderItem : RecyclerItem
    {
        string header;

        public HeaderItem(string iHeader)
        {
            header = iHeader;
        }

        public string Header
        {
            get { return header; }
            set { header = value; }
        }

        public override int Type
        {
            get { return (int)TYPE.HEADER; }
        }
    }

    public class MessageItem : RecyclerItem
    {
        TextMessage textMessage;

        public MessageItem(TextMessage iTextmessage)
        {
            textMessage = iTextmessage;
        }

        public TextMessage TextMessage
        {
            get { return textMessage; }
            set { textMessage = value; }
        }

        public override int Type
        {
            get { return (int)TYPE.MESSAGE; }
        }
    }

}