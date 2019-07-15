using Android.App;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V4.App;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;

using Android.Views;
using Com.EightbitLab.BlurViewBinding;
using Android.Graphics.Drawables;
using Android.Widget;
using Android.Provider;
using System.Collections.Generic;

using Android.Content;
using Android.Net;
using Android.Database;
using Android;

using LettreForAndroid.Class;
using LettreForAndroid.Utility;
using LettreForAndroid.Page;
using System.Threading;

namespace LettreForAndroid
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.activity_main);

            //메인 화면 세팅
            //SetupBlurView();      //블러뷰 적용시 배경화면이 뭉개져서 주석처리.
            SetupToolBar();
            SetupTabLayout();

            //처음 사용자면 welcompage 표시
            if (DataStorageManager.loadBoolData(this, "isFirst", true))
            {
                Android.App.FragmentTransaction transaction = FragmentManager.BeginTransaction();
                welcome_page firstMeetDialog = new welcome_page();
                firstMeetDialog.Show(transaction, "dialog_fragment");
                firstMeetDialog.onWelcomeComplete += FirstMeetDialog_onWelcomeComplete;     //Welcome Page에서 Dismiss되면 메소드 호출
            }
            else
            {
                GetTextMessage();
            }

            

        }

        private void FirstMeetDialog_onWelcomeComplete(object sender, welcome_page.OnWelcomeEventArgs e)
        {
            GetTextMessage();
        }

        public bool isDefaultApp()
        {
            return PackageName.Equals(Telephony.Sms.GetDefaultSmsPackage(this));
        }

        public void GetTextMessage()
        {
            if (DataStorageManager.loadBoolData(this, "isDefaultPackage", false))
            {
                List<TextMessage> lst = getAllTextMessages();
            }
            else
            {
                //기본앱으로 설정해야하는 이유를 알려주고 표시해라.
                //SetAsDefaultApp();
            }
        }

        //GetTextMessage
        public List<TextMessage> getAllTextMessages()
        {
            List<TextMessage> lstSms = new List<TextMessage>();
            TextMessage objSms = new TextMessage();
            Uri message = Uri.Parse("content://sms/");
            ContentResolver cr = this.ContentResolver;

            ICursor c = cr.Query(message, null, null, null, null);
            this.StartManagingCursor(c);
            int totalSMS = c.Count;

            if (c.MoveToFirst())
            {
                for (int i = 0; i < totalSMS; i++)
                {
                    objSms = new TextMessage();
                    objSms.Id = c.GetString(c.GetColumnIndexOrThrow("_id"));
                    objSms.Address = c.GetString(c.GetColumnIndexOrThrow("address"));
                    objSms.Msg = c.GetString(c.GetColumnIndexOrThrow("body"));
                    objSms.ReadState = c.GetString(c.GetColumnIndex("read"));
                    objSms.Time = c.GetString(c.GetColumnIndexOrThrow("date"));
                    if (c.GetString(c.GetColumnIndexOrThrow("type")).Contains("1"))
                    {
                        objSms.Folder = "inbox";
                    }
                    else
                    {
                        objSms.Folder = "sent";
                    }

                    lstSms.Add(objSms);
                    c.MoveToNext();
                }
            }
            // else {
            // throw new RuntimeException("You have no SMS");
            // }
            c.Close();

            return lstSms;
        }

        //BlurView 적용
        public void SetupBlurView()
        {
            ViewGroup root = FindViewById<ViewGroup>(Resource.Id.root);
            BlurView mainBlurView = FindViewById<BlurView>(Resource.Id.mainBlurView);

            float radius = 0.0001F;

            Drawable windowBackground = Window.DecorView.Background;

            var topViewSettings = mainBlurView.SetupWith(root)
                .WindowBackground(windowBackground)
                .BlurAlgorithm(new RenderScriptBlur(this))
                .BlurRadius(radius)
                .SetHasFixedTransformationMatrix(true);
        }

        //툴바 적용
        public void SetupToolBar()
        {
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.my_toolbar);

            SetSupportActionBar(toolbar);
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
            }
            else
            {
                DataStorageManager.saveStringData(this,"temp", item.TitleFormatted.ToString());
            }
            return base.OnOptionsItemSelected(item);
        }

        // 탭 레이아웃 설정
        public void SetupTabLayout()
        {
            var pager = FindViewById<ViewPager>(Resource.Id.pager);
            var tabLayout = FindViewById<TabLayout>(Resource.Id.sliding_tabs);
            var adapter = new CustomPagerAdapter(this, SupportFragmentManager);

            //탭 추가
            adapter.initTabs();

            // Set adapter to view pager
            pager.Adapter = adapter;

            // Setup tablayout with view pager
            tabLayout.SetupWithViewPager(pager);

            //모든 탭에 커스텀 뷰 적용
            for (int i = 0; i < tabLayout.TabCount; i++)
            {
                TabLayout.Tab tab = tabLayout.GetTabAt(i);
                tab.SetCustomView(adapter.GetTabView(i));
            }
            tabLayout.TabSelected += TabLayout_TabSelected;
            tabLayout.TabUnselected += TabLayout_TabUnselected;
        }

        private void TabLayout_TabSelected(object sender, TabLayout.TabSelectedEventArgs e)
        {
            TextView tv = e.Tab.CustomView.FindViewById<TextView>(Resource.Id.custTab_title);
            //Toast.MakeText(this, tv.Text + "선택됨" , ToastLength.Short).Show();
            tv.SetTypeface(tv.Typeface, Android.Graphics.TypefaceStyle.Bold);
        }
        private void TabLayout_TabUnselected(object sender, TabLayout.TabUnselectedEventArgs e)
        {
            TextView tv = e.Tab.CustomView.FindViewById<TextView>(Resource.Id.custTab_title);
            tv.SetTypeface(null, Android.Graphics.TypefaceStyle.Normal);
        }
    }
}