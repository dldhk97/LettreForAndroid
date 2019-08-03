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

        int _CurCategory;
        long _CurThread_id;
        Dialogue _CurDialogue;

        List<RecyclerItem> _RecyclerItems;

        RecyclerView _RecyclerView;
        Button _ma_sendButton;
        EditText _MsgBox;

        SmsManager _SmsManager;

        SmsSentReceiver _SmsSentReceiver;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.DialogueActivity);

            _Instance = this;

            //현페이지의 카테고리와 쓰레드ID 설정, 이것으로 어느 대화인지 특정할 수 있다.
            _CurCategory = Intent.GetIntExtra("category", -1);
            _CurThread_id = Intent.GetLongExtra("thread_id", -1);

            //대화 정보가 없음.
            if(_CurCategory == -1 && _CurThread_id == -1)
            {

            }

            SetupRecyclerView();

            SetupToolbar();

            _SmsManager = SmsManager.Default;

            //브로드캐스트 리시버 초기화
            _SmsSentReceiver = new SmsSentReceiver();
            _SmsSentReceiver.SentCompleteEvent += _SmsSentReceiver_SentCompleteEvent;
        }


        //문자 전송 이후 호출됨
        private void _SmsSentReceiver_SentCompleteEvent(int resultCode)
        {
            //문자 전송 성공
            if (resultCode.Equals((int)Result.Ok))
            {
                //문자를 DB에 저장
                ContentValues values = new ContentValues();
                values.Put(Telephony.TextBasedSmsColumns.Address, _CurDialogue.Address);
                values.Put(Telephony.TextBasedSmsColumns.Body, _MsgBox.Text);
                DateTimeUtillity dtu = new DateTimeUtillity();
                values.Put(Telephony.TextBasedSmsColumns.Date, dtu.getCurrentMilTime());
                values.Put(Telephony.TextBasedSmsColumns.Read, 1);
                values.Put(Telephony.TextBasedSmsColumns.Type, (int)TextMessage.MESSAGE_TYPE.SENT);
                values.Put(Telephony.TextBasedSmsColumns.ThreadId, _CurDialogue.Thread_id);
                ContentResolver.Insert(Telephony.Sms.Sent.ContentUri, values);

                //입력칸 비우기
                _MsgBox.Text = string.Empty;

                //DB 새로고침
                MessageDBManager.Get().Refresh();

                //UI 업데이트
                if (_Instance != null)
                    _Instance.RefreshRecyclerView();

                for (int i = 0; i < Customma_pagerAdapter._Pages.Count; i++)
                {
                    Customma_pagerAdapter._Pages[i].refreshRecyclerView();
                }
            }
            else
            {
                //문자 전송 실패시
                throw new Exception("문자 전송 실패시 코드 짜라");
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

        //-----------------------------------------------------------------
        //툴바 설정
        private void SetupToolbar()
        {
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.ma_toolbar);

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
        private void SetupRecyclerView()
        {
            _ma_sendButton = FindViewById<Button>(Resource.Id.da_sendBtn);
            _MsgBox = FindViewById<EditText>(Resource.Id.da_msgBox);

            _RecyclerView = FindViewById<RecyclerView>(Resource.Id.da_recyclerView1);

            _ma_sendButton.Click += ma_sendButton_Click;

            RefreshRecyclerView();

            //탭 새로고침
            TabFragManager._Instance.RefreshLayout();

            //대화목록(메인) 새로고침
            for (int i = 0; i < Customma_pagerAdapter._Pages.Count; i++)
            {
                Customma_pagerAdapter._Pages[i].refreshRecyclerView();
                if (DialogueActivity._Instance == null)
                    Customma_pagerAdapter._Pages[i].refreshFrag();
            }
        }

        public void RefreshRecyclerView()
        {
            _CurDialogue = MessageDBManager.Get().DialogueSets[_CurCategory][_CurThread_id];      //이 페이지에 해당되는 대화를 불러옴

            //대화를 모두 읽음으로 처리
            _CurDialogue.UnreadCnt = 0;
            foreach (TextMessage msg in _CurDialogue.TextMessageList)
            {
                if(msg.ReadState == (int)TextMessage.MESSAGE_READSTATE.UNREAD)
                {
                    msg.ReadState = (int)TextMessage.MESSAGE_READSTATE.READ;
                    MessageDBManager.Get().ChangeReadState(msg, (int)TextMessage.MESSAGE_READSTATE.READ);
                }
            }

            //대화를 리사이클러 뷰에 넣게 알맞은 형태로 변환. 헤더도 이때 포함시킨다.
            _RecyclerItems = groupByDate(_CurDialogue);

            //문자가 있으면 리사이클러 뷰 내용안에 표시하도록 함
            if (_RecyclerItems.Count > 0)
            {
                LinearLayoutManager layoutManager = new LinearLayoutManager(Application.Context);
                layoutManager.ReverseLayout = true;
                layoutManager.StackFromEnd = true;

                RecyclerItemAdpater Adapter = new RecyclerItemAdpater(_RecyclerItems, _CurDialogue.Contact);
                _RecyclerView.SetAdapter(Adapter);
                _RecyclerView.SetLayoutManager(layoutManager);
                _RecyclerView.ScrollToPosition(0);
            }
            else
            {
                //문자가 없으면... 여긴 버그 영역임...
                throw new Exception("어케들어왔노");
            }
        }

        //전송 버튼 클릭
        private void ma_sendButton_Click(object sender, EventArgs e)
        {
            string msgBody = _MsgBox.Text;

            if (msgBody != string.Empty)
                sendSms(_CurDialogue.Address, _MsgBox.Text);
        }

        public void sendSms(string address, string msg)
        {
            //권한 체크
            if (PermissionManager.HasPermission(Application.Context, PermissionManager.sendSMSPermission) == false)
            {
                Toast.MakeText(this, "메시지 발송을 위한 권한이 없습니다.", ToastLength.Long).Show();
                PermissionManager.RequestPermission(
                    this,
                    PermissionManager.sendSMSPermission,
                    "버튼을 눌러 권한을 승인해주세요.",
                    (int)PermissionManager.REQUESTS.SENDSMS
                    );
            }
            else
            {
                //권한이 있다면 바로 발송
                var piSent = PendingIntent.GetBroadcast(Application.Context, 0, new Intent(SmsSentReceiver.FILTER_SENT), 0);
                //var piDelivered = PendingIntent.GetBroadcast(Application.Context, 0, new Intent(SmsDeliverer.FILTER_DELIVERED), 0);

                _SmsManager.SendTextMessage(address, null, msg, piSent, null);
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
                        sendSms(_CurDialogue.Address, _MsgBox.Text);
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

            for (int i = 0; i < iDialogue.Count; i++)
            {
                DateTimeUtillity dtu = new DateTimeUtillity();
                string objTime = dtu.milisecondToDateTimeStr(iDialogue[i].Time, "yyyy년 MM월 dd일 E요일");

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

        public void bind(List<RecyclerItem> list, int iPosition, Contact iContact)
        {
            MessageItem obj = list[iPosition] as MessageItem;
            TextMessage message = obj.TextMessage;

            //연락처에 있는 사람이면
            if (iContact != null)
            {
                //연락처에 사진이 있다면 사진으로 대체
                if (iContact.PhotoThumnail_uri != null)
                    mProfileImage.SetImageURI(Android.Net.Uri.Parse(iContact.PhotoThumnail_uri));
                else
                    mProfileImage.SetImageURI(Android.Net.Uri.Parse("@drawable/dd9_send_256"));
            }
            else
            {
                //연락처에 사진이 없으면 기본사진으로 설정
                mProfileImage.SetImageURI(Android.Net.Uri.Parse("@drawable/dd9_send_256"));
            }
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
            mTime.Text = dtu.milisecondToDateTimeStr(message.Time, "a hh:mm");
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

        Contact mContact;

        // Load the adapter with the data set (photo album) at construction time:
        public RecyclerItemAdpater(List<RecyclerItem> iRecyclerItem, Contact iContact)
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