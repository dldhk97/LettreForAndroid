using Android.App;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.App;

using Android.Views;
using Android.Widget;

using LettreForAndroid.Class;
using LettreForAndroid.Utility;
using LettreForAndroid.UI;

using Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Content;
using Android.Runtime;
using System.Collections.Generic;
using System.Threading;

namespace LettreForAndroid
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Theme = "@style/BasicTheme")]
    public class MainActivity : AppCompatActivity
    {
        TabFragManager tfm;
        Toolbar mToolbar;

        const int mWelcomeActivityCallback = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.activity_main);

            //처음 사용자면 welcompage 표시
            if (DataStorageManager.loadBoolData(this, "isFirst", true))
            {
                StartActivityForResult(typeof(welcome_page), mWelcomeActivityCallback);
            }
            else
            {
                //처음 사용자가 아니면 바로 할일 함.
                OnWelcomeComplete();
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            switch (requestCode)
            {
                case mWelcomeActivityCallback:
                    OnWelcomeComplete();
                    break;
            }

        }

        //웰컴페이지가 끝나거나, 처음사용자가 아닌경우 바로 이 메소드로 옮.
        public void OnWelcomeComplete()
        {
            //메세지 매니저(싱글톤)세팅
            ContactManager.Get().Initialization(this);
            MessageManager.Get().Initialization(this);
            //ThreadPool.QueueUserWorkItem(o => MessageManager.Get().Initialization(this));     //스레드 풀 이용

            //Tab Fragment Manger 초기화
            tfm = new TabFragManager(this, SupportFragmentManager);

            //툴바 세팅
            SetupToolBar();

            //하단 바 설정
            SetupBottomBar();

            //탭 레이아웃 세팅
            tfm.SetupTabLayout();

            //이게 끝나면 각 리사이클뷰 내용 표시 처리함.
        }
        
         //툴바 적용
        public void SetupToolBar()
        {
            mToolbar = FindViewById<Toolbar>(Resource.Id.my_toolbar);

            SetSupportActionBar(mToolbar);
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
            //Toast.MakeText(this, "Top ActionBar pressed: " + item.TitleFormatted, ToastLength.Short).Show();
            if(item.ItemId == Resource.Id.toolbar_search)
            {
                //string str = DataStorageManager.loadStringData(this,"temp", "NULL");
                //Toast.MakeText(this, "Top ActionBar pressed: " + str, ToastLength.Short).Show();
                //StartActivity(typeof(dialogue_page));
                //string[] header = new string[] { "0", "1" };
                //string[] data1 = new string[] { "010-1234-1234", "하나만 보냄, 문자내용이 들어감1" };
                //List<string[]> dataList = new List<string[]>() { data1};
                //NetworkManager.Get().sendAndReceiveData(dataList, 0);
            }
            else
            {
                //DataStorageManager.saveStringData(this,"temp", item.TitleFormatted.ToString());
                string[] header = new string[] { "0", "5" };
                string[] data1 = new string[] { "010-1234-1234", "문자내용이 들어감1" };
                string[] data2 = new string[] { "010-1234-1235", "문자내용이 들어감2" };
                string[] data3 = new string[] { "010-1234-1236", "문자내용이 들어감3" };
                string[] data4 = new string[] { "010-1234-1237", "문자내용이 들어감4" };
                string[] data5 = new string[] { "010-1234-1238", "문자내용이 들어감5" };
                List<string[]> dataList = new List<string[]>() { data1, data2, data3, data4, data5 };
                NetworkManager.Get().sendAndReceiveData(dataList, 0);
            }
            return base.OnOptionsItemSelected(item);
        }

        public void SetupBottomBar()
        {
            var dialogueViewBtn = FindViewById<Button>(Resource.Id.mp_bottomBtn1);
            var contactViewBtn = FindViewById<Button>(Resource.Id.mp_bottomBtn2);
        }
    }
}