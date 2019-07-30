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
using Android.Content;

using LettreForAndroid.Receivers;

namespace LettreForAndroid.UI
{
    public class main_page : Fragment
    {
        public static main_page _Instance;

        const string INTENT_CATEGORY = "intentCategory";
        const string INTENT_POSITION = "intentPosition";
        private int _Category;
        private int _Position;

        DialogueSet _DialogueSet;

        TextView _GuideText;
        RecyclerView _RecyclerView;
        RecyclerView.LayoutManager _LayoutManager;

        //새로운 페이지가 만들어질때 호출됨
        public static main_page newInstance(int iPosition, int iCategory)  //어댑터로부터 현재 탭의 정보를 받음. 이것을 args에 저장함. Static이라서 args를 통해 OnCreate로 전달.
        {
            var args = new Bundle();
            args.PutInt(INTENT_CATEGORY, iCategory);
            args.PutInt(INTENT_POSITION, iPosition);

            var fragment = new main_page();
            fragment.Arguments = args;

            return fragment;
        }

        //새로운 페이지가 만들어질때 호출됨
        public override void OnCreate(Bundle iSavedInstanceState)
        {
            base.OnCreate(iSavedInstanceState);

            _Category = Arguments.GetInt(INTENT_CATEGORY);
            _Position = Arguments.GetInt(INTENT_POSITION);

            _Instance = this;
        }

        //페이지를 넘길때마다 호출됨
        public override View OnCreateView(LayoutInflater iInflater, ViewGroup iContainer, Bundle iSavedInstanceState)
        {
            var view = iInflater.Inflate(Resource.Layout.fragment_page, iContainer, false);
            _GuideText = view.FindViewById<TextView>(Resource.Id.fragPage_textView1);
            _RecyclerView = view.FindViewById<RecyclerView>(Resource.Id.fragPage_recyclerView1);

            //RecyclerView에 어댑터 Plug
            _LayoutManager = new LinearLayoutManager(Context);
            _RecyclerView.SetLayoutManager(_LayoutManager);

            refreshRecyclerView();

            return view;
        }

        public void refreshFrag()
        {
            FragmentTransaction a = FragmentManager.BeginTransaction();
            a.Detach(this);
            a.Attach(this);
            a.Commit();
        }

        public void refreshRecyclerView()
        {
            //데이터 준비 : 현재 탭에 해당되는 대화목록을 가져온다.
            _DialogueSet = MessageManager.Get().DialogueSets[_Category];

            //대화가 있으면 리사이클러 뷰 내용안에 표시하도록 함
            if (_DialogueSet.Count > 0)
            {
                DialogueSetAdpater adapter = new DialogueSetAdpater(_DialogueSet);
                _RecyclerView.SetAdapter(adapter);
            }
            else
            {
                //문자가 없으면 없다고 알려준다.
                _GuideText.Visibility = ViewStates.Visible;
                _RecyclerView.Visibility = ViewStates.Gone;
            }
        }

        //----------------------------------------------------------------------
        // DialogueFragAll VIEW HOLDER
        // 전체 탭의 프래그먼트

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
                TextMessage lastMessage = dialogue[0];                           //대화 중 가장 마지막 문자

                mCategoryText.Text = Dialogue._LableTypeStr[dialogue.MajorLable];   //카테고리 설정

                //이름 혹은 연락처 표시, 문자 내용 표시
                mAddress.Text = dialogue.DisplayName;
                mMsg.Text = lastMessage.Msg;

                //날짜 표시
                DateTimeUtillity dtu = new DateTimeUtillity();
                if (dtu.getNow().Year >= dtu.getYear(lastMessage.Time))                                  //올해 메시지이면
                {
                    if (dtu.getDatetime(lastMessage.Time) >= dtu.getToday())
                        mTime.Text = dtu.milisecondToDateTimeStr(lastMessage.Time, "a hh:mm");          //오늘 메시지이면
                    else
                        mTime.Text = dtu.milisecondToDateTimeStr(lastMessage.Time, "MM월 dd일");        //올해인데 오늘 메시지가 아님
                }
                else
                {
                    mTime.Text = dtu.milisecondToDateTimeStr(lastMessage.Time, "yyyy년 MM월 dd일");    //올해 메시지가 아님
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
        // 전체 탭을 제외한 나머지 탭의 프래그먼트

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
                if (dtu.getNow().Year >= dtu.getYear(lastMessage.Time))                                  //올해 메시지이면
                {
                    if (dtu.getDatetime(lastMessage.Time) >= dtu.getToday())
                        mTime.Text = dtu.milisecondToDateTimeStr(lastMessage.Time, "a hh:mm");          //오늘 메시지이면
                    else
                        mTime.Text = dtu.milisecondToDateTimeStr(lastMessage.Time, "MM월 dd일");        //올해인데 오늘 메시지가 아님
                }
                else
                {
                    mTime.Text = dtu.milisecondToDateTimeStr(lastMessage.Time, "yyyy년 MM월 dd일");    //올해 메시지가 아님
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

        public class DialogueSetAdpater : RecyclerView.Adapter
        {
            private const int VIEW_TYPE_ALL = 1;
            private const int VIEW_TYPE_CATEGORY = 2;
            
            // Event handler for item clicks:
            public event System.EventHandler<int> mItemClick;

            // 현 페이지 대화 목록
            public DialogueSet _DialogueSet;

            // Load the adapter with the data set (photo album) at construction time:
            public DialogueSetAdpater(DialogueSet iDialogueSet)
            {
                _DialogueSet = iDialogueSet;
            }

            public void updateDialogueSet(DialogueSet iDialogueSet)
            {
                _DialogueSet = iDialogueSet;
            }

            public override int GetItemViewType(int iPosition)
            {
                if (_DialogueSet.Category == (int)Dialogue.LableType.ALL)
                {
                    return VIEW_TYPE_ALL;
                }
                else if ((int)Dialogue.LableType.ALL < _DialogueSet.Category && _DialogueSet.Category < Dialogue.Lable_COUNT)
                {
                    return VIEW_TYPE_CATEGORY;
                }
                else
                {
                    throw new InvalidProgramException("지정되지 않은 탭 타입!");
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
                throw new InvalidProgramException("지정되지 않은 홀더 타입!");
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
                        a.bind(_DialogueSet[iPosition]);
                        break;
                    case VIEW_TYPE_CATEGORY:
                        DialogueFragCategoryHolder b = iHolder as DialogueFragCategoryHolder;
                        b.bind(_DialogueSet[iPosition]);
                        break;
                }
            }

            // Return the number of photos available in the photo album:
            public override int ItemCount
            {
                get { return _DialogueSet.Count; }
            }

            // 대화를 클릭했을 때 발생하는 메소드
            void OnClick(int iPosition)
            {
                if (mItemClick != null)
                    mItemClick(this, iPosition);

                Android.Content.Context context = Android.App.Application.Context;

                Android.Content.Intent intent = new Android.Content.Intent(context, typeof(DialogueActivity));
                intent.PutExtra("thread_id", _DialogueSet[iPosition].Thread_id);
                intent.PutExtra("category", _DialogueSet.Category);

                context.StartActivity(intent);
            }
        }
    }
}