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
        List<SMS> messages;
    }

    public class SMS
    {
        private string plainText;
    }

    public class MMS : SMS
    {
        private string smil;
    }

}