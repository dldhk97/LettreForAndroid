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
            //hi
            //BlurView Start
            root = FindViewById<ViewGroup>(Resource.Id.root);
            mainBlurView = FindViewById<BlurView>(Resource.Id.mainBlurView);

            float radius = 0.5F;

            //set background, if your root layout doesn't have one
            Drawable windowBackground = Window.DecorView.Background;

            var topViewSettings = mainBlurView.SetupWith(root)
                .WindowBackground(windowBackground)
                .BlurAlgorithm(new RenderScriptBlur(this)) // SupportRenderScriptBlur
                .BlurRadius(radius)
                .SetHasFixedTransformationMatrix(true);
            
            ////BlurView End

            // Find views
            var pager = FindViewById<ViewPager>(Resource.Id.pager);
            var tabLayout = FindViewById<TabLayout>(Resource.Id.sliding_tabs);
            var adapter = new CustomPagerAdapter(this, SupportFragmentManager);
            var toolbar = FindViewById<Toolbar>(Resource.Id.my_toolbar);

            // Setup Toolbar
            SetSupportActionBar(toolbar);
            //SupportActionBar.Title = "Lettre";
            SupportActionBar.Hide();

            // Set adapter to view pager
            pager.Adapter = adapter;

            // Setup tablayout with view pager
            tabLayout.SetupWithViewPager(pager);

            // Iterate over all tabs and set the custom view
            for (int i = 0; i < tabLayout.TabCount; i++)
            {
                TabLayout.Tab tab = tabLayout.GetTabAt(i);
                tab.SetCustomView(adapter.GetTabView(i));
            }
        }
    }
}