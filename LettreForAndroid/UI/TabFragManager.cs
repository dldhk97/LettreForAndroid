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

namespace LettreForAndroid.UI
{
    public class TabFragManager
    {
        TabFrag[] tabFrags = 
        {
            new TabFrag(Dialogue._LableTypeStr[(int)Dialogue.LableType.ALL], (int)Dialogue.LableType.ALL, 0, 0),                         //전체
            new TabFrag(Dialogue._LableTypeStr[(int)Dialogue.LableType.COMMON], (int)Dialogue.LableType.COMMON, 1, 0),                   //대화
            new TabFrag(Dialogue._LableTypeStr[(int)Dialogue.LableType.DELIVERY], (int)Dialogue.LableType.DELIVERY, 2, 0),               //택배
            new TabFrag(Dialogue._LableTypeStr[(int)Dialogue.LableType.CARD], (int)Dialogue.LableType.CARD, 3, 0),                       //카드
            new TabFrag(Dialogue._LableTypeStr[(int)Dialogue.LableType.IDENTIFICATION], (int)Dialogue.LableType.IDENTIFICATION, 4, 0),   //인증
            new TabFrag(Dialogue._LableTypeStr[(int)Dialogue.LableType.PUBLIC], (int)Dialogue.LableType.PUBLIC, 5, 0),                   //공공기관
            new TabFrag(Dialogue._LableTypeStr[(int)Dialogue.LableType.AGENCY], (int)Dialogue.LableType.AGENCY, 6, 0),                   //통신사
            new TabFrag(Dialogue._LableTypeStr[(int)Dialogue.LableType.UNKNOWN], (int)Dialogue.LableType.UNKNOWN, 7, 0),                       //스팸
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