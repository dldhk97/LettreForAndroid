﻿using Android.App;
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
using Android.Content.Res;
using System.Threading.Tasks;

namespace LettreForAndroid
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Theme = "@style/BasicTheme")]
    public class MainActivity : AppCompatActivity
    {
        public static MainActivity _Instance { get; private set; }

        TabFragManager _TabFragManager;
        ContactViewManager _ContactManager;

        const int REQUEST_NEWWELCOME_COMPLETE = 0;
        const int REQUEST_RECATEGORIZE_COMPLETE = 1;
        public bool _MessageLoadedOnce = false;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.MainActivity);

            _Instance = this;

            StartActivityForResult(typeof(WelcomeActivity), REQUEST_NEWWELCOME_COMPLETE);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            switch (requestCode)
            {
                case REQUEST_NEWWELCOME_COMPLETE:
                    Setup();
                    break;
                case REQUEST_RECATEGORIZE_COMPLETE:
                    LableDBManager.Get().Load();
                    LoadMessageDB();
                    _TabFragManager.RefreshLayout();
                    break;
            }
        }

        //웰컴페이지가 끝나거나, 처음사용자가 아닌경우 바로 이 메소드로 옮.
        public async void Setup()
        {
            Task contactLoadTsk = Task.Run(() => ContactDBManager.Get());       //연락처를 모두 메모리에 올림
            Task lableLoadTsk = Task.Run(() => LableDBManager.Get().Load());    //레이블 DB를 모두 메모리에 올림

            CreateNotificationChannel(); 

            SetupLayout();

            await contactLoadTsk;

            SetupContactLayout();

            await lableLoadTsk;

            Task messageLoadTsk = Task.Run(() => LoadMessageDB());          //메시지를 모두 메모리에 올림
            SetupDialogueLayout(messageLoadTsk);
        }

        private void LoadMessageDB()
        {
            MessageDBManager.Get().RefreshLastMessageAll();
            _MessageLoadedOnce = true;
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
                RunOnUiThread(() => { Toast.MakeText(this, "준비중인 기능입니다.", ToastLength.Short).Show(); });
            }
            else if (item.ItemId == Resource.Id.toolbar_recategorize)
            {
                DataStorageManager.SaveBoolData(this, "needRecategorize", true);
                LableDBManager.Get().Drop();
                StartActivityForResult(typeof(WelcomeActivity), REQUEST_RECATEGORIZE_COMPLETE);
            }
            else
            {
                RunOnUiThread(() => { Toast.MakeText(this, "준비중인 기능입니다.", ToastLength.Short).Show(); });
            }
            return base.OnOptionsItemSelected(item);
        }

        //---------------------------------------------------------------------
        //대화 레이아웃 세팅

        private void SetupDialogueLayout(Task messageLoadTsk)
        {
            //탭 레이아웃 및 뷰페이저 세팅
            _TabFragManager = new TabFragManager(this, SupportFragmentManager);
            _TabFragManager.SetupTabLayout(messageLoadTsk);
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