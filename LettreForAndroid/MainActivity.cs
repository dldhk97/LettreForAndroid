using Android.App;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.App;

using Android.Views;
using Android.Widget;

using LettreForAndroid.Class;
using LettreForAndroid.Utility;
using LettreForAndroid.Page;

using Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Content;
using Android.Runtime;

namespace LettreForAndroid
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        TabFragManager tfm;
        Toolbar mToolbar;

        const int mWelcomeActivityCallback = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.activity_main);

            //Tab Fragment Manger 초기화
            tfm = new TabFragManager(this, SupportFragmentManager);

            //툴바 세팅
            SetupToolBar();

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

        private void FirstMeetDialog_onWelcomeComplete(object sender, welcome_page.OnWelcomeEventArgs e)
        {
            OnWelcomeComplete();
        }

        //웰컴페이지가 끝나거나, 처음사용자가 아닌경우 바로 이 메소드로 옮.
        public void OnWelcomeComplete()
        {
            //탭 레이아웃 세팅
            tfm.SetupTabLayout();
            //메세지 매니저(싱글톤)세팅
            ContactManager.Get().Initialization(this);
            MessageManager.Get().Initialization(this);
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
            MenuInflater.Inflate(Resource.Menu.toolbar, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        //툴바 선택시
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            //Toast.MakeText(this, "Top ActionBar pressed: " + item.TitleFormatted, ToastLength.Short).Show();
            if(item.ItemId == Resource.Id.toolbar_search)
            {
                string str = DataStorageManager.loadStringData(this,"temp", "NULL");
                Toast.MakeText(this, "Top ActionBar pressed: " + str, ToastLength.Short).Show();
                StartActivity(typeof(dialogue_page));
            }
            else
            {
                DataStorageManager.saveStringData(this,"temp", item.TitleFormatted.ToString());
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}