using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using System;

namespace LettreForAndroid.Utility
{
    public class PermissionManager
    {
        public enum REQUESTS { ESSENTIAL = 1, REQUEST_SENDSMS, }

        public static readonly string[] essentialPermissions= 
        {
            Manifest.Permission.SendSms,
            Manifest.Permission.ReceiveSms,
            Manifest.Permission.ReadSms,
            Manifest.Permission.ReceiveWapPush,
            Manifest.Permission.ReceiveMms,
            Manifest.Permission.ReadContacts,
            Manifest.Permission.WriteContacts
        };


        public static bool HasPermission(Context context, string permission)
        {
            return context.CheckSelfPermission(permission) == (int)Permission.Granted;
        }

        public static bool HasPermission(Context context, string[] permissions)
        {
            foreach (string permission in permissions)
            {
                if(context.CheckSelfPermission(permission) != (int)Permission.Granted)
                {
                    return false;
                }
            }
            return true;
        }

        public static void RequestPermission(Activity activity, string[] permissions, string reasonSnackTxt, int callBackCode)
        {
            //권한 목록 중 요청이 거절된 것이 있나?
            bool isNeedRationale = false;
            foreach (string permission in permissions)
            {
                if(activity.ShouldShowRequestPermissionRationale(permission))
                {
                    isNeedRationale = true;
                    break;
                }
            }

            //있다면 이유를 보여주고, 스낵바 표시
            if (isNeedRationale)
            {
                Snackbar.Make(activity.Window.DecorView.FindViewById(Android.Resource.Id.Content), reasonSnackTxt, Snackbar.LengthShort)
                        .SetAction("승인", v => activity.RequestPermissions(permissions, callBackCode))
                        .Show();
                Toast.MakeText(activity.ApplicationContext, "만약 '다시 묻지 않음' 을 체크하셨다면,\n어플리케이션 옵션에서 직접 권한을 승인해주셔야 합니다!", ToastLength.Long);
            }
            else
            {
                //없다면 권한 요청
                activity.RequestPermissions(permissions, callBackCode);
            }
        }

    }
}