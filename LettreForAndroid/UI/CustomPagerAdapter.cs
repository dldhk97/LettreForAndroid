﻿using System;
using Android.Content;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Java.Lang;

using System.Collections.Generic;

using LettreForAndroid.Class;
using LettreForAndroid.Utility;

namespace LettreForAndroid.UI
{
    public class CustomPagerAdapter : FragmentPagerAdapter
    {
        private List<TabFrag> _Tabs = new List<TabFrag>();
        public static List<main_page> _Pages = new List<main_page>();
        readonly Context _Context;

        public CustomPagerAdapter(Context context, FragmentManager fm) : base(fm)
        {
            this._Context = context;
        }

        public override int Count
        {
            get { return _Tabs.Count; }
        }

        //새로운 탭이 만들어질때 호출됨.
        public override Fragment GetItem(int position)
        {
            main_page fragPage = main_page.newInstance(position, _Tabs[position].Category);
            _Pages.Add(fragPage);
            return fragPage;
        }


        public View GetTabView(int position)
        {
            var view = LayoutInflater.From(_Context).Inflate(Resource.Layout.custom_tab, null);

            TextView tabTitle = view.FindViewById<TextView>(Resource.Id.custTab_title);
            TextView notiCount = view.FindViewById<TextView>(Resource.Id.custTab_count);
            ImageView notiBackground = view.FindViewById<ImageView>(Resource.Id.custTab_count_background);

            tabTitle.Text = _Tabs[position].TabTitle;

            DialogueSet curDialogueSet = MessageManager.Get().DialogueSets[position];

            int totalUnreadCnt = 0;
            for (int i = 0; i < curDialogueSet.Count; i++)
            {
                totalUnreadCnt += curDialogueSet[i].UnreadCnt;
            }
            _Tabs[position].NotiCount = totalUnreadCnt;

            if (_Tabs[position].NotiCount > 0)
            {
                notiCount.Text = _Tabs[position].NotiCount.ToString();
                notiCount.Visibility = ViewStates.Visible;
                notiBackground.Visibility = ViewStates.Visible;
            }
            else
            {
                notiCount.Visibility = ViewStates.Gone;
                notiBackground.Visibility = ViewStates.Gone;
            }

            return view;
        }

        public void AddTab(TabFrag[] iTabFrags)
        {
            for(int i = 0; i < iTabFrags.Length; i++)
                _Tabs.Add(iTabFrags[i]);
        }
    }
}