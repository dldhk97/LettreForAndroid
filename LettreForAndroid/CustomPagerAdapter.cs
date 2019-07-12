using System;
using Android.Content;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Java.Lang;

using System.Collections.Generic;
using LettreForAndroid.Class;

namespace LettreForAndroid
{
    public class CustomPagerAdapter : FragmentPagerAdapter
    {
        private List<TabFrag> tabs = new List<TabFrag>();
        readonly Context context;

        public CustomPagerAdapter(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public CustomPagerAdapter(Context context, FragmentManager fm) : base(fm)
        {
            this.context = context;
        }

        public override int Count
        {
            get { return tabs.Count; }
        }

        public override Fragment GetItem(int position)
        {
            return PageFragment.newInstance(position + 1);
        }

        public View GetTabView(int position)
        {
            var view = LayoutInflater.From(context).Inflate(Resource.Layout.custom_tab, null);

            TextView tabTitle = view.FindViewById<TextView>(Resource.Id.custTab_title);
            TextView count = view.FindViewById<TextView>(Resource.Id.custTab_count);

            tabTitle.Text = tabs[position].TabTitle;
            count.Text = tabs[position].NotiCount.ToString();

            return view;
        }

        //public void AddTab(TabFrag tabFrag)
        //{
        //    tabs.Add(tabFrag);
        //}

        public void initTabs()
        {
            tabs.Add(new TabFrag("전체", 0));
            tabs.Add(new TabFrag("대화", 2));
            tabs.Add(new TabFrag("택배", 3));
            tabs.Add(new TabFrag("공공기관", 4));
            tabs.Add(new TabFrag("인증", 5));
            tabs.Add(new TabFrag("카드", 77));
            tabs.Add(new TabFrag("스팸", 100));   //99개 이상일땐 99개로 표시해야하나?
            tabs.Add(new TabFrag("기타", 99));
        }
    }
}