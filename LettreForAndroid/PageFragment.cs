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
        private int mCurrentCategory;

        RecyclerView mRecyclerView;
        RecyclerView.LayoutManager mLayoutManager;
        DialogueListAdpater mAdapter;

        public static PageFragment newInstance(int iCategory)  //어댑터로부터 현재 탭의 위치, 코드를 받음. 이것을 argument에 저장함. Static이라서 전역변수 못씀.
        {
            var args = new Bundle();
            args.PutInt(ARG_CATEGORY, iCategory);
            var fragment = new PageFragment();
            fragment.Arguments = args;
            return fragment;
        }

        public override void OnCreate(Bundle iSavedInstanceState)    //newInstance에서 argument에 저장한 값들을 전역변수에 저장시킴. 
        {
            base.OnCreate(iSavedInstanceState);
            mCurrentCategory = Arguments.GetInt(ARG_CATEGORY);
        }

        public override View OnCreateView(LayoutInflater iInflater, ViewGroup iContainer, Bundle iSavedInstanceState)
        {
            var view = iInflater.Inflate(Resource.Layout.fragment_page, iContainer, false);
            TextView textView1 = view.FindViewById<TextView>(Resource.Id.fragPage_textView1);
            mRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.fragPage_recyclerView1);

            //현재페이지의 카테고리 번호가 담긴 전역변수에서 값 가져옴.

            //여기부턴 리사이클 뷰


            //데이터 준비 : messageList에 카테고리에 해당되는 메세지를 모두 불러온다.
            List<Dialogue> dialogueList = MessageManager.Get().getAllMessages(mCurrentCategory);

            //카테고리에 해당되는 메세지가 담긴 messageList를 mDialogue안에 담는다.

            //문자가 있으면 리사이클러 뷰 내용안에 표시하도록 함
            if(dialogueList.Count > 0)
            {
                //어뎁터 준비
                //mAdapter = new DialogueListAdpater(dialogueList);

                //RecyclerView에 어댑터 Plug
                mRecyclerView.SetAdapter(mAdapter);

                mLayoutManager = new LinearLayoutManager(Context);
                mRecyclerView.SetLayoutManager(mLayoutManager);

                //내 어댑터 Plug In
                mAdapter = new DialogueListAdpater(dialogueList);
                mRecyclerView.SetAdapter(mAdapter);
            }
            else
            {
                //문자가 없으면 없다고 알려준다.
                textView1.Visibility = ViewStates.Visible;
                mRecyclerView.Visibility = ViewStates.Gone;
            }

            return view;
        }

        //----------------------------------------------------------------------
        // VIEW HOLDER

        // 뷰홀더 패턴 적용 : 각각의 뷰홀더가 CardView 안에 있는 UI 컴포넨트(이미지뷰와 텍스트뷰)를 참조한다.
        // 그것들은 리사이클러뷰 안의 행으로써 표시됨.
        public class DialogueViewHolder : RecyclerView.ViewHolder
        {
            public ImageButton mProfileImage { get; private set; }
            public TextView mAddress { get; private set; }
            public TextView mMsg { get; private set; }
            public TextView mTime { get; private set; }
            public ImageView mReadStateIndicator { get; private set; }

            // 카드뷰 레이아웃(message_view) 내 객체들 참조.
            public DialogueViewHolder(View iItemView, System.Action<int> iListener) : base(iItemView)
            {
                // Locate and cache view references:
                mProfileImage = iItemView.FindViewById<ImageButton>(Resource.Id.mv_profileImage);
                mAddress = iItemView.FindViewById<TextView>(Resource.Id.mv_address);
                mMsg = iItemView.FindViewById<TextView>(Resource.Id.mv_msg);
                mTime = iItemView.FindViewById<TextView>(Resource.Id.mv_time);
                mReadStateIndicator = iItemView.FindViewById<ImageView>(Resource.Id.mv_readStateIndicator);

                // Detect user clicks on the item view and report which item
                // was clicked (by layout position) to the listener:
                iItemView.Click += (sender, e) =>
                {
                    iListener(base.LayoutPosition);
                };

            }
        }

        public class DialogueListAdpater : RecyclerView.Adapter
        {

            // Event handler for item clicks:
            public event System.EventHandler<int> mItemClick;
            // 대화 목록
            public List<Dialogue> mDialogueList;

            // Load the adapter with the data set (photo album) at construction time:
            public DialogueListAdpater(List<Dialogue> iDialogueList)
            {
                mDialogueList = iDialogueList;
            }

            // 뷰 홀더 생성
            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup iParent, int iViewType)
            {
                // Inflate the CardView for the photo:
                View itemView = LayoutInflater.From(iParent.Context).
                            Inflate(Resource.Layout.message_view, iParent, false);

                // Create a ViewHolder to find and hold these view references, and 
                // register OnClick with the view holder:
                DialogueViewHolder vh = new DialogueViewHolder(itemView, OnClick);
                return vh;
            }

            // 뷰 홀더에 데이터를 설정하는 부분
            public override void OnBindViewHolder(RecyclerView.ViewHolder iHolder, int iPosition)
            {
                DialogueViewHolder vh = iHolder as DialogueViewHolder;

                //해당 대화와 가장 첫번째 메세지
                Dialogue currentDialogue = mDialogueList[iPosition];
                TextMessage currentMsg = currentDialogue[0];

                //연락처에 있는 사람이면
                if(currentDialogue.Contact != null)
                {
                    if(currentDialogue.Contact.Photo_uri != null)
                        vh.mProfileImage.SetImageURI(Android.Net.Uri.Parse(currentDialogue.Contact.Photo_uri));
                    else
                        vh.mProfileImage.SetImageURI(Android.Net.Uri.Parse("@drawable/dd9_send_256"));
                    vh.mAddress.Text = currentDialogue.Contact.Name;
                }
                else
                {
                    vh.mProfileImage.SetImageURI(Android.Net.Uri.Parse("@drawable/dd9_send_256"));
                    vh.mAddress.Text = currentMsg.Address;
                }

                vh.mMsg.Text = currentMsg.Msg;

                long milTime = currentMsg.Time;
                string pattern = "yyyy-MM-dd HH:mm:ss";
                Java.Text.SimpleDateFormat formatter = new Java.Text.SimpleDateFormat(pattern);
                string date = (string)formatter.Format(new Java.Sql.Timestamp(milTime));
                vh.mTime.Text = date;

                vh.mReadStateIndicator.Visibility = currentMsg.ReadState.Equals("0") ? ViewStates.Visible : ViewStates.Invisible;
            }

            // Return the number of photos available in the photo album:
            public override int ItemCount
            {
                get { return mDialogueList.Count; }
            }

            // Raise an event when the item-click takes place:
            void OnClick(int iPosition)
            {
                if (mItemClick != null)
                    mItemClick(this, iPosition);
                Console.WriteLine(mDialogueList[iPosition][0].Msg);
            }
        }
    }
}