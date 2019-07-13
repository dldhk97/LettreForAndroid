using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;

using LettreForAndroid.Utility;

namespace LettreForAndroid.Page
{
    public class OnSignUpEventArgs : EventArgs
    {
        private string mFirstName;
        private string mEmail;
        private string mPassword;

        public string FirstName
        {
            get { return mFirstName; }
            set { mFirstName = value; }
        }
        public string Email
        {
            get { return mEmail; }
            set { mEmail = value; }
        }
        public string Password
        {
            get { return mPassword; }
            set { mPassword = value; }
        }

        public OnSignUpEventArgs(string firstName, string email, string password) : base()
        {
            FirstName = firstName;
            Email = email;
            Password = password;
        }
    }
    class welcome_page : DialogFragment
    {
        public readonly string[] essentailPermissions = 
            {
            Manifest.Permission.ReadSms,
            Manifest.Permission.ReceiveSms,
            Manifest.Permission.SendSms,
            Manifest.Permission.WriteSms,
            Manifest.Permission.ReceiveMms,
            Manifest.Permission.ReadContacts,
            Manifest.Permission.WriteContacts,
            Manifest.Permission.Internet,
            Manifest.Permission.AccessNetworkState,
            Manifest.Permission.ReceiveWapPush
            };
        const int REQUEST_ESSENTIAL_CALLBACK = 1;         //onRequestPermissionsResult에서 이 요청값으로 권한 획득 구분가능. INT형가능.
        const int REQUEST_DEFAULT_CALLBACK = 0;
        const string permission = Android.Manifest.Permission.ReadSms;

        private Button mBtnSetDefault;
        private Button mBtnGetPermission;

        public event EventHandler<OnSignUpEventArgs> mOnSignUpComplete;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            Cancelable = false;     //현재 페이지를 무시할 수 없음.

            var view = inflater.Inflate(Resource.Layout.welcome_page, container, false);

            mBtnGetPermission = view.FindViewById<Button>(Resource.Id.welcomepage_button1);
            mBtnSetDefault = view.FindViewById<Button>(Resource.Id.welcomepage_button2);

            mBtnGetPermission.Click += async (sender, e) =>
            {
                if (!PermissionManager.HasEssentialPermission(Activity)) //권한 없으면
                {
                    await GetEsentialPermissionAsync();
                }
                else
                {
                    //권한 있으면?
                    var snack = Snackbar.Make(View, "권한이 이미 승인되어 있습니다.", Snackbar.LengthShort);
                    snack.Show();
                }
                CheckDismiss();
            };

            mBtnSetDefault.Click += async (sender, e) =>
            {
                if(!Context.PackageName.Equals(Telephony.Sms.GetDefaultSmsPackage(Activity)))   //기본 앱이 아니면
                {
                    await RequestSetAsDefaultAsync();
                }
                else
                {
                    var snack = Snackbar.Make(View, "이미 기본앱으로 설정되어 있습니다.", Snackbar.LengthShort);
                    snack.Show();
                }
                CheckDismiss();
            };

            return view;
        }


        async Task GetEsentialPermissionAsync()
        {
            if (PermissionManager.HasEssentialPermission(Activity))
            {
                //권한이 이미 있으면 그냥 반환
                var snack = Snackbar.Make(View, "권한이 이미 승인되어있습니다.", Snackbar.LengthShort);
                snack.Show();
                return;
            }

            //need to request permission
            if (ShouldShowRequestPermissionRationale(permission))
            {
                //이미 한번 거절당한 경우.
                //권한이 필요한 이유를 말해주고, OK누르면 요청 후 반환
                Snackbar.Make(this.View, "승인 버튼을 눌러주면 권한 승인 메세지가 표시됩니다.", Snackbar.LengthIndefinite)
                        .SetAction("승인", v => RequestPermissions(essentailPermissions, REQUEST_ESSENTIAL_CALLBACK))
                        .Show();
                return;
            }
            //권한 요청
            RequestPermissions(essentailPermissions, REQUEST_ESSENTIAL_CALLBACK);
        }

        public override async void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            switch (requestCode)
            {
                case REQUEST_ESSENTIAL_CALLBACK:
                    {
                        if (grantResults[0] == Permission.Granted)
                        {
                            var snack = Snackbar.Make(View, "권한이 승인되었습니다.", Snackbar.LengthShort);
                            snack.Show();
                            DataStorageManager.saveBoolData(Context, "isPermissionGranted", true);
                        }
                        else
                        {
                            var snack = Snackbar.Make(View, "권한이 거절당했습니다. 레뜨레를 사용하시려면 권한 승인 버튼을 다시 눌러주세요! ", Snackbar.LengthLong);
                            snack.Show();
                            DataStorageManager.saveBoolData(Context, "isPermissionGranted", false);
                        }
                    }
                    break;
            }
        }

        async Task RequestSetAsDefaultAsync()
        {
            if(Context.PackageName.Equals(Telephony.Sms.GetDefaultSmsPackage(Activity)))
            {
                var snack = Snackbar.Make(View, "이미 기본 앱으로 설정되어 있습니다.", Snackbar.LengthShort);
                snack.Show();
                DataStorageManager.saveBoolData(Context, "isDefaultPackage", true);
            }
            else
            {
                await SetAsDefaultAsync();
            }
        }

        async Task SetAsDefaultAsync()
        {
            Intent intent = new Intent(Telephony.Sms.Intents.ActionChangeDefault);
            intent.PutExtra(Telephony.Sms.Intents.ExtraPackageName, Context.PackageName);
            StartActivityForResult(intent, REQUEST_DEFAULT_CALLBACK);
        }

        public override async void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            switch(requestCode)
            {
                case REQUEST_DEFAULT_CALLBACK:
                    if (Context.PackageName.Equals(Telephony.Sms.GetDefaultSmsPackage(Activity)))
                    {
                        var snack = Snackbar.Make(View, "기본 앱으로 설정되었습니다.", Snackbar.LengthShort);
                        snack.Show();
                        DataStorageManager.saveBoolData(Context, "isDefaultPackage", true);
                    }
                    else
                    {
                        var snack = Snackbar.Make(View, "기본 앱으로 설정되지 않았습니다. 다시 버튼을 눌러주세요.", Snackbar.LengthShort);
                        snack.Show();
                        DataStorageManager.saveBoolData(Context, "isDefaultPackage", false);
                    }
                    break;
            }
        }

        //welcompage를 닫아도 될지 말지 판단. 기본앱 설정과 권한승인이 모두 끝나있으면 창 닫음
        private void CheckDismiss()
        {
            if (DataStorageManager.loadBoolData(Context, "isDefaultPackage", false) && DataStorageManager.loadBoolData(Context, "isPermissionGranted", false))
            {
                Toast.MakeText(Context, "기본앱 설정과 권한이 승인되었습니다.", ToastLength.Short).Show();
                DataStorageManager.saveBoolData(Context, "isFirst", false);
                Dismiss();
            }
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            if(Dialog != null)
            {
                Dialog.Window.RequestFeature(WindowFeatures.NoTitle);       //title bar을 투명으로
                base.OnActivityCreated(savedInstanceState);
                Dialog.Window.Attributes.WindowAnimations = Resource.Style.dialog_animation;    //animation 세팅
            }
        }

    }
}