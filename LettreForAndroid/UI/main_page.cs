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

namespace LettreForAndroid.UI
{
    public class main_page : Fragment
    {
        const string ARG_CATEGORY = "ARG_CATEGORY";
        private int mCategory;

        RecyclerView mRecyclerView;
        RecyclerView.LayoutManager mLayoutManager;
        DialogueSetAdpater mAdapter;

        public static main_page newInstance(int iCategory)  //어댑터로부터 현재 탭의 위치, 코드를 받음. 이것을 argument에 저장함. Static이라서 전역변수 못씀.
        {
            var args = new Bundle();
            args.PutInt(ARG_CATEGORY, iCategory);
            var fragment = new main_page();
            fragment.Arguments = args;
            return fragment;
        }

        public override void OnCreate(Bundle iSavedInstanceState)    //newInstance에서 argument에 저장한 값들을 전역변수에 저장시킴. 
        {
            base.OnCreate(iSavedInstanceState);
            mCategory = Arguments.GetInt(ARG_CATEGORY);
        }

        public override View OnCreateView(LayoutInflater iInflater, ViewGroup iContainer, Bundle iSavedInstanceState)
        {
            var view = iInflater.Inflate(Resource.Layout.fragment_page, iContainer, false);
            TextView textView1 = view.FindViewById<TextView>(Resource.Id.fragPage_textView1);
            mRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.fragPage_recyclerView1);

            //여기부턴 리사이클 뷰

            //데이터 준비 : messageList에 카테고리에 해당되는 메세지를 모두 불러온다.
            DialogueSet dialogueSet;
            dialogueSet = MessageManager.Get().DialogueSets[mCategory];

            //카테고리에 해당되는 메세지가 담긴 messageList를 mDialogue안에 담는다.

