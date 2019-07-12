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

namespace LettreForAndroid
{
    [Activity(Label = "@string/app_name", Icon ="@drawable/Icon_128", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        ViewGroup root;
        BlurView mainBlurView;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            
            //BlurView 시작
            root = FindViewById<ViewGroup>(Resource.Id.root);
            mainBlurView = FindViewById<BlurView>(Resource.Id.mainBlurView);

            float radius = 0.5F;

            Drawable windowBackground = Window.DecorView.Background;

            var topViewSettings = mainBlurView.SetupWith(root)
                .WindowBackground(windowBackground)
                .BlurAlgorithm(new RenderScriptBlur(this))
                .BlurRadius(radius)
                .SetHasFixedTransformationMatrix(true);
            
            ////BlurView 끝

            // 툴바를 액션바로 사용 & 탭 레이아웃 설정
            var pager = FindViewById<ViewPager>(Resource.Id.pager);
            var tabLayout = FindViewById<TabLayout>(Resource.Id.sliding_tabs);
            var adapter = new CustomPagerAdapter(this, SupportFragmentManager);
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.my_toolbar);

            // 툴바 설정
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "Lettre";
            //SupportActionBar.Hide();

            // Set adapter to view pager
            pager.Adapter = adapter;

            // Setup tablayout with view pager
            tabLayout.SetupWithViewPager(pager);

            // 모든 탭에 커스텀 뷰 적용
            for (int i = 0; i < tabLayout.TabCount; i++)
            {
                TabLayout.Tab tab = tabLayout.GetTabAt(i);
                tab.SetCustomView(adapter.GetTabView(i));
            }

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
            Toast.MakeText(this, "Top ActionBar pressed: " + item.TitleFormatted, ToastLength.Short).Show();
            return base.OnOptionsItemSelected(item);
        }
    }
}