using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;

using LettreForAndroid.Utility;

namespace LettreForAndroid.UI
{
    [Activity(Label = "WelcomeActivity2", Theme = "@style/BasicTheme")]
    class WelcomeActivity2 : AppCompatActivity
    {
        private NonSwipeableViewPager _ViewPager;
        private Button _NextBtn;

        private enum WELCOME_SCREEN { WELCOME = 0, PRIVACY, PERMISSION, PACKAGE, CATEGORIZE };

        List<Screen> _Screens = new List<Screen>()
        {
            new Screen(Resource.Layout.welcome_screen, Resource.Drawable.main_icon_drawable_512, "환영합니다!", 
                "계속 버튼을 눌러 진행해주세요", Application.Context.Resources.GetColor(Resource.Color.colorBackground_welcome1)),
            new Screen(Resource.Layout.welcome_screen, Resource.Drawable.privacy_icon, "개인정보취급방침 동의", 
                "개인정보 취급방침 내용과, 동의 버튼 다이얼로그 표시", Application.Context.Resources.GetColor(Resource.Color.colorBackground_welcome2)),
            new Screen(Resource.Layout.welcome_screen, Resource.Drawable.permission_icon, "권한이 필요합니다!", 
                "메시지 수발신, 연락처 조회 등의 권한이 필요합니다.", Application.Context.Resources.GetColor(Resource.Color.colorBackground_welcome3)),
            new Screen(Resource.Layout.welcome_screen, Resource.Drawable.main_icon_drawable_512, "기본앱으로 지정해주세요!",
                "메시지 수신을 위해 기본앱으로 지정해야 합니다.", Application.Context.Resources.GetColor(Resource.Color.colorBackground_welcome4)),
            new Screen(Resource.Layout.welcome_screen, Resource.Drawable.categorize_icon, "카테고리 분류",
                "문자 메시지들을 분류합니다.", Application.Context.Resources.GetColor(Resource.Color.colorBackground_welcome5)),
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //처음이 아닌 경우
            if(DataStorageManager.loadBoolData(this, "isFirst", false) == false)
            {
                //권한이 다 있나?
                //기본앱으로 되어있나?
            }

            SetContentView(Resource.Layout.WelcomeActivity2);

            //뷰페이저 설정
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
            switch (_ViewPager.CurrentItem)
            {
                case (int)WELCOME_SCREEN.WELCOME:
                    _ViewPager.SetCurrentItem(_ViewPager.CurrentItem + 1, true);
                    break;
                case (int)WELCOME_SCREEN.PRIVACY:
                    _ViewPager.SetCurrentItem(_ViewPager.CurrentItem + 1, true);
                    break;
                case (int)WELCOME_SCREEN.PERMISSION:
                    _ViewPager.SetCurrentItem(_ViewPager.CurrentItem + 1, true);
                    break;
                case (int)WELCOME_SCREEN.PACKAGE:
                    _ViewPager.SetCurrentItem(_ViewPager.CurrentItem + 1, true);
                    break;
                case (int)WELCOME_SCREEN.CATEGORIZE:
                    Finish();
                    break;
            }
        }
    }

    //----------------------------------------------------------------------------------------------
    // UI

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

            rootRL.SetBackgroundColor(_Screens[position].BackgroundColor);
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

        public Screen(int layout, int image, string primaryText, string secondaryText, Android.Graphics.Color backgroundColor)
        {
            this.layout = layout;
            this.image = image;
            this.primaryText = primaryText;
            this.secondaryText = secondaryText;
            this.backgroundColor = backgroundColor;
        }

        public int Layout { get { return layout; } }
        public int Image { get { return image; } }
        public string PrimaryText { get { return primaryText; } }
        public string SecondaryText { get { return secondaryText; } }
        public Android.Graphics.Color BackgroundColor { get { return backgroundColor; } }

    }

}