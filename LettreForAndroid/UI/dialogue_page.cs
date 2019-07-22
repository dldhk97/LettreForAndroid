using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

using LettreForAndroid.Class;
using LettreForAndroid.Utility;

using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace LettreForAndroid.UI
{
    [Activity(Label = "dialogue_page", Theme = "@style/NulltActivity")]
    public class dialogue_page : AppCompatActivity
    {
        private int curPosition;
        private int curCategory;
        Dialogue curDialogue;

        Toolbar mToolbar;

        RecyclerView mRecyclerView;
        HeaderModelAdpater mAdapter;

        List<HeaderModel> recyclerViewItems;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.dialogue_page);

            mToolbar = FindViewById<Toolbar>(Resource.Id.my_toolbar);
            SetupToolbar();

            //여기부턴 리사이클 뷰

            mRecyclerView = FindViewById<RecyclerView>(Resource.Id.dp_recyclerView1);

            //데이터 준비 : curDialogue에 해당되는 대화를 불러옴
            curDialogue = MessageManager.Get().DialogueSets[curCategory][curPosition];

            initItem();

            //문자가 있으면 리사이클러 뷰 내용안에 표시하도록 함
            if (recyclerViewItems.Count > 0)
            {
                //어뎁터 준비
                mAdapter = new HeaderModelAdpater(recyclerViewItems);

                //RecyclerView에 어댑터 Plug
                mRecyclerView.SetAdapter(mAdapter);

                LinearLayoutManager mLayoutManager = new LinearLayoutManager(Application.Context);
                mLayoutManager.ReverseLayout = true;
                mLayoutManager.StackFromEnd = true;

                mRecyclerView.SetLayoutManager(mLayoutManager);

                //내 어댑터 Plug In
                mAdapter = new HeaderModelAdpater(recyclerViewItems);
                mRecyclerView.SetAdapter(mAdapter);
                mRecyclerView.ScrollToPosition(0);
            }
            else
            {
                //문자가 없으면 없다고 알려준다. 여긴 버그 영역임...
                //문자가 없는데 어케 대화페이지 들어옴
                throw new InvalidProgramException("어케들어왔노");
            }

        }

        public void initItem()
        {
            string prevTime = "NULL";
            recyclerViewItems = new List<HeaderModel>();

            for (int i = 0; i < curDialogue.Count; i++)
            {
                DateTimeUtillity dtu = new DateTimeUtillity();
                string objTime = dtu.milisecondToDateTimeStr(curDialogue[i].Time, "yyyy-MM-dd");
                if (prevTime != objTime)
                {
                    if (prevTime != "NULL")
                    {
                        HeaderModel a = new HeaderModel();
                        a.Header = prevTime;
                        recyclerViewItems.Add(a);
                    }

                    ChildModel c = new ChildModel();
                    c.TextMessage = curDialogue[i];
                    recyclerViewItems.Add(c);
                    prevTime = objTime;
                }
                else
                {
                    ChildModel b = new ChildModel();
                    b.TextMessage = curDialogue[i];
                    recyclerViewItems.Add(b);
                }
            }
            HeaderModel d = new HeaderModel();
            d.Header = prevTime;
            recyclerViewItems.Add(d);
        }



        //-----------------------------------------------------------------
        //툴바 설정
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                Finish();
                //OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_fade_out);     //창 닫을때 페이드효과
                OverridePendingTransition(0, 0);     //창 닫을때 효과 없음
            }
            return base.OnOptionsItemSelected(item);
        }

        //툴바에 메뉴 추가
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.toolbar_dialogue, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        private void SetupToolbar()
        {
            //Toolbar will now take on default actionbar characteristics
            SetSupportActionBar(mToolbar);

            curPosition = Intent.GetIntExtra("position", -1);
            curCategory = Intent.GetIntExtra("category", -1);

            DialogueSet a = MessageManager.Get().DialogueSets[curCategory];
            Dialogue b = a[curPosition];
            SupportActionBar.Title = b.DisplayName;

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
        }
    }

    //----------------------------------------------------------------------
    // HEADER VIEW HOLDER

    public class HeaderHolder : RecyclerView.ViewHolder
    {
        public TextView mTime { get; private set; }

        // 카드뷰 레이아웃(message_view) 내 객체들 참조.
        public HeaderHolder(View iItemView, System.Action<int> iListener) : base(iItemView)
        {
            mTime = iItemView.FindViewById<TextView>(Resource.Id.mfh_timeTV);

            iItemView.Click += (sender, e) =>
            {
                iListener(base.LayoutPosition);
            };
        }

        public void bind(List<HeaderModel> list, int iPosition)
        {
            HeaderModel headerObj = list[iPosition];

            mTime.Text = headerObj.Header;
        }
    }

    //----------------------------------------------------------------------
    // ReceivedMessage VIEW HOLDER

    // 뷰홀더 패턴 적용 : 각각의 뷰홀더가 CardView 안에 있는 UI 컴포넨트(이미지뷰와 텍스트뷰)를 참조한다.
    // 그것들은 리사이클러뷰 안의 행으로써 표시됨.
    public class ReceivedMessageHolder : RecyclerView.ViewHolder
    {
        public ImageButton mProfileImage { get; private set; }
        public TextView mMsg { get; private set; }
        public TextView mTime { get; private set; }

        // 카드뷰 레이아웃(message_view) 내 객체들 참조.
        public ReceivedMessageHolder(View iItemView, System.Action<int> iListener) : base(iItemView)
        {
            // Locate and cache view references:
            mProfileImage = iItemView.FindViewById<ImageButton>(Resource.Id.mfr_profileIB);
            mMsg = iItemView.FindViewById<TextView>(Resource.Id.mfr_msgTV);
            mTime = iItemView.FindViewById<TextView>(Resource.Id.mfr_timeTV);

            iItemView.Click += (sender, e) =>
            {
                iListener(base.LayoutPosition);
            };
        }

        public void bind(List<HeaderModel> list, int iPosition)
        {
            ChildModel obj = list[iPosition] as ChildModel;
            TextMessage message = obj.TextMessage;

            //연락처에 있는 사람이면
            //if (list.Contact != null)
            //{
            //    //연락처에 사진이 있다면 사진으로 대체
            //    if (list.Contact.PhotoThumnail_uri != null)
            //        mProfileImage.SetImageURI(Android.Net.Uri.Parse(list.Contact.PhotoThumnail_uri));
            //    else
            //        mProfileImage.SetImageURI(Android.Net.Uri.Parse("@drawable/dd9_send_256"));
            //}
            //else
            //{
            //    //연락처에 사진이 없으면 기본사진으로 설정
            //    mProfileImage.SetImageURI(Android.Net.Uri.Parse("@drawable/dd9_send_256"));
            //}
            mMsg.Text = message.Msg;

            DateTimeUtillity dtu = new DateTimeUtillity();
            mTime.Text = dtu.milisecondToDateTimeStr(message.Time, "a hh:mm");
        }
    }

    //----------------------------------------------------------------------
    // SetnMessage VIEW HOLDER

    public class SentMessageHolder : RecyclerView.ViewHolder
    {
        public TextView mMsg { get; private set; }
        public TextView mTime { get; private set; }

        // 카드뷰 레이아웃(message_view) 내 객체들 참조.
        public SentMessageHolder(View iItemView, System.Action<int> iListener) : base(iItemView)
        {
            // Locate and cache view references:
            mMsg = iItemView.FindViewById<TextView>(Resource.Id.mfs_msgTV);
            mTime = iItemView.FindViewById<TextView>(Resource.Id.mfs_timeTV);

            // Detect user clicks on the item view and report which item
            // was clicked (by layout position) to the listener:
            iItemView.Click += (sender, e) =>
            {
                iListener(base.LayoutPosition);
            };
        }

        public void bind(List<HeaderModel> list, int iPosition)
        {
            ChildModel obj = list[iPosition] as ChildModel;
            TextMessage message = obj.TextMessage;

            mMsg.Text = message.Msg;

            DateTimeUtillity dtu = new DateTimeUtillity();
            mTime.Text = dtu.milisecondToDateTimeStr(message.Time, "a hh:mm");
        }
    }




    //----------------------------------------------------------------------
    // ADPATER

    public class HeaderModelAdpater : RecyclerView.Adapter
    {
        private const int VIEW_TYPE_HEADER = 0;
        private const int VIEW_TYPE_MESSAGE_RECEIVED = 1;
        private const int VIEW_TYPE_MESSAGE_SENT = 2;


        // Event handler for item clicks:
        public event System.EventHandler<int> mItemClick;

        // 현 페이지 대화 목록
        public List<HeaderModel> mHeaderModel;

        // Load the adapter with the data set (photo album) at construction time:
        public HeaderModelAdpater(List<HeaderModel> iHeaderModel)
        {
            mHeaderModel = iHeaderModel;
        }

        public override int GetItemViewType(int position)
        {
            if (mHeaderModel[position].isHeader())
            {
                return VIEW_TYPE_HEADER;
            }
            else
            {
                ChildModel ch = mHeaderModel[position] as ChildModel;
                TextMessage tm = ch.TextMessage;

                switch (Convert.ToInt32(tm.Type))
                {
                    case (int)TextMessage.MESSAGE_FOLDER.RECEIVED:
                        return VIEW_TYPE_MESSAGE_RECEIVED;
                    case (int)TextMessage.MESSAGE_FOLDER.SENT:
                        return VIEW_TYPE_MESSAGE_SENT;
                    default:
                        return -1;        //에러 체크해야됨
                }
            }

        }

        // 뷰 홀더 생성
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup iParent, int iViewType)
        {
            View itemView;

            if (iViewType == VIEW_TYPE_HEADER)
            {
                itemView = LayoutInflater.From(iParent.Context).Inflate(Resource.Layout.message_frag_header, iParent, false);
                return new HeaderHolder(itemView, OnClick);
            }
            else if (iViewType == VIEW_TYPE_MESSAGE_RECEIVED)
            {
                itemView = LayoutInflater.From(iParent.Context).Inflate(Resource.Layout.message_frag_received, iParent, false);
                return new ReceivedMessageHolder(itemView, OnClick);
            }
            else if (iViewType == VIEW_TYPE_MESSAGE_SENT)
            {
                itemView = LayoutInflater.From(iParent.Context).Inflate(Resource.Layout.message_frag_sent, iParent, false);
                return new SentMessageHolder(itemView, OnClick);
            }
            return null;
        }

        // 뷰 홀더에 데이터를 설정하는 부분
        public override void OnBindViewHolder(RecyclerView.ViewHolder iHolder, int iPosition)
        {

            switch (GetItemViewType(iPosition))
            {
                case VIEW_TYPE_HEADER:
                    HeaderHolder a = iHolder as HeaderHolder;
                    a.bind(mHeaderModel, iPosition);
                    break;
                case VIEW_TYPE_MESSAGE_RECEIVED:
                    ReceivedMessageHolder b = iHolder as ReceivedMessageHolder;
                    b.bind(mHeaderModel, iPosition);
                    break;
                case VIEW_TYPE_MESSAGE_SENT:
                    SentMessageHolder c = iHolder as SentMessageHolder;
                    c.bind(mHeaderModel, iPosition);
                    break;
            }


        }

        // Return the number of photos available in the photo album:
        public override int ItemCount
        {
            get { return mHeaderModel.Count; }
        }

        // 메세지를 클릭했을 때 발생하는 메소드
        void OnClick(int iPosition)
        {
            if (mItemClick != null)
                mItemClick(this, iPosition);
        }
    }
}