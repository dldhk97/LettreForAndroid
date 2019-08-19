using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Telephony;
using Android.Views;
using Android.Widget;

using LettreForAndroid.Class;
using LettreForAndroid.Receivers;
using LettreForAndroid.Utility;

using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace LettreForAndroid.UI
{
    [Activity(Label = "DialogueActivity", Theme = "@style/FadeInOutActivity")]
    [IntentFilter(new[] { "android.intent.action.SEND", "android.intent.action.SENDTO" }, Categories = new[] { "android.intent.category.DEFAULT", "android.intent.category.BROWSABLE" }, DataSchemes = new[] { "sms", "smsto", "mms", "mmsto" })]
    public class DialogueActivity : AppCompatActivity
    {
        public static DialogueActivity _Instance;

        string _CurAddress;
        long _CurThread_id;
        Dialogue _CurDialogue;

        List<RecyclerItem> _RecyclerItems;

        RecyclerView _RecyclerView;
        Button _SendButton;
        EditText _MsgBox;

        SmsSentReceiver _SmsSentReceiver;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.DialogueActivity);

            _Instance = this;

            _CurAddress = Intent.GetStringExtra("address");                             //액티비티 인자로 전화번호를 받는다.
            _CurThread_id = MessageDBManager.Get().GetThreadId(_CurAddress);           //해당 전화번호로 등록된 thread_id를 찾는다.

            _CurDialogue = MessageDBManager.Get().LoadDialogue(_CurThread_id, false);                  //thread_id로 기존에 대화가 존재하는지 찾는다.
            if (_CurDialogue == null)                                                   //대화가 없으면 새로 만든다.
            {
                _CurDialogue = CreateNewDialogue(_CurAddress);
            }

            SetupLayout();

            SetupToolbar();

            _SmsSentReceiver = new SmsSentReceiver();                                   //브로드캐스트 리시버 초기화
            _SmsSentReceiver.SentCompleteEvent += _SmsSentReceiver_SentCompleteEvent;
        }

        private Dialogue CreateNewDialogue(string address)
        {
            Dialogue dialogue = new Dialogue();
            dialogue.Address = address;
            dialogue.Contact = ContactDBManager.Get().getContactDataByAddress(address);

            if(dialogue.Contact != null)
                dialogue.DisplayName = dialogue.Contact.Name;
            else
                dialogue.DisplayName = address;

            dialogue.MajorLable = (int)Dialogue.LableType.COMMON;
            dialogue.UnreadCnt = 0;
            dialogue.Thread_id = MessageDBManager.Get().GetThreadId(address);

            return dialogue;
        }

        //-----------------------------------------------------------------
        //툴바 설정
        private void SetupToolbar()
        {
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.da_toolbar);

            SetSupportActionBar(toolbar);

            SupportActionBar.Title = _CurDialogue.DisplayName;
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
        }
        //툴바 버튼 클릭 시
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            //돌아가기 클릭 시
            if (item.ItemId == Android.Resource.Id.Home)
            {
                Finish();
                OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_fade_out);     //창 닫을때 페이드효과
                //OverridePendingTransition(0, 0);     //창 닫을때 효과 없음
            }
            return base.OnOptionsItemSelected(item);
        }

        //툴바에 메뉴 추가
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.toolbar_dialogue, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        //-----------------------------------------------------------------
        //UI
        private void SetupLayout()
        {
            _SendButton = FindViewById<Button>(Resource.Id.dbl_sendBtn);
            _MsgBox = FindViewById<EditText>(Resource.Id.dbl_msgBox);

            _RecyclerView = FindViewById<RecyclerView>(Resource.Id.da_recyclerView1);

            _SendButton.Click += _SendButton_Click;

            RefreshRecyclerView();

            //탭 새로고침
            TabFragManager._Instance.RefreshTabLayout();
        }

        public void RefreshRecyclerView()
        {
            Dialogue updatedDialogue = MessageDBManager.Get().FindDialogue(_CurThread_id);      //기존에 대화가 있었다면 업데이트, 없으면 새로만들어진 경우이므로 유지
            if (updatedDialogue != null)
                _CurDialogue = updatedDialogue;

            //대화를 모두 읽음으로 처리
            _CurDialogue.UnreadCnt = 0;
            foreach (TextMessage msg in _CurDialogue.TextMessageList)
            {
                if (msg.ReadState == (int)TextMessage.MESSAGE_READSTATE.UNREAD)
                {
                    msg.ReadState = (int)TextMessage.MESSAGE_READSTATE.READ;
                    MessageDBManager.Get().ChangeReadState(msg, (int)TextMessage.MESSAGE_READSTATE.READ);
                }
            }

            //대화를 리사이클러 뷰에 넣게 알맞은 형태로 변환. 헤더도 이때 포함시킨다.
            _RecyclerItems = groupByDate(_CurDialogue);

            //리사이클러 뷰 내용안에 표시함
            LinearLayoutManager layoutManager = new LinearLayoutManager(Application.Context);
            layoutManager.ReverseLayout = true;
            layoutManager.StackFromEnd = true;

            RecyclerItemAdpater Adapter = new RecyclerItemAdpater(_RecyclerItems, _CurDialogue.Contact);
            _RecyclerView.SetAdapter(Adapter);
            _RecyclerView.SetLayoutManager(layoutManager);
            _RecyclerView.ScrollToPosition(0);
        }

        //전송 버튼 클릭
        private void _SendButton_Click(object sender, EventArgs e)
        {
            string msgBody = _MsgBox.Text;

            if (msgBody != string.Empty)
            {
                MessageSender.SendSms(this, _CurDialogue.Address, msgBody);
            }
                
        }

        //-----------------------------------------------------------------
        //리퀘스트
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            switch (requestCode)
            {
                case (int)PermissionManager.REQUESTS.SENDSMS:
                    if (PermissionManager.HasPermission(this, PermissionManager.sendSMSPermission))
                    {
                        //권한 취득했으면 다시 전송요청
                        MessageSender.SendSms(this, _CurDialogue.Address, _MsgBox.Text);
                    }
                    else
                    {
                        Toast.MakeText(this, "권한이 없어 메세지 발송에 실패했습니다.", ToastLength.Short).Show();
                    }
                    break;
            }
        }

        //-----------------------------------------------------------------
        //대화(메세지 목록)을 리사이클러뷰에 넣기 알맞은 형태로 변환하고, 날짜별로 그룹화 및 헤더추가함.
        public List<RecyclerItem> groupByDate(Dialogue iDialogue)
        {
            string prevTime = "NULL";
            List<RecyclerItem> recyclerItems = new List<RecyclerItem>();
            if (iDialogue.Count <= 0)
                return recyclerItems;

            for (int i = 0; i < iDialogue.Count; i++)
            {
                DateTimeUtillity dtu = new DateTimeUtillity();
                string objTime = dtu.MilisecondToDateTimeStr(iDialogue[i].Time, "yyyy년 MM월 dd일 E요일");

                if (prevTime != objTime)
                {
                    if (prevTime != "NULL")
                    {
                        recyclerItems.Add(new HeaderItem(prevTime));
                    }
                    prevTime = objTime;
                }
                recyclerItems.Add(new MessageItem(iDialogue[i]));
            }
            recyclerItems.Add(new HeaderItem(prevTime));

            return recyclerItems;
        }

        //-----------------------------------------------------------------
        //리시버

        //문자 전송 이후 호출됨
        private void _SmsSentReceiver_SentCompleteEvent(int resultCode)
        {
            //문자 전송 성공
            if (resultCode.Equals((int)Result.Ok))
            {
                //DB에 삽입
                MessageDBManager.Get().InsertMessage(_CurDialogue.Address, _MsgBox.Text, 1, (int)TextMessage.MESSAGE_TYPE.SENT);

                //입력칸 비우기
                _MsgBox.Text = string.Empty;

                //DB 새로고침
                MessageDBManager.Get().LoadDialogue(_CurThread_id, true);

                //UI 업데이트
                if (_Instance != null)
                    _Instance.RefreshRecyclerView();

                for (int i = 0; i < CustomPagerAdapter._Pages.Count; i++)
                {
                    CustomPagerAdapter._Pages[i].refreshRecyclerView();
                }
            }
            else
            {
                //문자 전송 실패시
                Toast.MakeText(this, "문자 전송에 실패하였습니다.", ToastLength.Long).Show();
                //throw new Exception("문자 전송 실패시 코드 짜라");
            }
        }

        //Resume됬을때는 리시버 다시 등록
        protected override void OnResume()
        {
            base.OnResume();

            IntentFilter ifr = new IntentFilter(SmsSentReceiver.FILTER_SENT);
            RegisterReceiver(_SmsSentReceiver, ifr);
        }

        //멈추면 리시버 해제
        protected override void OnPause()
        {
            base.OnPause();
            UnregisterReceiver(_SmsSentReceiver);
        }

    }




    //----------------------------------------------------------------------
    //----------------------------------------------------------------------
    // HEADER VIEW HOLDER

    public class HeaderHolder : RecyclerView.ViewHolder
    {
        public TextView mTime { get; private set; }

        // 카드뷰 레이아웃(message_view) 내 객체들 참조.
        public HeaderHolder(View iItemView, System.Action<int> iListener) : base(iItemView)
        {
            mTime = iItemView.FindViewById<TextView>(Resource.Id.mfh_timeTV);
        }

        public void bind(List<RecyclerItem> list, int iPosition)
        {
            HeaderItem headerObj = list[iPosition] as HeaderItem;

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

            iItemView.LongClick += (sendet, e) =>
             {
                 //iListener(base.LayoutPosition);
                 Android.Widget.PopupMenu menu = new Android.Widget.PopupMenu(Application.Context, iItemView);
                 menu.MenuInflater.Inflate(Resource.Menu.toolbar_dialogue, menu.Menu);
                 menu.Show();
             };
        }

        public void bind(List<RecyclerItem> list, int iPosition, ContactData iContact)
        {
            MessageItem obj = list[iPosition] as MessageItem;
            TextMessage message = obj.TextMessage;

            //연락처에 있는 사람이면
            if (iContact != null)
            {
                ////연락처에 사진이 있다면 사진으로 대체
                if (iContact.PhotoThumnail_uri != null)
                    mProfileImage.SetImageURI(Android.Net.Uri.Parse(iContact.PhotoThumnail_uri));
                else
                    mProfileImage.SetImageResource(Resource.Drawable.profile_icon_256_background);
            }
            else
            {
                //연락처에 사진이 없으면 기본사진으로 설정
                mProfileImage.SetImageResource(Resource.Drawable.profile_icon_256_background);
            }
            mMsg.Text = message.Msg;

            DateTimeUtillity dtu = new DateTimeUtillity();
            mTime.Text = dtu.MilisecondToDateTimeStr(message.Time, "a hh:mm");
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
            iItemView.LongClick += (sender, e) =>
            {
                //iListener(base.LayoutPosition);
                Android.Widget.PopupMenu menu = new Android.Widget.PopupMenu(Application.Context, iItemView);
                menu.MenuInflater.Inflate(Resource.Menu.toolbar_dialogue, menu.Menu);
                menu.Show();
            };
        }

        public void bind(List<RecyclerItem> list, int iPosition)
        {
            MessageItem obj = list[iPosition] as MessageItem;
            TextMessage message = obj.TextMessage;

            mMsg.Text = message.Msg;

            DateTimeUtillity dtu = new DateTimeUtillity();
            mTime.Text = dtu.MilisecondToDateTimeStr(message.Time, "a hh:mm");
        }
    }




    //----------------------------------------------------------------------
    // ADPATER

    public class RecyclerItemAdpater : RecyclerView.Adapter
    {
        private const int VIEW_TYPE_HEADER = 2;
        private const int VIEW_TYPE_MESSAGE_RECEIVED = 0;
        private const int VIEW_TYPE_MESSAGE_SENT = 1;
        
        // 현 페이지 대화 목록
        public List<RecyclerItem> mRecyclerItem;

        ContactData mContact;

        // Load the adapter with the data set (photo album) at construction time:
        public RecyclerItemAdpater(List<RecyclerItem> iRecyclerItem, ContactData iContact)
        {
            mRecyclerItem = iRecyclerItem;
            mContact = iContact;
        }

        public override int GetItemViewType(int position)
        {
            if (mRecyclerItem[position].Type == (int)RecyclerItem.TYPE.HEADER)
            {
                return VIEW_TYPE_HEADER;
            }
            else if (mRecyclerItem[position].Type == (int)RecyclerItem.TYPE.MESSAGE)
            {
                MessageItem ch = mRecyclerItem[position] as MessageItem;
                TextMessage objSms = ch.TextMessage;

                switch (objSms.Type)
                {
                    case (int)TextMessage.MESSAGE_TYPE.RECEIVED:
                        return VIEW_TYPE_MESSAGE_RECEIVED;
                    case (int)TextMessage.MESSAGE_TYPE.SENT:
                        return VIEW_TYPE_MESSAGE_SENT;
                    default:
                        //DEBUG!
                        Toast.MakeText(Application.Context, "DB에 타입이" + objSms.Type.ToString() + "인 메세지가 존재합니다.", ToastLength.Short);
                        return VIEW_TYPE_MESSAGE_RECEIVED;
                }
            }
            else
            {
                return -1;
            }

        }

        // 뷰 홀더 생성
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup iParent, int iViewType)
        {
            View itemView;

            if (iViewType == VIEW_TYPE_HEADER)
            {
                itemView = LayoutInflater.From(iParent.Context).Inflate(Resource.Layout.message_frag_header, iParent, false);
                return new HeaderHolder(itemView, null);
            }
            else if (iViewType == VIEW_TYPE_MESSAGE_RECEIVED)
            {
                itemView = LayoutInflater.From(iParent.Context).Inflate(Resource.Layout.message_frag_received, iParent, false);
                return new ReceivedMessageHolder(itemView, OnMsgBoxLongClick);
            }
            else if (iViewType == VIEW_TYPE_MESSAGE_SENT)
            {
                itemView = LayoutInflater.From(iParent.Context).Inflate(Resource.Layout.message_frag_sent, iParent, false);
                return new SentMessageHolder(itemView, OnMsgBoxLongClick);
            }
            throw new InvalidProgramException("존재하지 않는 뷰홀더 타입입니다!");
        }

        // 뷰 홀더에 데이터를 설정하는 부분
        public override void OnBindViewHolder(RecyclerView.ViewHolder iHolder, int iPosition)
        {

            switch (GetItemViewType(iPosition))
            {
                case VIEW_TYPE_HEADER:
                    HeaderHolder a = iHolder as HeaderHolder;
                    a.bind(mRecyclerItem, iPosition);
                    break;
                case VIEW_TYPE_MESSAGE_RECEIVED:
                    ReceivedMessageHolder b = iHolder as ReceivedMessageHolder;
                    b.bind(mRecyclerItem, iPosition, mContact);
                    break;
                case VIEW_TYPE_MESSAGE_SENT:
                    SentMessageHolder c = iHolder as SentMessageHolder;
                    c.bind(mRecyclerItem, iPosition);
                    break;
            }


        }

        // Return the number of photos available in the photo album:
        public override int ItemCount
        {
            get { return mRecyclerItem.Count; }
        }

        // 메세지를 롱 클릭했을 때 발생하는 메소드
        void OnMsgBoxLongClick(int iPosition)
        {

        }

    }
}