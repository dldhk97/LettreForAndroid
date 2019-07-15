using System;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Android.Support.V7.Widget;
using LettreForAndroid.Class;

namespace LettreForAndroid
{
    public class PageFragment : Fragment
    {
        const string ARG_PAGE = "ARG_PAGE";
        private int mPage;

        //리사이클러뷰 연습
        RecyclerView mRecyclerView;
        RecyclerView.LayoutManager mLayoutManager;
        DialogueAdpater mAdapter;
        Dialogue mDialogue;


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

            //여기부턴 리사이클 뷰 연습

            //데이터 준비
            mDialogue = new Dialogue();

            //어뎁터 준비
            mAdapter = new DialogueAdpater(mDialogue);

            //RecyclerView 가져오기
            mRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.fragPage_recyclerView1);

            //RecyclerView에 어댑터 Plug
            mRecyclerView.SetAdapter(mAdapter);

            mLayoutManager = new LinearLayoutManager(Context);
            mRecyclerView.SetLayoutManager(mLayoutManager);

            //내 어댑터 Plug In
            mAdapter = new DialogueAdpater(mDialogue);
            mRecyclerView.SetAdapter(mAdapter);

            return view;
        }

        //----------------------------------------------------------------------
        // VIEW HOLDER

        // 뷰홀더 패턴 적용 : 각각의 뷰홀더가 CardView 안에 있는 UI 컴포넨트(이미지뷰와 텍스트뷰)를 참조한다.
        // 그것들은 리사이클러뷰 안의 행으로써 표시됨.
        public class MessageViewHolder : RecyclerView.ViewHolder
        {
            public ImageButton ImageButton { get; private set; }
            public TextView Address { get; private set; }
            public TextView Msg { get; private set; }

            // 카드뷰 레이아웃(message_view) 내 객체들 참조.
            public MessageViewHolder(View itemView, System.Action<int> listener) : base(itemView)
            {
                // Locate and cache view references:
                ImageButton = itemView.FindViewById<ImageButton>(Resource.Id.mv_profileImage);
                Address = itemView.FindViewById<TextView>(Resource.Id.mv_address);
                Msg = itemView.FindViewById<TextView>(Resource.Id.mv_msg);

                // Detect user clicks on the item view and report which item
                // was clicked (by layout position) to the listener:
                itemView.Click += (sender, e) => listener(base.LayoutPosition);
            }
        }

        public class DialogueAdpater : RecyclerView.Adapter
        {
            // Event handler for item clicks:
            public event System.EventHandler<int> ItemClick;

            // Underlying data set (a photo album):
            public Dialogue mDialogue;

            // Load the adapter with the data set (photo album) at construction time:
            public DialogueAdpater(Dialogue dialogue)
            {
                mDialogue = dialogue;
            }

            // Create a new photo CardView (invoked by the layout manager): 
            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                // Inflate the CardView for the photo:
                View itemView = LayoutInflater.From(parent.Context).
                            Inflate(Resource.Layout.message_view, parent, false);

                // Create a ViewHolder to find and hold these view references, and 
                // register OnClick with the view holder:
                MessageViewHolder vh = new MessageViewHolder(itemView, OnClick);
                return vh;
            }

            // Fill in the contents of the photo card (invoked by the layout manager):
            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                MessageViewHolder vh = holder as MessageViewHolder;

                // Set the ImageView and TextView in this ViewHolder's CardView 
                // from this position in the photo album:
                //vh.Image.SetImageResource(mDialogue[position].Id);
                vh.Address.Text = mDialogue[position].Address;
                vh.Msg.Text = mDialogue[position].Msg;
            }

            // Return the number of photos available in the photo album:
            public override int ItemCount
            {
                get { return mDialogue.NumMessages; }
            }

            // Raise an event when the item-click takes place:
            void OnClick(int position)
            {
                if (ItemClick != null)
                    ItemClick(this, position);
            }
        }
    }
}