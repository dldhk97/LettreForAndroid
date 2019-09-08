using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
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
        public static void ShowNotification(Context context, string channelID, string title, string msg, string address, string ticker, int notifId, int unreadCnt)
        {
            var intent = new Intent(context, typeof(DialogueActivity));
            intent.PutExtra("address", address);
            PendingIntent dialogueActivityIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.UpdateCurrent);

            NotificationCompat.Builder builder = new NotificationCompat.Builder(context, channelID)
                .SetContentTitle(title)
                .SetContentText(msg)
                .SetTicker(ticker)
                .SetDefaults((int)NotificationDefaults.Sound | (int)NotificationDefaults.Vibrate)
                .SetVisibility((int)NotificationVisibility.Public)
                .SetPriority((int)NotificationPriority.High)
                .SetVibrate(new long[] { 0, 1000, 500, 1000 })
                .SetFullScreenIntent(dialogueActivityIntent, true)
                .SetLargeIcon(BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.ic_notification))
                .SetSmallIcon(Resource.Drawable.ic_notification)
                .SetCategory(Notification.CategoryMessage)
                .SetAutoCancel(true)                                           //알림 클릭시 알림 아이콘이 상단바에서 사라짐.
                .SetNumber(unreadCnt)
                //.AddAction(Resource.Drawable.dd9_send_36dp_2x_2, "답장", dialogueActivityIntent)
                //.AddAction(Resource.Drawable.dd9_send_36dp_2x_2, "읽음", dialogueActivityIntent)
                .SetLights(Application.Context.Resources.GetColor(Resource.Color.colorPrimary), 1, 1);    //LED 표시

            //확장 메시지
            NotificationCompat.InboxStyle inboxStyle = new NotificationCompat.InboxStyle(builder);

            foreach (string elem in msg.Split('\n'))
            {
                inboxStyle.AddLine(elem);
            }
            inboxStyle.SetSummaryText("더 보기");
            builder.SetStyle(inboxStyle);

            NotificationManagerCompat notificationManager = NotificationManagerCompat.From(context);
            notificationManager.Notify(notifId, builder.Build());
        }

    }
}