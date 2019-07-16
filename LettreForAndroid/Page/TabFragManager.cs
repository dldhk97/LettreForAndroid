using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

using LettreForAndroid.Class;

using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace LettreForAndroid.Page
{
    public class TabFragManager
    {

        TabFrag[] tabFrags = 
        {
            new TabFrag(0, "전체", 0),
            new TabFrag(1, "대화", 0),
            new TabFrag(2, "인증", 0),
            new TabFrag(3, "택배", 0),
            new TabFrag(4, "공공기관", 0),
            new TabFrag(5, "카드", 0),
            new TabFrag(6, "스팸", 0),
            new TabFrag(7, "미분류", 0),
        };

        readonly Activity activity;
        readonly FragmentManager fm;
        
        public TabFragManager(Activity iActivity, FragmentManager iFm)
        {
            activity = iActivity;
            fm = iFm;
        }
        // 탭 레이아웃 설정
        public void SetupTabLayout()
        {
            var pager = activity.FindViewById<ViewPager>(Resource.Id.pager);
            var tabLayout = activity.FindViewById<TabLayout>(Resource.Id.sliding_tabs);
            var adapter = new CustomPagerAdapter(activity.BaseContext, fm);

            adapter.AddTab(tabFrags);

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
            tv.SetTypeface(tv.Typeface, Android.Graphics.TypefaceStyle.Bold);
        }
        private void TabLayout_TabUnselected(object sender, TabLayout.TabUnselectedEventArgs e)
        {
            TextView tv = e.Tab.CustomView.FindViewById<TextView>(Resource.Id.custTab_title);
            tv.SetTypeface(null, Android.Graphics.TypefaceStyle.Normal);
        }
    }
}