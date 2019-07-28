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

namespace LettreForAndroid.Utility
{
    class DateTimeUtillity
    {
        //public DateTime convertMilisecondsToDateTime(long milTime)
        //{
        //    TimeSpan time = TimeSpan.FromMilliseconds(milTime);
        //    DateTime startdate = new DateTime(1970, 1, 1) + time;
        //    return startdate;
        //}

        public int getCurrentYear()
        {
            return DateTime.Now.Year;
        }
        
        public int getYear(long milTime)
        {
            Java.Text.SimpleDateFormat formatter = new Java.Text.SimpleDateFormat("YYYY");
            string result = (string)formatter.Format(new Java.Sql.Timestamp(milTime));
            return Convert.ToInt32(result);
        }

        public long getCurrentMilTime()
        {
            DateTime baseDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(DateTime.Now.ToUniversalTime() - baseDate).TotalMilliseconds;
        }

        public string milisecondToDateTimeStr(long milTime, string pattern)
        {
            //날짜 표시
            //string pattern = "yyyy-MM-dd HH:mm:ss";
            Java.Text.SimpleDateFormat formatter = new Java.Text.SimpleDateFormat(pattern);
            string result = (string)formatter.Format(new Java.Sql.Timestamp(milTime));
            return result;
        }
    }
}