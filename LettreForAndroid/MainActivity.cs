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


namespace LettreForAndroid
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Theme = "@style/BasicTheme")]
    public class MainActivity : AppCompatActivity
    {
        TabFragManager _TabFragManager;

        const int REQUEST_DEFAULTPACKCOMPLETE = 1;
        const int REQUEST_WELCOMEACTIVITYCOMPLETE = 2;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.MainActivity);

            string thisPackName = PackageName;
            string defulatPackName = Android.Provider.Telephony.Sms.GetDefaultSmsPackage(this);

            //기본앱이 아니면 Welcompage Activity 시작
            if (!thisPackName.Equals(defulatPackName))
            {
                StartActivityForResult(typeof(DefaultPackActivity), REQUEST_DEFAULTPACKCOMPLETE);
            }
            else if(!LableDBManager.Get().IsDBExist())
            {
                StartActivityForResult(typeof(WelcomeActivity), REQUEST_WELCOMEACTIVITYCOMPLETE);
            }
            else
            {
                Setup();
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            switch (requestCode)
            {
                case REQUEST_DEFAULTPACKCOMPLETE:
                    StartActivityForResult(typeof(WelcomeActivity), REQUEST_WELCOMEACTIVITYCOMPLETE);
                    break;
                case REQUEST_WELCOMEACTIVITYCOMPLETE:
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

            //레이블 DB가 있나?
            if(LableDBManager.Get().IsDBExist())
            {
                MessageDBManager.Get().CategorizeNewMsg(); //메시지 중 레이블이 붙어있지 않은 대화가 있으면, 그 대화 다시 카테고라이즈함.
                MessageDBManager.Get().CategorizeLocally(
                    MessageDBManager.Get().DialogueSets[(int)Dialogue.LableType.UNKNOWN]);
            }
            else
            {
                //레이블 DB가 없는 경우. 정상적인 사용이 불가능.
            }

            //ThreadPool.QueueUserWorkItem(o => MessageManager.Get().Initialization(this));     //스레드 풀 이용

            CreateNotificationChannel();            //Notification을 위한 채널 등록

            SetupLayout();
        }

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
            //툴바 세팅
            SetupToolBar();

            //하단 바 설정
            SetupBottomBar();

            //탭 레이아웃 세팅
            _TabFragManager = new TabFragManager(this, SupportFragmentManager);
            _TabFragManager.SetupTabLayout();
        }

        //-------------------------------------------------------------
        //툴바 세팅
        
         //툴바 적용
        public void SetupToolBar()
        {
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.my_toolbar);

            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "Lettre";
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

            }
            else
            {

            }
            return base.OnOptionsItemSelected(item);
        }

        //---------------------------------------------------------------------
        //하단 버튼 세팅

        public void SetupBottomBar()
        {
            var dialogueViewBtn = FindViewById<Button>(Resource.Id.ma_bottomBtn1);
            var contactViewBtn = FindViewById<Button>(Resource.Id.ma_bottomBtn2);
            //연락처 액티비티와 메인액티비티 간 전환 메소드 넣으면 됨.
        }
    }
}