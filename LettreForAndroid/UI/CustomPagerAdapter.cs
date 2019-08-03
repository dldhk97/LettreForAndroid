using System;
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
    public class Customma_pagerAdapter : FragmentPagerAdapter
    {
        private List<TabFrag> _Tabs = new List<TabFrag>();
        public static List<MainFragActivity> _Pages = new List<MainFragActivity>();
        readonly Context _Context;

        public Customma_pagerAdapter(Context context, FragmentManager fm) : base(fm)
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
            MainFragActivity fragPage = MainFragActivity.newInstance(position, _Tabs[position].Category);
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

        public void AddTab(List<TabFrag> iTabFrags)
        {
            for(int i = 0; i < iTabFrags.Count; i++)
                _Tabs.Add(iTabFrags[i]);
        }
    }
}