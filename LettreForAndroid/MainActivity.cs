using Android.App;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.App;

using Android.Views;
using Android.Widget;

using Android.Content;
using Android.Runtime;
using System.Collections.Generic;
using System.Threading;

using LettreForAndroid.Class;
using LettreForAndroid.Utility;
using LettreForAndroid.UI;
using LettreForAndroid.Receivers;

using Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.Design.Widget;

namespace LettreForAndroid
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Theme = "@style/BasicTheme")]
    public class MainActivity : AppCompatActivity
    {
        TabFragManager _TabFragManager;
        ContactViewManager _ContactManager;

        const int REQUEST_NEWWELCOMECOMPLETE = 0;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.MainActivity);

            StartActivityForResult(typeof(WelcomeActivity), REQUEST_NEWWELCOMECOMPLETE);

        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            switch (requestCode)
            {
                case REQUEST_NEWWELCOMECOMPLETE:
                    Setup();
                    break;
            }
        }

        //웰컴페이지가 끝나거나, 처음사용자가 아닌경우 바로 이 메소드로 옮.
        public void Setup()
        {
            ContactDBManager.Get();                         //연락처를 모두 메모리에 올림
            LableDBManager.Get();                           //레이블 DB를 모두 메모리에 올림
            MessageDBManager.Get();                         //메시지를 모두 메모리에 올림

            MessageDBManager.Get().SortDialogueSets();

            //ThreadPool.QueueUserWorkItem(o => MessageManager.Get().Initialization(this));     //스레드 풀 이용

            CreateNotificationChannel(); 

            SetupLayout();
        }

        //Notification을 위한 채널 등록
        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                return;

            var channelName = Resources.GetString(Resource.String.channel_name);
            var channelDescription = GetString(Resource.String.channel_description);
            var channelID = GetString(Resource.String.channel_Id);

            var channel = new NotificationChannel(channelID, channelName, NotificationImportance.Default);
            channel.Description = channelDescription;
            channel.Importance = NotificationImportance.Max;

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        public void SetupLayout()
        {
            SetupToolBar();

            SetupDialogueLayout();

            SetupContactLayout();

            SetupFloatingButton();

            SetupBottomBar();
        }

        //-------------------------------------------------------------
        //툴바 세팅
        
         //툴바 적용
        public void SetupToolBar()
        {
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.ma_toolbar);

            SetSupportActionBar(toolbar);
            SupportActionBar.Title = Resources.GetString(Resource.String.app_name);
        }
        //툴바에 메뉴 추가
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.toolbar_main, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        //툴바 선택시
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if(item.ItemId == Resource.Id.toolbar_search)
            {
                //MessageDBManager.Get().Load2();
            }
            else
            {

            }
            return base.OnOptionsItemSelected(item);
        }

        //---------------------------------------------------------------------
        //대화 레이아웃 세팅

        private void SetupDialogueLayout()
        {
            //탭 레이아웃 및 뷰페이저 세팅
            _TabFragManager = new TabFragManager(this, SupportFragmentManager);
            _TabFragManager.SetupTabLayout();
        }

        //---------------------------------------------------------------------
        //연락처 레이아웃 세팅

        private void SetupContactLayout()
        {
            _ContactManager = new ContactViewManager();
            _ContactManager.SetContactViewLayout(this);
        }

        //---------------------------------------------------------------------
        //플로팅 버튼 세팅
        private void SetupFloatingButton()
        {
            FloatingActionButton ma_sendButton = FindViewById<FloatingActionButton>(Resource.Id.ma_sendButton);
            ma_sendButton.Click += (sender, ob) =>
            {
                Intent intent = new Intent(this, typeof(NewDialogueActivity));
                StartActivity(intent);
            };
        }

        //---------------------------------------------------------------------
        //하단 버튼 세팅
        private enum MAINPAGETYPE {DIALOGUE = 1, CONTACT };
        int _CurrentPage = 1;
        private void SetupBottomBar()
        {
            var dialogueBtn = FindViewById<Button>(Resource.Id.ma_bottomBtn1);
            var contactBtn = FindViewById<Button>(Resource.Id.ma_bottomBtn2);
            var dialogueLayout = FindViewById<RelativeLayout>(Resource.Id.ma_dialogueLayout);
            var contactLayout = FindViewById<RelativeLayout>(Resource.Id.ma_contactLayout);

            //연락처 버튼 클릭 시
            contactBtn.Click += (sender, o) =>
            {
                if (_CurrentPage == (int)MAINPAGETYPE.CONTACT)
                    return;
                _CurrentPage = (int)MAINPAGETYPE.CONTACT;

                ContactDBManager.Get().Refresh();
                _ContactManager.Refresh();

                Android.Views.Animations.Animation anim_left_out = Android.Views.Animations.AnimationUtils.LoadAnimation(BaseContext, Resource.Animation.slide_left_out);
                Android.Views.Animations.Animation anim_left_in = Android.Views.Animations.AnimationUtils.LoadAnimation(BaseContext, Resource.Animation.slide_left_in);
                anim_left_out.AnimationEnd += (sender2, e) =>
                  {
                      dialogueLayout.Visibility = ViewStates.Gone;
                  };

                contactLayout.Visibility = ViewStates.Visible;
                contactBtn.SetTypeface(contactBtn.Typeface, Android.Graphics.TypefaceStyle.Bold);
                dialogueBtn.SetTypeface(null, Android.Graphics.TypefaceStyle.Normal);

                dialogueLayout.StartAnimation(anim_left_out);
                contactLayout.StartAnimation(anim_left_in);
            };

            //메시지 버튼 클릭 시
            dialogueBtn.Click += (sender, o) =>
            {
                if (_CurrentPage == (int)MAINPAGETYPE.DIALOGUE)
                    return;
                _CurrentPage = (int)MAINPAGETYPE.DIALOGUE;

                Android.Views.Animations.Animation anim_right_out = Android.Views.Animations.AnimationUtils.LoadAnimation(BaseContext, Resource.Animation.slide_right_out);
                Android.Views.Animations.Animation anim_right_in = Android.Views.Animations.AnimationUtils.LoadAnimation(BaseContext, Resource.Animation.slide_right_in);
                anim_right_out.AnimationEnd += (sender2, e) =>
                {
                    contactLayout.Visibility = ViewStates.Gone;
                };

                dialogueLayout.Visibility = ViewStates.Visible;
                dialogueBtn.SetTypeface(contactBtn.Typeface, Android.Graphics.TypefaceStyle.Bold);
                contactBtn.SetTypeface(null, Android.Graphics.TypefaceStyle.Normal);

                dialogueLayout.StartAnimation(anim_right_out);
                contactLayout.StartAnimation(anim_right_in);
            };
        }
    }
}