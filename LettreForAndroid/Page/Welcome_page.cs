using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Runtime;
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
        private Button mBtnSetDefault;
        private Button mBtnGetPermission;

        bool isPermissionGranted = false;
        bool isDefaultPackage = false;

        public event EventHandler<OnSignUpEventArgs> mOnSignUpComplete;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            this.Cancelable = false;

            var view = inflater.Inflate(Resource.Layout.welcome_page, container, false);

            mBtnGetPermission = view.FindViewById<Button>(Resource.Id.welcomepage_button1);
            mBtnSetDefault = view.FindViewById<Button>(Resource.Id.welcomepage_button2);

            mBtnGetPermission.Click += MBtnGetPermission_Click;
            mBtnSetDefault.Click += mBtnSetDefault_Click;

            if(Context.PackageName.Equals(Telephony.Sms.GetDefaultSmsPackage(Activity)))
            {
                isDefaultPackage = true;
                SetBtnDisable(mBtnSetDefault, "기본 앱으로 설정되었습니다.");
            }

            if(PermissionManager.HasEssentialPermission(Activity))
            {
                isPermissionGranted = true;
                SetBtnDisable(mBtnGetPermission, "모든 권한이 허가되었습니다.");
            }

            return view;
        }

        //기본앱으로 설정 버튼 클릭
        private void mBtnSetDefault_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(Telephony.Sms.Intents.ActionChangeDefault);
            intent.PutExtra(Telephony.Sms.Intents.ExtraPackageName, Context.PackageName);
            StartActivityForResult(intent, 0);

            isDefaultPackage = true;
            SetBtnDisable(mBtnSetDefault, "기본 앱으로 설정되었습니다.");
            CheckDismiss();
        }
        //권한 요청 버튼 클릭
        private void MBtnGetPermission_Click(object sender, EventArgs e)
        {
            PermissionManager.RequestEssentialPermission(Activity);

            isPermissionGranted = true;
            SetBtnDisable(mBtnGetPermission, "모든 권한이 허가되었습니다.");
            CheckDismiss();
        }
        //버튼을 클릭했다고 인식하여, 버튼을 비활성화 후 기본텍스트를 btnText로 치환
        private void SetBtnDisable(Button btn, string btnText)
        {
            btn.Enabled = false;
            btn.Text = btnText;
        }
        //welcompage를 닫아도 될지 말지 판단. 기본앱 설정과 권한승인이 모두 끝나있으면 창 닫음
        private void CheckDismiss()
        {
            if (isDefaultPackage && isPermissionGranted)
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
            //권한과 기본앱 설정이 이미 되있으면 창 내림.
            CheckDismiss();

        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        }

    }
}