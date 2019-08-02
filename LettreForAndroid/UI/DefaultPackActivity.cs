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
    public class DefaultPackActivity : Activity
    {
        Button _BtnContinue;
        TextView _dpa_guidetext1;

        const int REQUEST_DEFAULT_CALLBACK = 2;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.DefaultPackActivity);

            _BtnContinue = FindViewById<Button>(Resource.Id.dpa_button1);
            _dpa_guidetext1 = FindViewById<TextView>(Resource.Id.dpa_guidetext1);

            _BtnContinue.Click += async (sender, e) =>
            {
                await GetEsentialPermissionAsync();
            };
        }

        public override void OnBackPressed()
        {
            //뒤로가기를 눌렀을때 아무 반응 하지 않음.
        }

        //권한 체크와 승인, 이후 기본앱 설정
        async Task GetEsentialPermissionAsync()
        {
            if (PermissionManager.HasPermission(this, PermissionManager.essentialPermissions))
            {
                //기본앱 체크
                await RequestSetAsDefaultAsync();
                return;
            }

            PermissionManager.RequestPermission(
                this, 
                PermissionManager.essentialPermissions, 
                "레뜨레 사용을 위해 승인을 눌러주세요.",
                (int)PermissionManager.REQUESTS.ESSENTIAL
                );
        }

        //권한 요청 후 결과가 나왔을 때
        public override async void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            switch (requestCode)
            {
                case (int)PermissionManager.REQUESTS.ESSENTIAL:
                    {
                        //모두 승인이 되었는지 확인
                        bool isAllGranted = true;
                        foreach(Permission grantResult in grantResults)
                        {
                            if(grantResult == Permission.Denied)
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
                        Toast.MakeText(this, "기본 앱으로 설정되지 않았습니다.\n기본 앱으로 지정해야 레뜨레를 사용할 수 있습니다.", ToastLength.Short).Show();
                    }
                    break;
            }
        }

        private void backToTheMain()
        {
            //DataStorageManager.saveBoolData(this, "isFirst", false);

            //Finish 이후 문자 로딩하는데, 로딩하는동안 프로그레스바라도 띄우면 좋을듯
            _dpa_guidetext1.Text = "잠시만 기다려주세요.";
            _BtnContinue.Enabled = false;
            Finish();
        }
    }
}