            //문자가 있으면 리사이클러 뷰 내용안에 표시하도록 함
            if (dialogueSet.Count > 0)
            {

                //RecyclerView에 어댑터 Plug
                mRecyclerView.SetAdapter(mAdapter);

                mLayoutManager = new LinearLayoutManager(Context);
                mRecyclerView.SetLayoutManager(mLayoutManager);

                //내 어댑터 Plug In
                mAdapter = new DialogueSetAdpater(dialogueSet);
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
        // DialogueFragAll VIEW HOLDER

        public class DialogueFragAllHolder : RecyclerView.ViewHolder
        {
            public TextView mCategoryText { get; private set; }
            public View mSpliter { get; private set; }
            public TextView mAddress { get; private set; }
            public TextView mMsg { get; private set; }
            public TextView mTime { get; private set; }
            public RelativeLayout mReadStateRL { get; private set; }
            public TextView mReadStateCnt { get; private set; }

            // 카드뷰 레이아웃(dialogue_frag) 내 객체들 참조.
            public DialogueFragAllHolder(View iItemView, System.Action<int> iListener) : base(iItemView)
            {
                mCategoryText = iItemView.FindViewById<TextView>(Resource.Id.dfa_categoryTV);
                mSpliter = iItemView.FindViewById<View>(Resource.Id.dfa_splitterV);
                mAddress = iItemView.FindViewById<TextView>(Resource.Id.dfa_addressTV);
                mMsg = iItemView.FindViewById<TextView>(Resource.Id.dfa_msgTV);
                mTime = iItemView.FindViewById<TextView>(Resource.Id.dfa_timeTV);
                mReadStateRL = iItemView.FindViewById<RelativeLayout>(Resource.Id.dfa_readstateRL);
                mReadStateCnt = iItemView.FindViewById<TextView>(Resource.Id.dfa_readstateCntTV);

                iItemView.Click += (sender, e) =>
                {
                    iListener(base.LayoutPosition);
                };
            }

            public void bind(Dialogue dialogue)
            {
                TextMessage lastMessage = dialogue[0];  //대화 중 가장 마지막 문자

                mCategoryText.Text = TabFrag.mCategory_Str[dialogue.Category];   //카테고리 설정

                //이름 혹은 연락처 표시, 문자 내용 표시
                mAddress.Text = dialogue.DisplayName;
                mMsg.Text = lastMessage.Msg;

                //날짜 표시
                DateTimeUtillity dtu = new DateTimeUtillity();
                if (dtu.getCurrentYear() > dtu.getYear(lastMessage.Time))         //올해 메세지가 아니면
                {
                    mTime.Text = dtu.milisecondToDateTimeStr(lastMessage.Time, "yyyy년 MM월 dd일");
                }
                else
                {
                    mTime.Text = dtu.milisecondToDateTimeStr(lastMessage.Time, "MM월 dd일");
                }

                //문자 읽음 여부에 따른 상태표시기 표시여부 및 카운트설정
                if (dialogue.UnreadCnt > 0)
                {
                    mReadStateRL.Visibility = ViewStates.Visible;
                    mReadStateCnt.Text = dialogue.UnreadCnt.ToString();
                }
                else
                {
                    mReadStateRL.Visibility = ViewStates.Invisible;
                }
            }
        }

        //----------------------------------------------------------------------
        // DialogueFragCategory VIEW HOLDER

        public class DialogueFragCategoryHolder : RecyclerView.ViewHolder
        {
            public ImageButton mProfileImage { get; private set; }
            public TextView mAddress { get; private set; }
            public TextView mMsg { get; private set; }
            public TextView mTime { get; private set; }
            public RelativeLayout mReadStateRL { get; private set; }
            public TextView mReadStateCnt { get; private set; }

            private int mCategory;
            public int Category
            {
                set { mCategory = value; }
            }

            // 카드뷰 레이아웃(dialogue_frag) 내 객체들 참조.
            public DialogueFragCategoryHolder(View iItemView, System.Action<int> iListener) : base(iItemView)
            {
                mProfileImage = iItemView.FindViewById<ImageButton>(Resource.Id.dfc_profileIB);
                mAddress = iItemView.FindViewById<TextView>(Resource.Id.dfc_addressTV);
                mMsg = iItemView.FindViewById<TextView>(Resource.Id.dfc_msgTV);
                mTime = iItemView.FindViewById<TextView>(Resource.Id.dfc_timeTV);
                mReadStateRL = iItemView.FindViewById<RelativeLayout>(Resource.Id.dfc_readstateRL);
                mReadStateCnt = iItemView.FindViewById<TextView>(Resource.Id.dfc_readstateCntTV);

                iItemView.Click += (sender, e) =>
                {
                    iListener(base.LayoutPosition);
                };

            }

            public void bind(Dialogue dialogue)
            {
                TextMessage lastMessage = dialogue[0];
                //전체 탭이 아닌 경우
                //연락처에 있는 사람이면
                if (dialogue.Contact != null)
                {
                    //연락처에 사진이 있다면 사진으로 대체
                    if (dialogue.Contact.PhotoThumnail_uri != null)
                        mProfileImage.SetImageURI(Android.Net.Uri.Parse(dialogue.Contact.PhotoThumnail_uri));
                    else
                        mProfileImage.SetImageURI(Android.Net.Uri.Parse("@drawable/dd9_send_256"));
                }
                else
                {
                    //연락처에 사진이 없으면 기본사진으로 설정
                    mProfileImage.SetImageURI(Android.Net.Uri.Parse("@drawable/dd9_send_256"));
                }

                //이름 혹은 연락처 표시, 문자 내용 표시
                mAddress.Text = dialogue.DisplayName;
                mMsg.Text = lastMessage.Msg;

                //날짜 표시
                DateTimeUtillity dtu = new DateTimeUtillity();
                if (dtu.getCurrentYear() > dtu.getYear(lastMessage.Time))         //올해 메세지가 아니면
                {
                    mTime.Text = dtu.milisecondToDateTimeStr(lastMessage.Time, "yyyy년 MM월 dd일");
                }
                else
                {
                    mTime.Text = dtu.milisecondToDateTimeStr(lastMessage.Time, "MM월 dd일");
                }

                //문자 읽음 여부에 따른 상태표시기 표시여부 및 카운트설정
                if(dialogue.UnreadCnt > 0)
                {
                    mReadStateRL.Visibility = ViewStates.Visible;
                    mReadStateCnt.Text = dialogue.UnreadCnt.ToString();
                }
                else
                {
                    mReadStateRL.Visibility = ViewStates.Invisible;
                }
            }
        }

        



        public class DialogueSetAdpater : RecyclerView.Adapter
        {
            private const int VIEW_TYPE_ALL = 1;
            private const int VIEW_TYPE_CATEGORY = 2;
            
            // Event handler for item clicks:
            public event System.EventHandler<int> mItemClick;

            // 현 페이지 대화 목록
            public DialogueSet mDialogueSet;

            // Load the adapter with the data set (photo album) at construction time:
            public DialogueSetAdpater(DialogueSet iDialogueSet)
            {
                mDialogueSet = iDialogueSet;
            }

            public override int GetItemViewType(int iPosition)
            {
                if (mDialogueSet.Category == (int)TabFrag.CATEGORY.ALL)
                {
                    return VIEW_TYPE_ALL;
                }
                else if ((int)TabFrag.CATEGORY.ALL < mDialogueSet.Category && mDialogueSet.Category < TabFrag.CATEGORY_COUNT)
                {
                    return VIEW_TYPE_CATEGORY;
                }
                else
                {
                    return -1;  //error 체크하셈
                }
            }

            // 뷰 홀더 생성
            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup iParent, int iViewType)
            {
                View itemView;

                if (iViewType == VIEW_TYPE_ALL)
                {
                    itemView = LayoutInflater.From(iParent.Context).Inflate(Resource.Layout.dialogue_frag_all, iParent, false);
                    return new DialogueFragAllHolder(itemView, OnClick);
                }
                else if (iViewType == VIEW_TYPE_CATEGORY)
                {
                    itemView = LayoutInflater.From(iParent.Context).Inflate(Resource.Layout.dialogue_frag_category, iParent, false);
                    return new DialogueFragCategoryHolder(itemView, OnClick);
                }
                return null;
            }

            // 뷰 홀더에 데이터를 설정하는 부분
            public override void OnBindViewHolder(RecyclerView.ViewHolder iHolder, int iPosition)
            {
                if (iHolder == null)
                    return;

                switch (GetItemViewType(iPosition))
                {
                    case VIEW_TYPE_ALL:
                        DialogueFragAllHolder a = iHolder as DialogueFragAllHolder;
                        a.bind(mDialogueSet[iPosition]);
                        break;
                    case VIEW_TYPE_CATEGORY:
                        DialogueFragCategoryHolder b = iHolder as DialogueFragCategoryHolder;
                        b.bind(mDialogueSet[iPosition]);
                        break;
                }

            }

            // Return the number of photos available in the photo album:
            public override int ItemCount
            {
                get { return mDialogueSet.Count; }
            }

            // 대화를 클릭했을 때 발생하는 메소드
            void OnClick(int iPosition)
            {
                if (mItemClick != null)
                    mItemClick(this, iPosition);
                //Console.WriteLine(mDialogueList[iPosition][0].Msg);

                Android.Content.Intent intent = new Android.Content.Intent(Android.App.Application.Context, typeof(dialogue_page));
                intent.PutExtra("position", iPosition);
                intent.PutExtra("category", mDialogueSet.Category);

                Android.App.Application.Context.StartActivity(intent);
            }
        }
    }
}