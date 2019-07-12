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
    class TabFrag
    {
        private string tabTitle;
        private int notiCount;

        public TabFrag()
        {
            tabTitle = "NULL";
            notiCount = -1;
        }
        public TabFrag(string title)
        {
            tabTitle = title;
            notiCount = -1;
        }
        public TabFrag(string title, int count)
        {
            tabTitle = title;
            notiCount = count;
        }
        public string TabTitle
        {
            get { return tabTitle; }
            set { tabTitle = value; }
        }
        public int NotiCount
        {
            get { return notiCount; }
            set { notiCount = value; }
        }

    }
}