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

            //메인 화면 세팅
            //BlurViewManager bvm = new BlurViewManager(this);
            //bvm.SetupBlurView();      //블러뷰 적용시 배경화면이 뭉개져서 주석처리.

            //툴바 세팅
            ToolbarManager tm = new ToolbarManager(this, this);
            tm.SetupToolBar();

            tfm = new TabFragManager(this, SupportFragmentManager);

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
                OnWelcomeComplete();
            }

        }

        private void FirstMeetDialog_onWelcomeComplete(object sender, welcome_page.OnWelcomeEventArgs e)
        {
            OnWelcomeComplete();
        }

        public void OnWelcomeComplete()
        {
            tfm.SetupTabLayout();
            MessageManager.Get().Initialization(this);
        }
        
    }
}