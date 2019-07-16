using System;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Android.Support.V7.Widget;

using LettreForAndroid.Class;
using LettreForAndroid.Utility;
using System.Collections.Generic;

namespace LettreForAndroid
{
    public class PageFragment : Fragment
    {
        const string ARG_CATEGORY = "ARG_CATEGORY";
        private int currentCategory;

        //리사이클러뷰 연습
        RecyclerView mRecyclerView;
        RecyclerView.LayoutManager mLayoutManager;
        DialogueAdpater mAdapter;

        public static PageFragment newInstance(int category)  //어댑터로부터 현재 탭의 위치, 코드를 받음. 이것을 argument에 저장함. Static이라서 전역변수 못씀.
        {
            var args = new Bundle();
            args.PutInt(ARG_CATEGORY, category);
            var fragment = new PageFragment();
            fragment.Arguments = args;
            return fragment;
        }

        public override void OnCreate(Bundle savedInstanceState)    //newInstance에서 argument에 저장한 값들을 전역변수에 저장시킴. 
        {
            base.OnCreate(savedInstanceState);
            currentCategory = Arguments.GetInt(ARG_CATEGORY);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_page, container, false);
            TextView textView1 = view.FindViewById<TextView>(Resource.Id.fragPage_textView1);
            mRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.fragPage_recyclerView1);

            //현재페이지의 카테고리 번호가 담긴 전역변수에서 값 가져옴.
            string thisCode = currentCategory.ToString();

            //여기부턴 리사이클 뷰

            //데이터 준비
            List<TextMessage> messageList;
            if(currentCategory == (int)TabFrag.CATEGORY.ALL)
            {
                 messageList = MessageManager.Get().getAllMessages();
            }
            else
            {
                messageList = MessageManager.Get().getAllMessages();
            }
            
            Dialogue mDialogue = new Dialogue(messageList);

            //문자가 있으면
            if(MessageManager.Get().Count > 0)
            {
                //어뎁터 준비
                mAdapter = new DialogueAdpater(mDialogue);

                //RecyclerView에 어댑터 Plug
                mRecyclerView.SetAdapter(mAdapter);

                mLayoutManager = new LinearLayoutManager(Context);
                mRecyclerView.SetLayoutManager(mLayoutManager);

                //내 어댑터 Plug In
                mAdapter = new DialogueAdpater(mDialogue);
                mRecyclerView.SetAdapter(mAdapter);
            }
            else
            {
                //문자가 없으면 
                textView1.Visibility = ViewStates.Visible;
                mRecyclerView.Visibility = ViewStates.Gone;
            }

            return view;
        }

        //----------------------------------------------------------------------
        // VIEW HOLDER

        // 뷰홀더 패턴 적용 : 각각의 뷰홀더가 CardView 안에 있는 UI 컴포넨트(이미지뷰와 텍스트뷰)를 참조한다.
        // 그것들은 리사이클러뷰 안의 행으로써 표시됨.
        public class MessageViewHolder : RecyclerView.ViewHolder
        {
            public ImageButton ProfileImage { get; private set; }
            public TextView Address { get; private set; }
            public TextView Msg { get; private set; }
            public TextView Time { get; private set; }
            public ImageView ReadStateIndicator { get; private set; }

            // 카드뷰 레이아웃(message_view) 내 객체들 참조.
            public MessageViewHolder(View itemView, System.Action<int> listener) : base(itemView)
            {
                // Locate and cache view references:
                ProfileImage = itemView.FindViewById<ImageButton>(Resource.Id.mv_profileImage);
                Address = itemView.FindViewById<TextView>(Resource.Id.mv_address);
                Msg = itemView.FindViewById<TextView>(Resource.Id.mv_msg);
                Time = itemView.FindViewById<TextView>(Resource.Id.mv_time);
                ReadStateIndicator = itemView.FindViewById<ImageView>(Resource.Id.mv_readStateIndicator);

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

                //vh.ProfileImage.SetImageResource(mDialogue[position]) //대충 프로필사진으로 때운다는 내용
                vh.Address.Text = mDialogue[position].Address;
                vh.Msg.Text = mDialogue[position].Msg;

                var time = TimeSpan.FromMilliseconds(Convert.ToDouble(mDialogue[position].Time));
                DateTime dt = new DateTime(1970,1,1) + time;
                vh.Time.Text = dt.ToLongDateString() + dt.ToLongTimeString();

                vh.ReadStateIndicator.Visibility = mDialogue[position].ReadState.Equals("0") ? ViewStates.Visible : ViewStates.Invisible;
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