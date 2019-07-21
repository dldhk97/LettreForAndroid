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
    public class CustomPagerAdapter : FragmentPagerAdapter
    {
        private List<TabFrag> tabs = new List<TabFrag>();
        readonly Context context;

        public CustomPagerAdapter(Context context, FragmentManager fm) : base(fm)
        {
            this.context = context;
        }

        public override int Count
        {
            get { return tabs.Count; }
        }

        //메인문이 모두 끝났을때와, 페이지 넘길때 이 메소드가 호출되어 newInstance가 각 FragmentPage의 내용물을 채움
        public override Fragment GetItem(int position)
        {
            return main_page.newInstance(tabs[position].Category);
        }

        public View GetTabView(int position)
        {
            var view = LayoutInflater.From(context).Inflate(Resource.Layout.custom_tab, null);

            TextView tabTitle = view.FindViewById<TextView>(Resource.Id.custTab_title);
            TextView notiCount = view.FindViewById<TextView>(Resource.Id.custTab_count);
            ImageView notiBackground = view.FindViewById<ImageView>(Resource.Id.custTab_count_background);

            tabTitle.Text = tabs[position].TabTitle;

            DialogueSet curDialogueSet = MessageManager.Get().DialogueSets[position];
            int totalUnreadCnt = 0;
            for (int i = 0; i < curDialogueSet.Count; i++)
            {
                totalUnreadCnt += curDialogueSet[i].UnreadCnt;
            }
            tabs[position].NotiCount = totalUnreadCnt;

            if (tabs[position].NotiCount > 0)
            {
                notiCount.Text = tabs[position].NotiCount.ToString();
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
                tabs.Add(iTabFrags[i]);
        }
    }
}