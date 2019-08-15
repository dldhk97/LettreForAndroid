using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using LettreForAndroid.Class;
using LettreForAndroid.Utility;

namespace LettreForAndroid.UI
{
    [Activity(Label = "WelcomeActivity2", Theme = "@style/BasicTheme")]
    class WelcomeActivity2 : AppCompatActivity
    {
        private NonSwipeableViewPager _ViewPager;
        private Button _NextBtn;

        private enum WELCOME_SCREEN { WELCOME = 0, PRIVACY, PERMISSION, PACKAGE, CATEGORIZE, MACHINE };

        List<Screen> _Screens = new List<Screen>()
        {
            new Screen(Resource.Layout.welcome_screen, Resource.Drawable.main_icon_drawable_512, 
                Application.Context.Resources.GetString(Resource.String.welcome_screen1_primaryText),
                Application.Context.Resources.GetString(Resource.String.welcome_screen1_secondaryText),
                Application.Context.Resources.GetColor(Resource.Color.colorBackground_welcome1)),

            new Screen(Resource.Layout.welcome_screen, Resource.Drawable.privacy_icon,
                Application.Context.Resources.GetString(Resource.String.welcome_screen2_primaryText),
                Application.Context.Resources.GetString(Resource.String.welcome_screen2_secondaryText),
                Application.Context.Resources.GetColor(Resource.Color.colorBackground_welcome2)),

            new Screen(Resource.Layout.welcome_screen, Resource.Drawable.permission_icon,
                Application.Context.Resources.GetString(Resource.String.welcome_screen3_primaryText),
                Application.Context.Resources.GetString(Resource.String.welcome_screen3_secondaryText),
                Application.Context.Resources.GetColor(Resource.Color.colorBackground_welcome3)),

            new Screen(Resource.Layout.welcome_screen, Resource.Drawable.main_icon_drawable_512,
                Application.Context.Resources.GetString(Resource.String.welcome_screen4_primaryText),
                Application.Context.Resources.GetString(Resource.String.welcome_screen4_secondaryText),
                Application.Context.Resources.GetColor(Resource.Color.colorBackground_welcome4)),

            new Screen(Resource.Layout.welcome_screen, Resource.Drawable.categorize_icon,
                Application.Context.Resources.GetString(Resource.String.welcome_screen5_primaryText),
                Application.Context.Resources.GetString(Resource.String.welcome_screen5_secondaryText),
                Application.Context.Resources.GetColor(Resource.Color.colorBackground_welcome5)),

            new Screen(Resource.Layout.welcome_screen, Resource.Drawable.machine_icon,
                Application.Context.Resources.GetString(Resource.String.welcome_screen6_primaryText),
                Application.Context.Resources.GetString(Resource.String.welcome_screen6_secondaryText),
                Application.Context.Resources.GetColor(Resource.Color.colorBackground_welcome6)),
        };

        bool _IsFirst = true;
        bool _HasPermission = false;
        bool _IsDefaultPackage = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.WelcomeActivity2);

            //뷰페이저 설정
            _ViewPager = FindViewById<NonSwipeableViewPager>(Resource.Id.wa_viewpager);
            _NextBtn = FindViewById<Button>(Resource.Id.wa_nextBtn);

            WelcomeScreenAdapter adapter = new WelcomeScreenAdapter(this, _Screens);

            _ViewPager.Adapter = adapter;

            _NextBtn.Click += _NextBtn_Click;

