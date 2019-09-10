using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
    [Activity(Label = "WelcomeActivity", Theme = "@style/BasicTheme")]
    class WelcomeActivity : AppCompatActivity
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

        Task _MessageDBLoadTsk;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.WelcomeActivity);

            //뷰페이저 설정
            _ViewPager = FindViewById<NonSwipeableViewPager>(Resource.Id.wa_viewpager);
            _NextBtn = FindViewById<Button>(Resource.Id.wa_nextBtn);

            WelcomeScreenAdapter adapter = new WelcomeScreenAdapter(this, _Screens);
            _ViewPager.Adapter = adapter;
            _NextBtn.Click += _NextBtn_Click;

            //처음이 아닌 경우
            _IsFirst = DataStorageManager.LoadBoolData(this, "isFirst", true);
            if (_IsFirst == false)
            {
                //권한이 있는가?
                if (PermissionManager.HasPermission(this, PermissionManager.essentialPermissions));
                    _HasPermission = true;

                //기본 패키지로 되어있는가?
                if (PackageName.Equals(Telephony.Sms.GetDefaultSmsPackage(this)))
                    _IsDefaultPackage = true;

                if (_HasPermission == false)                                            //권한이 없다면, 권한페이지로 이동.
                    _ViewPager.SetCurrentItem((int)WELCOME_SCREEN.PERMISSION, false);
                else if (_IsDefaultPackage == false)                                    //기본 패키지가 아니라면, 기본앱 설정 페이지로 이동.
                    _ViewPager.SetCurrentItem((int)WELCOME_SCREEN.PACKAGE, false);
                else                                                                    //이미 설정 다되있으면 피니쉬
                    Finish();
            }
        }

        public override void OnBackPressed()
        {
            //뒤로가기를 눌렀을 때 아무 반응 하지 않음.
        }

        private void _NextBtn_Click(object sender, EventArgs e)
        {
            RunOnUiThread(() => { _NextBtn.Clickable = false; });
            switch (_ViewPager.CurrentItem)
            {
                case (int)WELCOME_SCREEN.WELCOME:
                    MoveToNextScreen();
                    break;
                case (int)WELCOME_SCREEN.PRIVACY:
                    MoveToNextScreen();
                    break;
                case (int)WELCOME_SCREEN.PERMISSION:
                    GetEsentialPermissionAsync();
                    break;
                case (int)WELCOME_SCREEN.PACKAGE:
                    RequestSetAsDefault();
                    break;
                case (int)WELCOME_SCREEN.CATEGORIZE:
                    AskOfflineMode();
                    break;
                case (int)WELCOME_SCREEN.MACHINE:
                    AskMachineSupport();
                    break;
            }
        }

        private void MoveToNextScreen()
        {
            RunOnUiThread(() => 
            {
                _ViewPager.SetCurrentItem(_ViewPager.CurrentItem + 1, true);
                _NextBtn.Clickable = true;
            });
        }

        //---------------------------------------------------------------------------------
        // 권한 부여

        //권한 체크와 승인, 이후 기본앱 설정
        void GetEsentialPermissionAsync()
        {
            //권한이 이미 승인되어있다면
            if (PermissionManager.HasPermission(this, PermissionManager.essentialPermissions))
            {
                //미리 메시지 DB 로드
                _MessageDBLoadTsk = Task.Run(() => LoadMessageDBAsync());

                //처음왔거나, 기본앱설정도 해야된다면 계속 진행.
                if (_IsFirst || _IsDefaultPackage == false)
                    MoveToNextScreen();
                else
                    Finish();
                return;
            }

            //권한 요청
            PermissionManager.RequestPermission(
                this,
                PermissionManager.essentialPermissions,
                "레뜨레 사용을 위해 승인을 눌러주세요.",
                (int)PermissionManager.REQUESTS.ESSENTIAL
                );
        }

        //권한 요청 후 결과가 나왔을 때
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
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

                        //권한이 취득됬으면
                        if (isAllGranted)
                        {
                            Toast.MakeText(this, "권한이 승인되었습니다.", ToastLength.Short).Show();
                            //처음왔거나, 기본앱설정도 해야된다면 계속 진행. 권한만 설정해야되는 경우였다면 Finish.
                            if (_IsFirst || _IsDefaultPackage == false)
                            {
                                //미리 메시지 DB 로드
                                _MessageDBLoadTsk = Task.Run(() => LoadMessageDBAsync());

                                //다음 화면으로 이동
                                MoveToNextScreen();
                            }
                            else
                            {
                                Finish();
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, "권한이 거절당했습니다. 다시 시도해주세요.", ToastLength.Short).Show();
                            RunOnUiThread(() => { _NextBtn.Clickable = true; });        //버튼 누를 수 있게 풀어줘야 됨.
                        }
                    }
                    break;
            }
        }

        private void LoadMessageDBAsync()
        {
            //전체탭에 들어간 대화 중 연락처가 없는 대화는 모두 로드하여 Unknown 카테고리에 넣음.
            MessageDBManager.Get().LoadUnknownMetaDatas();

            //Unknown 카테고리에 들어간 대화의 내용을 모두 로드
            foreach(Dialogue objDialogue in MessageDBManager.Get().UnknownDialogueSet.DialogueList.Values)
            {
                MessageDBManager.Get().ReLoadDialogue(objDialogue, (int)TextMessage.MESSAGE_TYPE.RECEIVED);
            }
        }

        //---------------------------------------------------------------------
        // 기본앱 설정

        const int REQUEST_DEFAULTPACK = 2;

        private void RequestSetAsDefault()
        {
            //기본앱으로 이미 지정이 되어있나?
            if (this.PackageName.Equals(Telephony.Sms.GetDefaultSmsPackage(this)))
            {
                //처음온거면 계속 진행, 기본앱이 풀려서 다시 설정한 것이라면 Finish
                if (_IsFirst)
                    MoveToNextScreen();
                else
                    Finish();
                return;
            }
            else
            {
                //기본앱으로 설정
                Intent intent = new Intent(Telephony.Sms.Intents.ActionChangeDefault);
                intent.PutExtra(Telephony.Sms.Intents.ExtraPackageName, this.PackageName);
                StartActivityForResult(intent, REQUEST_DEFAULTPACK);
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            switch (requestCode)
            {
                case REQUEST_DEFAULTPACK:
                    if (this.PackageName.Equals(Telephony.Sms.GetDefaultSmsPackage(this)))
                    {
                        Toast.MakeText(this, "기본 앱으로 설정되었습니다.", ToastLength.Short).Show();
                        //처음온거면 계속 진행, 기본앱이 풀려서 다시 설정한 것이라면 Finish
                        if (_IsFirst)
                            MoveToNextScreen();
                        else
                            Finish();
                    }
                    else
                    {
                        Toast.MakeText(this, "기본 앱으로 설정되지 않았습니다.\n다시 시도해주세요.", ToastLength.Short).Show();
                        RunOnUiThread(() => { _NextBtn.Clickable = true; });        //버튼 누를 수 있게 풀어줘야 됨.
                    }
                    break;
            }
        }

        //---------------------------------------------------------------------
        // 오프라인 모드 여부 (서버 통신 X)
        private void AskOfflineMode()
        {
            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);
            builder.SetCancelable(false);
            builder.SetTitle("온라인 모드를 사용하시겠습니까?");
            builder.SetMessage("아니오를 누르시면 문자를 서버로 전송하지 않습니다.\n대신 문자 분류 정확성이 떨어지게 됩니다.");
            builder.SetPositiveButton("예", (senderAlert2, args2) =>
            {
                DataStorageManager.SaveBoolData(this, "useOfflineMode", false);         //오프라인 모드 사용하지 않음.
                Categorize();                                                           //온라인 모드이므로, 온라인 카테고리 분류 실행
            });
            builder.SetNegativeButton("아니오", (senderAlert2, args2) =>
            {
                DataStorageManager.SaveBoolData(this, "useOfflineMode", true);        //오프라인 모드 사용

				Stopwatch sw = new Stopwatch();

                Categorize();

            });
            Dialog dialog2 = builder.Create();
            dialog2.Show();
        }


        //---------------------------------------------------------------------
        // 레이블 DB 생성 (서버 통신)

        public event EventHandler<CategorizeEventArgs> _OnCategorizeComplete;

        private void Categorize()
        {
            //이벤트 등록
            _OnCategorizeComplete -= WelcomeActivity_OnCategorizeComplete;
            _OnCategorizeComplete += WelcomeActivity_OnCategorizeComplete;

            //프로그레스바 표기
            _Screens[(int)WELCOME_SCREEN.CATEGORIZE].ProgressBarViewStates = ViewStates.Visible;
            _ViewPager.Adapter.NotifyDataSetChanged();

			Stopwatch sw = new Stopwatch();

            ThreadPool.QueueUserWorkItem(o => CreateLableDB());                                 //카테고리 분류
        }

		private void WelcomeActivity_OnCategorizeComplete(object sender, EventArgs e)
        {
            CategorizeEventArgs resultArgs = e as CategorizeEventArgs;
            bool isSucceed = false;
            string toastMsg = string.Empty;

            //프로그레스바 숨기기
            _Screens[(int)WELCOME_SCREEN.CATEGORIZE].ProgressBarViewStates = ViewStates.Invisible;
            RunOnUiThread(() => { _ViewPager.Adapter.NotifyDataSetChanged(); });

            switch (resultArgs.Result)
            {
                case (int)CategorizeEventArgs.RESULT.EXIST:
                    toastMsg = "메시지 분류가 이미 되어있습니다.";
                    isSucceed = true;
                    break;
                case (int)CategorizeEventArgs.RESULT.SUCCESS:
                    toastMsg = "메시지 분류가 완료되었습니다.";
                    isSucceed = true;
                    break;
                case (int)CategorizeEventArgs.RESULT.FAIL:
                    toastMsg = "메시지 분류에 실패했습니다.";
                    ShowRetryDialog();
                    break;
                case (int)CategorizeEventArgs.RESULT.EMPTY:
                    toastMsg = "분류할 메시지가 없습니다.";
                    isSucceed = true;
                    break;
            }

            RunOnUiThread(() => { Toast.MakeText(this, toastMsg, ToastLength.Short).Show(); });

            if (isSucceed)
            {
                if(DataStorageManager.LoadBoolData(Application.Context, "useOfflineMode", false))
                {
                    //오프라인 분석이 끝나면 화면 종료.
                    DataStorageManager.SaveBoolData(this, "isFirst", false);                        //isFirst 해제
                    DataStorageManager.SaveBoolData(this, "supportMachineLearning", false);         //기계학습 지원 비승인
                    Finish();                                                                       //오프라인 모드를 사용하므로, 기계학습페이지를 표시하지 않고 바로 WelcomeActivity 종료
                }
                else
                {
                    //온라인 분석이 끝나면 다음 화면으로 이동.
                    MoveToNextScreen();
                }
                
            }
            
        }

        private void ShowRetryDialog()
        {
            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);
            builder.SetCancelable(false);
            builder.SetTitle("메시지 분류에 실패했습니다.");
            builder.SetMessage("다시 시도하시겠습니까?");
            builder.SetPositiveButton("예", (senderAlert, args) =>
            {
                Categorize();
            });
            builder.SetNegativeButton("아니오", (senderAlert, args) =>
            {
                Android.Support.V7.App.AlertDialog.Builder builder2 = new Android.Support.V7.App.AlertDialog.Builder(this);
                builder2.SetTitle("메시지를 나중에 분류할 수 있습니다.");
                builder2.SetMessage("메시지 분류를 미루시겠습니까?");
                builder2.SetPositiveButton("예", (senderAlert2, args2) =>
                {
                    MoveToNextScreen();
                });
                builder2.SetNegativeButton("아니오", (senderAlert2, args2) =>
                {
                    RunOnUiThread(() => { _NextBtn.Clickable = true; });        //버튼 누를 수 있게 풀어줘야 됨.
                });
                RunOnUiThread(() =>
                {
                    Dialog dialog2 = builder2.Create();
                    dialog2.Show();
                });
            });
            RunOnUiThread(() => 
            {
                Dialog dialog = builder.Create();
                dialog.Show();
            });
        }

        private async void CreateLableDB()
        {
            //메시지 DB가 로드될때까지 대기
            RunOnUiThread(() => { Toast.MakeText(this, "메시지 DB를 불러오는중...[1/4]", ToastLength.Short).Show(); });
            await _MessageDBLoadTsk;

            //미분류 메시지가 하나도 없는 경우
            if (MessageDBManager.Get().UnknownDialogueSet.Count <= 0)
            {
                _OnCategorizeComplete.Invoke(this, new CategorizeEventArgs((int)CategorizeEventArgs.RESULT.EMPTY));
                return;
            }

            //서버와 통신해서 Lable DB 생성 후 메모리에 올림.
            LableDBManager.Get().CreateDBProgressEvent += WelcomeActivity_CreateDBProgressEvent;

            bool isOffline = DataStorageManager.LoadBoolData(this, "useOfflineMode", false);

            if (isOffline)
            {
                LableDBManager.Get().CreateLableDB_Offline(MessageDBManager.Get().UnknownDialogueSet);
            }
            else
            {
                LableDBManager.Get().CreateLableDB(MessageDBManager.Get().UnknownDialogueSet);
            }
            
            //레이블 DB가 생성되었나?
            if (LableDBManager.Get().IsDBExist())
                _OnCategorizeComplete.Invoke(this, new CategorizeEventArgs((int)CategorizeEventArgs.RESULT.SUCCESS));
            else
                _OnCategorizeComplete.Invoke(this, new CategorizeEventArgs((int)CategorizeEventArgs.RESULT.FAIL));
        }

		private void WelcomeActivity_CreateDBProgressEvent(string toastMsg)
        {
            RunOnUiThread(() =>
            {
                Toast.MakeText(BaseContext, toastMsg, ToastLength.Short).Show();
            });
        }

        //---------------------------------------------------------------------
        // 기계학습 요청

        private void AskMachineSupport()
        {
            Android.Support.V7.App.AlertDialog.Builder builder2 = new Android.Support.V7.App.AlertDialog.Builder(this);
            builder2.SetCancelable(false);
            builder2.SetTitle("기계학습 지원 요청");
            builder2.SetMessage("기계학습 지원을 하시겠습니까?");
            builder2.SetPositiveButton("예", (senderAlert2, args2) =>
            {
                DataStorageManager.SaveBoolData(this, "isFirst", false);                      //isFirst 해제
                DataStorageManager.SaveBoolData(this, "supportMachineLearning", true);        //기계학습 지원 승인
                Finish();                                                                     //Welecome Activity 종료
            });
            builder2.SetNegativeButton("아니오", (senderAlert2, args2) =>
            {
                DataStorageManager.SaveBoolData(this, "isFirst", false);                        //isFirst 해제
                DataStorageManager.SaveBoolData(this, "supportMachineLearning", false);         //기계학습 지원 비승인
                Finish();
            });
            Dialog dialog2 = builder2.Create();
            dialog2.Show();
        }
    }

    //----------------------------------------------------------------------------------------------
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
            ProgressBar progressBar = view.FindViewById<ProgressBar>(Resource.Id.ws_progressBar);

            rootRL.SetBackgroundColor(_Screens[position].BackgroundColor);
            primaryTV.Text = _Screens[position].PrimaryText;
            imageView.SetImageResource(_Screens[position].Image);
            secondaryTV.Text = _Screens[position].SecondaryText;
            progressBar.Visibility = _Screens[position].ProgressBarViewStates;

            var viewPager = container.JavaCast<NonSwipeableViewPager>();
            viewPager.AddView(view);

            return view;
        }

        public override void DestroyItem(View container, int position, Java.Lang.Object view)
        {
            var viewPager = container.JavaCast<NonSwipeableViewPager>();
            viewPager.RemoveView(view as View);
        }

        public override int GetItemPosition(Java.Lang.Object @object)
        {
            return PositionNone;
        }
    }

    //----------------------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------------------
    // ViewPage의 Screen

    public class Screen
    {
        int layout;
        int image;
        string primaryText;
        string secondaryText;
        Android.Graphics.Color backgroundColor;
        ViewStates progressBarViewStates;

        public Screen(int layout, int image, string primaryText, string secondaryText, Android.Graphics.Color backgroundColor)
        {
            this.layout = layout;
            this.image = image;
            this.primaryText = primaryText;
            this.secondaryText = secondaryText;
            this.backgroundColor = backgroundColor;
            this.progressBarViewStates = ViewStates.Invisible;
        }

        public int Layout { get { return layout; } }
        public int Image { get { return image; } }
        public string PrimaryText { get { return primaryText; } }
        public string SecondaryText { get { return secondaryText; } }
        public Android.Graphics.Color BackgroundColor { get { return backgroundColor; } }
        public ViewStates ProgressBarViewStates { set { this.progressBarViewStates = value; } get { return progressBarViewStates; } }

    }

    //----------------------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------------------
    // Categorize EventArgs

    public class CategorizeEventArgs : EventArgs
    {
        public enum RESULT { EXIST = 0, SUCCESS, FAIL, EMPTY }
        private int result;

        public CategorizeEventArgs(int result)
        {
            this.result = result;
        }

        public int Result { get { return result; } }
    }



}