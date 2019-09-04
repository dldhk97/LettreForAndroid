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
            new TabFrag(Dialogue._LableTypeStr[(int)Dialogue.LableType.ETC], (int)Dialogue.LableType.ETC, 7, 0),                          //기타
            new TabFrag(Dialogue._LableTypeStr[(int)Dialogue.LableType.UNKNOWN], (int)Dialogue.LableType.UNKNOWN, 8, 0),                 //미분류
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

            UpdateNotiCount();

            //모든 탭에 커스텀 뷰 적용
            for (int i = 0; i < _TabLayout.TabCount; i++)
            {
                TabLayout.Tab tab = _TabLayout.GetTabAt(tabFrags[i].Position);

                tab.SetCustomView(_Adapter.GetTabView(tabFrags[i].Position));
            }
            _TabLayout.TabSelected += TabLayout_TabSelected;
            _TabLayout.TabUnselected += TabLayout_TabUnselected;
        }

        private void UpdateNotiCount()
        {
            foreach(TabFrag objTab in tabFrags)
            {
                int totalUnreadCnt = 0;
                DialogueSet objDialogueSet;

                if (objTab.Category == (int)Dialogue.LableType.ALL)
                    objDialogueSet = MessageDBManager.Get().TotalDialogueSet;
                else if (objTab.Category == (int)Dialogue.LableType.UNKNOWN)
                    objDialogueSet = MessageDBManager.Get().UnknownDialogueSet;
                else
                    objDialogueSet = MessageDBManager.Get().DialogueSets[objTab.Category];

                foreach(Dialogue objDialogue in objDialogueSet.DialogueList.Values)
                {
                    totalUnreadCnt += objDialogue.UnreadCnt;
                }
                objTab.NotiCount = totalUnreadCnt;
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

        public void RefreshTabLayout()
        {
            UpdateNotiCount();
            //모든 탭 새로고침
            for (int i = 0; i < _TabLayout.TabCount; i++)
            {
                TabLayout.Tab tab = _TabLayout.GetTabAt(tabFrags[i].Position);

                tab.SetCustomView(null);
                tab.SetCustomView(_Adapter.GetTabView(tabFrags[i].Position));

            }

            //대화목록 새로고침
            for (int i = 0; i < CustomPagerAdapter._Pages.Count; i++)
            {
                CustomPagerAdapter._Pages[i].RefreshRecyclerView();
                if (DialogueActivity._Instance == null)
                    CustomPagerAdapter._Pages[i].RefreshFrag();
            }
        }
    }
}