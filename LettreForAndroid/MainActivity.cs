using Android.App;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.App;

using Android.Views;
using Android.Widget;


using LettreForAndroid.Class;
using LettreForAndroid.Utility;
using LettreForAndroid.Page;

namespace LettreForAndroid
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        TabFragManager tfm;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.activity_main);

            //Tab Fragment Manger 초기화
            tfm = new TabFragManager(this, SupportFragmentManager);

            //툴바 세팅
            ToolbarManager tm = new ToolbarManager(this, this);
            tm.SetupToolBar();

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
                //처음 사용자가 아니면 바로 할일 함.
                OnWelcomeComplete();
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
        
    }
}