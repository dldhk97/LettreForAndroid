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
    class welcome_page : DialogFragment
    {
        public class OnWelcomeEventArgs : EventArgs
        {
            public OnWelcomeEventArgs()
            {

            }
        }
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

        TextView hiddenGuideText;

        public event EventHandler<OnWelcomeEventArgs> onWelcomeComplete;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            Cancelable = false;     //현재 페이지를 무시할 수 없음.

            var view = inflater.Inflate(Resource.Layout.welcome_page, container, false);

            Button mBtnContinue = view.FindViewById<Button>(Resource.Id.welcomepage_button1);
            hiddenGuideText = view.FindViewById<TextView>(Resource.Id.welcomepage_hiddenGuideText1);

            mBtnContinue.Click += async (sender, e) =>
            {
                //권한 체크와 승인, 이후 기본앱 설정
                await GetEsentialPermissionAsync();
            };
            return view;
        }


        async Task GetEsentialPermissionAsync()
        {
            if (PermissionManager.HasEssentialPermission(Activity))
            {
                //권한이 이미 있으면 기본앱 체크
                Toast.MakeText(Context, "권한이 이미 승인되어 있습니다.", ToastLength.Short).Show();
                DataStorageManager.saveBoolData(Context, "isPermissionGranted", true);

                //기본앱 체크
                await RequestSetAsDefaultAsync();
                return;
            }

            //이미 한번 거절당한 경우.
            if (ShouldShowRequestPermissionRationale(permission))
            {
                //권한이 필요한 이유를 말해주고, OK누르면 요청 후 반환
                Snackbar.Make(this.View, "레뜨레 사용을 위해 승인을 눌러주세요.", Snackbar.LengthShort)
                        .SetAction("승인", v => RequestPermissions(essentailPermissions, REQUEST_ESSENTIAL_CALLBACK))
                        .Show();
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
                            Toast.MakeText(Context, "권한이 승인되었습니다.", ToastLength.Short).Show();
                            DataStorageManager.saveBoolData(Context, "isPermissionGranted", true);
                            
                            //기본앱 체크
                            await RequestSetAsDefaultAsync();
                        }
                        else
                        {
                            Toast.MakeText(Context, "권한이 거절당했습니다. 레뜨레를 사용하시려면 버튼을 다시 눌러주세요!", ToastLength.Short).Show();
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
                Toast.MakeText(Context, "이미 기본 앱으로 설정되어 있습니다.", ToastLength.Short).Show();
                DataStorageManager.saveBoolData(Context, "isDefaultPackage", true);
                CheckDismiss();
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
                        Toast.MakeText(Context, "기본 앱으로 설정되었습니다.", ToastLength.Short).Show();
                        DataStorageManager.saveBoolData(Context, "isDefaultPackage", true);
                        CheckDismiss();
                    }
                    else
                    {
                        Toast.MakeText(Context, "기본 앱으로 설정되지 않았습니다. 기본 앱으로 지정해야 레뜨레를 사용할 수 있습니다.", ToastLength.Short).Show();
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
                Toast.MakeText(Context, "이제 레뜨레를 사용하실 수 있습니다!", ToastLength.Short).Show();
                DataStorageManager.saveBoolData(Context, "isFirst", false);
                onWelcomeComplete.Invoke(this, new OnWelcomeEventArgs());
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