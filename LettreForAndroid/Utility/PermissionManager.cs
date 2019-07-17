using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Support.V4.App;
using Android.Widget;

namespace LettreForAndroid.Utility
{
    public class PermissionManager
    {
        const int PERMISSION_ALL = 1;
        //Manifest.Permission.BroadcastSms, Manifest.Permission.BroadcastWapPush 는 안되는듯하다.
        public static string[] essentailPermissions = { Manifest.Permission.ReadSms, Manifest.Permission.ReceiveSms,  Manifest.Permission.SendSms, Manifest.Permission.WriteSms, Manifest.Permission.ReceiveMms, Manifest.Permission.ReadContacts, Manifest.Permission.WriteContacts, Manifest.Permission.Internet, Manifest.Permission.AccessNetworkState, Manifest.Permission.ReceiveWapPush };

        public static bool HasPermissions(Context iContext, string[] iPermissions)
        {
            if (iContext != null && iPermissions != null)
            {
                foreach (string permission in iPermissions)
                {
                    if (ActivityCompat.CheckSelfPermission(iContext, permission) != Permission.Granted)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public static void RequestEssentialPermission(Activity iActivtiy)
        {
            if (!HasPermissions(iActivtiy, essentailPermissions))
            {
                ActivityCompat.RequestPermissions(iActivtiy, essentailPermissions, PERMISSION_ALL);
            }

        }

        public static bool HasEssentialPermission(Activity iActivtiy)
        {
            return HasPermissions(iActivtiy, essentailPermissions);
        }
    }
}