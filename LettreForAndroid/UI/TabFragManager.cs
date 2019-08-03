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
using LettreForAndroid.Utility;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace LettreForAndroid.UI
{
    public class TabFragManager
    {
        List<TabFrag> tabFrags = new List<TabFrag>()
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

        public static TabFragManager _Instance;
        TabLayout _TabLayout;
        CustomPagerAdapter _Adapter;

        public TabFragManager(Activity iActivity, FragmentManager iFm)
        {
            activity = iActivity;
            fm = iFm;
            _Instance = this;
        }
        // 탭 레이아웃 설정
        public void SetupTabLayout()
        {
            var viewPager = activity.FindViewById<ViewPager>(Resource.Id.ma_pager);
            _TabLayout = activity.FindViewById<TabLayout>(Resource.Id.ma_sliding_tabs);
            _Adapter = new CustomPagerAdapter(activity.BaseContext, fm);

            _Adapter.AddTab(tabFrags);

            // Set adapter to view ma_pager
            viewPager.Adapter = _Adapter;

            // Setup tablayout with view ma_pager
            _TabLayout.SetupWithViewPager(viewPager);

            CountNoti();

            //모든 탭에 커스텀 뷰 적용
            for (int i = 0; i < _TabLayout.TabCount; i++)
            {
                TabLayout.Tab tab = _TabLayout.GetTabAt(i);

                tab.SetCustomView(_Adapter.GetTabView(i));
            }
            _TabLayout.TabSelected += TabLayout_TabSelected;
            _TabLayout.TabUnselected += TabLayout_TabUnselected;
        }

        private void CountNoti()
        {
            for(int i = 0; i < tabFrags.Count; i++)
            {
                int totalUnreadCnt = 0;
                DialogueSet objDialogueSet = MessageDBManager.Get().DialogueSets[i];

                for (int j = 0; j < objDialogueSet.Count; j++)
                {
                    totalUnreadCnt += objDialogueSet[j].UnreadCnt;
                }
                tabFrags[i].NotiCount = totalUnreadCnt;
            }
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

        public void RefreshLayout()
        {
            CountNoti();
            //모든 탭 새로고침
            for (int i = 0; i < _TabLayout.TabCount; i++)
            {
                TabLayout.Tab tab = _TabLayout.GetTabAt(i);

                tab.SetCustomView(null);
                tab.SetCustomView(_Adapter.GetTabView(i));

            }

            //대화목록 새로고침
            for (int i = 0; i < CustomPagerAdapter._Pages.Count; i++)
            {
                CustomPagerAdapter._Pages[i].refreshRecyclerView();
                if (DialogueActivity._Instance == null)
                    CustomPagerAdapter._Pages[i].refreshFrag();
            }
        }
    }
}