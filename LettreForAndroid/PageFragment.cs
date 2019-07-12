using System;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Android.Support.V7.Widget;

namespace LettreForAndroid
{
    public class PageFragment : Fragment
    {
        const string ARG_PAGE = "ARG_PAGE";
        private int mPage;

        public static PageFragment newInstance(int page)
        {
            var args = new Bundle();
            args.PutInt(ARG_PAGE, page);
            var fragment = new PageFragment();
            fragment.Arguments = args;
            return fragment;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            mPage = Arguments.GetInt(ARG_PAGE);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_page, container, false);
            //TextView textView1 = view.FindViewById<TextView>(Resource.Id.fragPage_textView1);
            //textView1.Text = "페이지 #" + mPage;

            //RecyclerView 초기화
            RecyclerView recyclerView = view.FindViewById<RecyclerView>(Resource.Id.fragPage_recyclerView1);
            recyclerView.SetLayoutManager(new LinearLayoutManager(view.Context));

            return view;
        }
    }
}