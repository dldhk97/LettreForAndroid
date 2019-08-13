using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using LettreForAndroid.UI;

namespace LettreForAndroid.Utility
{
    class NotificationHandler
    {
        public static void Notification(Context context, string channelID, string title, string msg, string ticker, int notifId)
        {
            var intent = new Intent(context, typeof(DialogueActivity));
            //intent.PutExtra("thread_id", _DialogueSet[iPosition].Thread_id);
            //intent.PutExtra("category", _DialogueSet.Lable);

            Android.Support.V4.App.NotificationCompat.Builder builder = new Android.Support.V4.App.NotificationCompat.Builder(context, channelID)
                .SetContentTitle(title)
                .SetContentText(msg)
                .SetTicker(ticker)
                .SetDefaults((int)NotificationDefaults.Sound)
                .SetVisibility((int)NotificationVisibility.Public)
                .SetPriority((int)NotificationPriority.Max)
                .SetVibrate(new long[0])
                //.SetFullScreenIntent(intent, true)
                .SetSmallIcon(Resource.Drawable.drawable_main_icon);

            NotificationManagerCompat notificationManager = NotificationManagerCompat.From(context);
            notificationManager.Notify(notifId, builder.Build());
        }

        //private int getNotificationIcon()
        //{
        //    bool useWhiteIcon = (Android.OS.Build.VERSION.SdkInt >= Android.OS.Build.VERSION_CODES.Lollipop);
        //    return useWhiteIcon ? Resource.dra.main_icon : R.drawable.ic_launcher;
        //}

    }
}