            //처음이 아닌 경우
            _IsFirst = DataStorageManager.loadBoolData(this, "isFirst", true);
            if (_IsFirst == false)
            {
                if (PermissionManager.HasPermission(this, PermissionManager.essentialPermissions));
                    _HasPermission = true;

                if (PackageName.Equals(Telephony.Sms.GetDefaultSmsPackage(this)))
                    _IsDefaultPackage = true;

                if (_HasPermission == false)
                    _ViewPager.SetCurrentItem((int)WELCOME_SCREEN.PERMISSION, false);
                else if (_IsDefaultPackage == false)
                    _ViewPager.SetCurrentItem((int)WELCOME_SCREEN.PACKAGE, false);
                else
                    Finish();
            }

        }

        public override void OnBackPressed()
        {
            //뒤로가기를 눌렀을 때 아무 반응 하지 않음.
        }

        private void _NextBtn_Click(object sender, EventArgs e)
        {
            switch (_ViewPager.CurrentItem)
            {
                case (int)WELCOME_SCREEN.WELCOME:
                    _ViewPager.SetCurrentItem(_ViewPager.CurrentItem + 1, true);
                    break;
                case (int)WELCOME_SCREEN.PRIVACY:
                    _ViewPager.SetCurrentItem(_ViewPager.CurrentItem + 1, true);
                    break;
                case (int)WELCOME_SCREEN.PERMISSION:
                    GetEsentialPermissionAsync();
                    break;
                case (int)WELCOME_SCREEN.PACKAGE:
                    RequestSetAsDefaultAsync();
                    break;
                case (int)WELCOME_SCREEN.CATEGORIZE:
                    CreateLableDBAction();
                    break;
                case (int)WELCOME_SCREEN.MACHINE:
                    AskMachineSupport();
                    break;
            }
        }

        //---------------------------------------------------------------------------------
        // 권한 부여

        //권한 체크와 승인, 이후 기본앱 설정
        void GetEsentialPermissionAsync()
        {
            //권한이 이미 승인되어있다면
            if (PermissionManager.HasPermission(this, PermissionManager.essentialPermissions))
            {
                //처음왔거나, 기본앱설정도 해야된다면 계속 진행.
                if (_IsFirst || _IsDefaultPackage == false)
                    _ViewPager.SetCurrentItem(_ViewPager.CurrentItem + 1, true);
                else
                    Finish();
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
                        foreach (Permission grantResult in grantResults)
                        {
                            if (grantResult == Permission.Denied)
                            {
                                isAllGranted = false;
                                break;
                            }
                        }

                        if (isAllGranted)
                        {
                            Toast.MakeText(this, "권한이 승인되었습니다.", ToastLength.Short).Show();
                            //처음왔거나, 기본앱설정도 해야된다면 계속 진행. 권한만 설정해야되는 경우였다면 Finish.
                            if (_IsFirst || _IsDefaultPackage == false)
                                _ViewPager.SetCurrentItem(_ViewPager.CurrentItem + 1, true);
                            else
                                Finish();
                        }
                        else
                        {
                            Toast.MakeText(this, "권한이 거절당했습니다. 다시 시도해주세요.", ToastLength.Short).Show();
                        }
                    }
                    break;
            }
        }

        //---------------------------------------------------------------------
        // 기본앱 설정

        const int REQUEST_DEFAULTPACK = 2;

        async Task RequestSetAsDefaultAsync()
        {
            //기본앱으로 이미 지정이 되어있나?
            if (this.PackageName.Equals(Telephony.Sms.GetDefaultSmsPackage(this)))
            {
                //처음온거면 계속 진행, 기본앱이 풀려서 다시 설정한 것이라면 Finish
                if (_IsFirst)
                    _ViewPager.SetCurrentItem(_ViewPager.CurrentItem + 1, true);
                else
                    Finish();
                return;
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
            StartActivityForResult(intent, REQUEST_DEFAULTPACK);
        }

        protected override async void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            switch (requestCode)
            {
                case REQUEST_DEFAULTPACK:
                    if (this.PackageName.Equals(Telephony.Sms.GetDefaultSmsPackage(this)))
                    {
                        Toast.MakeText(this, "기본 앱으로 설정되었습니다.", ToastLength.Short).Show();
                        //처음온거면 계속 진행, 기본앱이 풀려서 다시 설정한 것이라면 Finish
                        if (_IsFirst)
                            _ViewPager.SetCurrentItem(_ViewPager.CurrentItem + 1, true);
                        else
                            Finish();
                    }
                    else
                    {
                        Toast.MakeText(this, "기본 앱으로 설정되지 않았습니다.\n다시 시도해주세요.", ToastLength.Short).Show();
                    }
                    break;
            }
        }

        //---------------------------------------------------------------------
        // 레이블 DB 생성 (서버 통신)

        private void CreateLableDBAction()
        {
            if (CreateLableDB())
            {
                Toast.MakeText(this, "메시지 분류가 완료되었습니다.", ToastLength.Short).Show();
                _ViewPager.SetCurrentItem(_ViewPager.CurrentItem + 1, true);
            }
            else
            {
                Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);
                builder.SetTitle("메시지 분류에 실패했습니다.");
                builder.SetMessage("다시 시도하시겠습니까?");
                builder.SetPositiveButton("예", (senderAlert, args) =>
                {
                    CreateLableDBAction();
                });
                builder.SetNegativeButton("아니오", (senderAlert, args) =>
                {
                    Android.Support.V7.App.AlertDialog.Builder builder2 = new Android.Support.V7.App.AlertDialog.Builder(this);
                    builder2.SetTitle("메시지 분류에 실패했습니다.");
                    builder2.SetMessage("메시지 분류를 나중에 하시겠습니까?");
                    builder2.SetPositiveButton("예", (senderAlert2, args2) =>
                    {
                        _ViewPager.SetCurrentItem(_ViewPager.CurrentItem + 1, true);
                    });
                    builder2.SetNegativeButton("아니오", (senderAlert2, args2) =>
                    {
                    });
                    Dialog dialog2 = builder2.Create();
                    dialog2.Show();
                });
                Dialog dialog = builder.Create();
                dialog.Show();
            }
        }

        private bool CreateLableDB()
        {
            //미분류 메시지가 하나도 없는 경우
            if (MessageDBManager.Get().DialogueSets[(int)Dialogue.LableType.UNKNOWN].Count <= 0)
                return true;

            //서버와 통신해서 Lable DB 생성 후 메모리에 올림.
            LableDBManager.Get().CreateLableDB(
            MessageDBManager.Get().DialogueSets[(int)Dialogue.LableType.UNKNOWN]);

            if (LableDBManager.Get().IsDBExist())
            {
                return true;
            }
            else
            {
                Toast.MakeText(this, "레이블 DB 생성에 실패했습니다.", ToastLength.Short).Show();
                return false;
            }
        }

        //---------------------------------------------------------------------
        // 기계학습 요청

        private void AskMachineSupport()
        {
            Android.Support.V7.App.AlertDialog.Builder builder2 = new Android.Support.V7.App.AlertDialog.Builder(this);
            builder2.SetTitle("기계학습 지원 요청");
            builder2.SetMessage("기계학습 지원을 하시겠습니까?");
            builder2.SetPositiveButton("예", (senderAlert2, args2) =>
            {
                Finish();
                DataStorageManager.saveBoolData(this, "isFirst", false);        //isFirst 해제
            });
            builder2.SetNegativeButton("아니오", (senderAlert2, args2) =>
            {
                Finish();
                DataStorageManager.saveBoolData(this, "isFirst", false);        //isFirst 해제
            });
            Dialog dialog2 = builder2.Create();
            dialog2.Show();
        }
    }

    //----------------------------------------------------------------------------------------------
    // UI, 뷰페이저

    public class NonSwipeableViewPager : ViewPager
    {
        public NonSwipeableViewPager(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer) { }
        public NonSwipeableViewPager(Context context, IAttributeSet attrs) : base(context, attrs) { }

        public override bool OnTouchEvent(MotionEvent e) { return false; }

        public override bool OnInterceptTouchEvent(MotionEvent ev) { return false; }
    }

    public class WelcomeScreenAdapter : PagerAdapter
    {
        Context _Context;
        List<Screen> _Screens;

        public WelcomeScreenAdapter(Context context, List<Screen> screens)
        {
            _Context = context;
            _Screens = screens;
        }

        public override int Count
        {
            get { return _Screens.Count; }
        }

        public override bool IsViewFromObject(View view, Java.Lang.Object obj)
        {
            return view == obj;
        }

        public override Java.Lang.Object InstantiateItem(View container, int position)
        {
            LayoutInflater inflater = LayoutInflater.From(_Context);
            View view = inflater.Inflate(Resource.Layout.welcome_screen, null);

            RelativeLayout rootRL = view.FindViewById<RelativeLayout>(Resource.Id.ws_rootRL);
            TextView primaryTV = view.FindViewById<TextView>(Resource.Id.ws_primaryTV);
            ImageView imageView = view.FindViewById<ImageView>(Resource.Id.ws_imageView);
            TextView secondaryTV = view.FindViewById<TextView>(Resource.Id.ws_secondaryTV);

            rootRL.SetBackgroundColor(_Screens[position].BackgroundColor);
            primaryTV.Text = _Screens[position].PrimaryText;
            imageView.SetImageResource(_Screens[position].Image);
            secondaryTV.Text = _Screens[position].SecondaryText;

            var viewPager = container.JavaCast<NonSwipeableViewPager>();
            viewPager.AddView(view);

            return view;
        }

        public override void DestroyItem(View container, int position, Java.Lang.Object view)
        {
            var viewPager = container.JavaCast<NonSwipeableViewPager>();
            viewPager.RemoveView(view as View);
        }
    }

    public class Screen
    {
        int layout;
        int image;
        string primaryText;
        string secondaryText;
        Android.Graphics.Color backgroundColor;

        public Screen(int layout, int image, string primaryText, string secondaryText, Android.Graphics.Color backgroundColor)
        {
            this.layout = layout;
            this.image = image;
            this.primaryText = primaryText;
            this.secondaryText = secondaryText;
            this.backgroundColor = backgroundColor;
        }

        public int Layout { get { return layout; } }
        public int Image { get { return image; } }
        public string PrimaryText { get { return primaryText; } }
        public string SecondaryText { get { return secondaryText; } }
        public Android.Graphics.Color BackgroundColor { get { return backgroundColor; } }

    }

}