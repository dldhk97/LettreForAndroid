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

    public class HeaderModel
    {
        string header;

        public string Header
        {
            get { return header; }
            set { header = value; }
        }

        public virtual bool isHeader()
        {
            return true;
        }
    }

    public class ChildModel : HeaderModel
    {
        TextMessage textMessage;

        public TextMessage TextMessage
        {
            get { return textMessage; }
            set { textMessage = value; }
        }

        public override bool isHeader()
        {
            return false;
        }
    }

}