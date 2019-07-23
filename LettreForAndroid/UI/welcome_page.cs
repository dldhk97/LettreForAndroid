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

namespace LettreForAndroid.UI
{
    [Activity(Label = "welcome_page", Theme = "@style/FadeInOutActivity")]
    public class welcome_page : Activity
    {
        //필수 퍼미션들
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

        const string permission_readSms = Android.Manifest.Permission.ReadSms;
        const string permission_readContacts = Android.Manifest.Permission.ReadContacts;

        Button mBtnContinue;
        TextView welcomepage_guidetext1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.welcome_page);

            mBtnContinue = FindViewById<Button>(Resource.Id.welcomepage_button1);
            welcomepage_guidetext1 = FindViewById<TextView>(Resource.Id.welcomepage_guidetext1);

            mBtnContinue.Click += async (sender, e) =>
            {
                //권한 체크와 승인, 이후 기본앱 설정
                await GetEsentialPermissionAsync();
            };
        }

        public override void OnBackPressed()
        {
            //뒤로가기를 눌렀을때 아무 반응 하지 않음.
        }




        async Task GetEsentialPermissionAsync()
        {
            if (PermissionManager.HasEssentialPermission(this))
            {
                //기본앱 체크
                await RequestSetAsDefaultAsync();
                return;
            }

            //이미 한번 거절당한 경우.
            if (ShouldShowRequestPermissionRationale(permission_readSms) || ShouldShowRequestPermissionRationale(permission_readContacts))
            {
                //권한이 필요한 이유를 말해주고, OK누르면 요청 후 반환
                Snackbar.Make(Window.DecorView.RootView, "레뜨레 사용을 위해 승인을 눌러주세요.", Snackbar.LengthShort)
                        .SetAction("승인", v => RequestPermissions(essentailPermissions, REQUEST_ESSENTIAL_CALLBACK))
                        .Show();
                welcomepage_guidetext1.Text = "만약 '다시 묻지 않음' 을 체크하셨다면,\n어플리케이션 옵션에서 직접 권한을 승인해주셔야 합니다!";
                return;
            }
            //권한 요청
            RequestPermissions(essentailPermissions, REQUEST_ESSENTIAL_CALLBACK);
        }

        //권한 요청 후 결과 받음.
        public override async void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            switch (requestCode)
            {
                case REQUEST_ESSENTIAL_CALLBACK:
                    {
                        bool isAllGranted = true;
                        foreach(Permission elem in grantResults)
                        {
                            if(elem == Permission.Denied)
                            {
                                isAllGranted = false;
                                break;
                            }
                        }

                        if (isAllGranted)
                        {
                            Toast.MakeText(this, "권한이 승인되었습니다.", ToastLength.Short).Show();

                            //기본앱 체크
                            await RequestSetAsDefaultAsync();
                        }
                        else
                        {
                            Toast.MakeText(this, "권한이 거절당했습니다. 레뜨레를 사용하시려면 버튼을 다시 눌러주세요!", ToastLength.Short).Show();
                        }
                    }
                    break;
            }
        }

        async Task RequestSetAsDefaultAsync()
        {
            if (this.PackageName.Equals(Telephony.Sms.GetDefaultSmsPackage(this)))
            {
                Toast.MakeText(this, "이미 기본 앱으로 설정되어 있습니다.", ToastLength.Short).Show();
                backToTheMain();
            }
            else
            {
                await SetAsDefaultAsync();
            }
        }

        async Task SetAsDefaultAsync()
        {
            Intent intent = new Intent(Telephony.Sms.Intents.ActionChangeDefault);
            intent.PutExtra(Telephony.Sms.Intents.ExtraPackageName, this.PackageName);
            StartActivityForResult(intent, REQUEST_DEFAULT_CALLBACK);
        }

        protected override async void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            switch (requestCode)
            {
                case REQUEST_DEFAULT_CALLBACK:
                    if (this.PackageName.Equals(Telephony.Sms.GetDefaultSmsPackage(this)))
                    {
                        Toast.MakeText(this, "기본 앱으로 설정되었습니다.", ToastLength.Short).Show();
                        backToTheMain();
                    }
                    else
                    {
                        Toast.MakeText(this, "기본 앱으로 설정되지 않았습니다. 기본 앱으로 지정해야 레뜨레를 사용할 수 있습니다.", ToastLength.Short).Show();
                    }
                    break;
            }
        }

        private void backToTheMain()
        {
            DataStorageManager.saveBoolData(this, "isFirst", false);

            //Finish 이후 문자 로딩하는데, 로딩하는동안 프로그레스바라도 띄우면 좋을듯
            welcomepage_guidetext1.Text = "잠시만 기다려주세요.";
            mBtnContinue.Enabled = false;
            Finish();
        }
    }
}