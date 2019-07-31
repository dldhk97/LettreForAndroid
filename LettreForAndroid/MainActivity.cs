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

        const int REQUEST_WELCOMECOMPLETE = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.activity_main);

            string thisPackName = PackageName;
            string defulatPackName = Android.Provider.Telephony.Sms.GetDefaultSmsPackage(this);

            //기본앱이 아니면 Welcompage Activity 시작
            if (!thisPackName.Equals(defulatPackName))
            {
                StartActivityForResult(typeof(welcome_page), REQUEST_WELCOMECOMPLETE);
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
                case REQUEST_WELCOMECOMPLETE:
                    Setup();
                    break;
            }

        }

        //웰컴페이지가 끝나거나, 처음사용자가 아닌경우 바로 이 메소드로 옮.
        public void Setup()
        {
            //연락처 및 메세지 매니저 세팅
            ContactManager.Get().Initialization();
            ContactManager.Get().refreshContacts(this);
            MessageManager.Get().Initialization();
            MessageManager.Get().refreshMessages();
            //ThreadPool.QueueUserWorkItem(o => MessageManager.Get().Initialization(this));     //스레드 풀 이용

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
                DataStorageManager.saveStringData(this, "temp", item.TitleFormatted.ToString());
                string[] data1 = new string[] { "010-1234-1234", "문자내용이 들어감1" };
                string[] data2 = new string[] { "010-1234-1235", "문자내용이 들어감2" };
                string[] data3 = new string[] { "010-1234-1236", "문자내용이 들어감3" };
                string[] data4 = new string[] { "010-1234-1237", "문자내용이 들어감4" };
                string[] data5 = new string[] { "010-1234-1238", "문자내용이 들어감5" };
                List<string[]> dataList = new List<string[]>() { data1, data2, data3, data4, data5 };
                NetworkManager.Get().SendAndReceiveData(dataList);
            }
            return base.OnOptionsItemSelected(item);
        }

        //---------------------------------------------------------------------
        //하단 버튼 세팅

        public void SetupBottomBar()
        {
            var dialogueViewBtn = FindViewById<Button>(Resource.Id.mp_bottomBtn1);
            var contactViewBtn = FindViewById<Button>(Resource.Id.mp_bottomBtn2);
            //연락처 액티비티와 메인액티비티 간 전환 메소드 넣으면 됨.
        }
    }
}