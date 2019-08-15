using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace LettreForAndroid.UI
{
    [Activity(Label = "WelcomeActivity2", Theme = "@style/BasicTheme")]
    class WelcomeActivity2 : AppCompatActivity
    {
        private NonSwipeableViewPager _ViewPager;
        private Button _NextBtn;

        List<Screen> _Screens = new List<Screen>()
        {
            new Screen(Resource.Layout.welcome_screen, Resource.Drawable.main_icon_drawable_512, "환영합니다!", "계속 버튼을 눌러 진행해주세요"),
            new Screen(Resource.Layout.welcome_screen, Resource.Drawable.main_icon_drawable_512, "개인정보처리방침", "개인정보 처리방침 내용과, 동의 버튼 다이얼로그 표시"),
            new Screen(Resource.Layout.welcome_screen, Resource.Drawable.main_icon_drawable_512, "권한이 필요합니다!", "메시지 권한을 주세요!"),
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //isfirst? - > finish();

            SetContentView(Resource.Layout.WelcomeActivity2);

            _ViewPager = FindViewById<NonSwipeableViewPager>(Resource.Id.wa_viewpager);
            _NextBtn = FindViewById<Button>(Resource.Id.wa_nextBtn);

            WelcomeScreenAdapter adapter = new WelcomeScreenAdapter(this, _Screens);

            _ViewPager.Adapter = adapter;

            _NextBtn.Click += _NextBtn_Click;

        }

        public override void OnBackPressed()
        {
            //뒤로가기를 눌렀을 때 아무 반응 하지 않음.
        }

        private void _NextBtn_Click(object sender, EventArgs e)
        {
            if (_ViewPager.CurrentItem + 1 == _Screens.Count)
                Finish();
            _ViewPager.SetCurrentItem(_ViewPager.CurrentItem + 1, true);
        }
    }

    public class NonSwipeableViewPager : ViewPager
    {
        public NonSwipeableViewPager(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer) { }
        public NonSwipeableViewPager(Context context, IAttributeSet attrs) : base(context, attrs) { }

        public override bool OnTouchEvent(MotionEvent e) { return false; }

        public override bool OnInterceptTouchEvent(MotionEvent ev) { return false; }
    }

    public class WelcomeScreenAdapter : PagerAdapter
    {
        Context _Context;
        List<Screen> _Screens;

        public WelcomeScreenAdapter(Context context, List<Screen> screens)
        {
            _Context = context;
            _Screens = screens;
        }

        public override int Count
        {
            get { return _Screens.Count; }
        }

        public override bool IsViewFromObject(View view, Java.Lang.Object obj)
        {
            return view == obj;
        }

        public override Java.Lang.Object InstantiateItem(View container, int position)
        {
            LayoutInflater inflater = LayoutInflater.From(_Context);
            View view = inflater.Inflate(Resource.Layout.welcome_screen, null);

            RelativeLayout rootRL = view.FindViewById<RelativeLayout>(Resource.Id.ws_rootRL);
            TextView primaryTV = view.FindViewById<TextView>(Resource.Id.ws_primaryTV);
            ImageView imageView = view.FindViewById<ImageView>(Resource.Id.ws_imageView);
            TextView secondaryTV = view.FindViewById<TextView>(Resource.Id.ws_secondaryTV);

            primaryTV.Text = _Screens[position].PrimaryText;
            imageView.SetImageResource(_Screens[position].Image);
            secondaryTV.Text = _Screens[position].SecondaryText;

            var viewPager = container.JavaCast<NonSwipeableViewPager>();
            viewPager.AddView(view);

            return view;
        }

        public override void DestroyItem(View container, int position, Java.Lang.Object view)
        {
            var viewPager = container.JavaCast<NonSwipeableViewPager>();
            viewPager.RemoveView(view as View);
        }
    }

    public class Screen
    {
        int layout;
        int image;
        string primaryText;
        string secondaryText;
        Android.Graphics.Color backgroundColor;

        public Screen(int layout, int image, string primaryText, string secondaryText)
        {
            this.layout = layout;
            this.image = image;
            this.primaryText = primaryText;
            this.secondaryText = secondaryText;
            //this.backgroundColor = backgroundColor;
        }

        public int Layout { get { return layout; } }
        public int Image { get { return image; } }
        public string PrimaryText { get { return primaryText; } }
        public string SecondaryText { get { return secondaryText; } }

    }

}