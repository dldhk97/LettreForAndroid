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

namespace LettreForAndroid
{
    [Activity(Label = "welcome_page")]
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

        const string permission = Android.Manifest.Permission.ReadSms;

        public event EventHandler<OnWelcomeEventArgs> onWelcomeComplete;

        //이벤트 핸들러, mainAcitivity에서 WelcomePage의 역할이 끝난것을 알아차리기 위함.
        public class OnWelcomeEventArgs : EventArgs
        {
            public OnWelcomeEventArgs()
            {

            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.welcome_page);

            Button mBtnContinue = FindViewById<Button>(Resource.Id.welcomepage_button1);

            mBtnContinue.Click += async (sender, e) =>
            {
                //권한 체크와 승인, 이후 기본앱 설정
                await GetEsentialPermissionAsync();
            };
        }

        async Task GetEsentialPermissionAsync()
        {
            if (PermissionManager.HasEssentialPermission(this))
            {
                //권한이 이미 있으면 기본앱 체크
                Toast.MakeText(this, "권한이 이미 승인되어 있습니다.", ToastLength.Short).Show();
                DataStorageManager.saveBoolData(this, "isPermissionGranted", true);

                //기본앱 체크
                await RequestSetAsDefaultAsync();
                return;
            }

            //이미 한번 거절당한 경우.
            if (ShouldShowRequestPermissionRationale(permission))
            {
                //권한이 필요한 이유를 말해주고, OK누르면 요청 후 반환
                Snackbar.Make(Window.DecorView.RootView, "레뜨레 사용을 위해 승인을 눌러주세요.", Snackbar.LengthShort)
                        .SetAction("승인", v => RequestPermissions(essentailPermissions, REQUEST_ESSENTIAL_CALLBACK))
                        .Show();
                TextView hiddenGuideText = FindViewById<TextView>(Resource.Id.welcomepage_hiddenGuideText1);
                hiddenGuideText.Visibility = ViewStates.Visible;
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
                        if (grantResults[0] == Permission.Granted)
                        {
                            Toast.MakeText(this, "권한이 승인되었습니다.", ToastLength.Short).Show();
                            DataStorageManager.saveBoolData(this, "isPermissionGranted", true);

                            //기본앱 체크
                            await RequestSetAsDefaultAsync();
                        }
                        else
                        {
                            Toast.MakeText(this, "권한이 거절당했습니다. 레뜨레를 사용하시려면 버튼을 다시 눌러주세요!", ToastLength.Short).Show();
                            DataStorageManager.saveBoolData(this, "isPermissionGranted", false);
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
            //base.OnActivityResult(requestCode, resultCode, data);
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
                        DataStorageManager.saveBoolData(this, "isDefaultPackage", false);
                    }
                    break;
            }
        }

        private void backToTheMain()
        {
            //isDefaultPackage와 isPermissionGranted는 데이터스토리지 매니저가 관리하는게 아닌, 여기서 체크해야될듯.
            DataStorageManager.saveBoolData(this, "isDefaultPackage", true);
            DataStorageManager.saveBoolData(this, "isPermissionGranted", true);
            DataStorageManager.saveBoolData(this, "isFirst", false);
            Finish();
        }

        //public override void OnActivityCreated(Bundle savedInstanceState)
        //{
        //    if (Dialog != null)
        //    {
        //        Dialog.Window.RequestFeature(WindowFeatures.NoTitle);       //title bar을 투명으로
        //        base.OnActivityCreated(savedInstanceState);
        //        Dialog.Window.Attributes.WindowAnimations = Resource.Style.dialog_animation;    //animation 세팅
        //    }
        //}
    }
}