using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;

using LettreForAndroid.Class;
using LettreForAndroid.Utility;
using LettreForAndroid.Receivers;

using Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Provider;

namespace LettreForAndroid.UI
{
    [Activity(Label = "NewDialogueActivity", Theme = "@style/BasicTheme", NoHistory = true)]
    class NewDialogueActivity : AppCompatActivity
    {
        Button _SendButton;
        EditText _MsgBox;
        EditText _AddressBox;

        SmsSentReceiver _SmsSentReceiver;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.NewDialogueActivity);

            SetupToolbar();

            SetupContactLayout();

            SetupBottomLayout();

            //브로드캐스트 리시버 초기화
            _SmsSentReceiver = new SmsSentReceiver();
            _SmsSentReceiver.SentCompleteEvent += _SmsSentReceiver_SentCompleteEvent;
        }

        //---------------------------------------------------------------------
        //연락처 레이아웃 세팅

        //툴바 설정
        private void SetupToolbar()
        {
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.nda_toolbar);

            SetSupportActionBar(toolbar);

            SupportActionBar.Title = "새 대화";
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

        //---------------------------------------------------------------------
        //연락처 레이아웃 세팅

        private void SetupContactLayout()
        {
            ContactViewManager contactManager = new ContactViewManager();
            contactManager.SetContactViewLayout(this);
        }

        //-----------------------------------------------------------------
        //UI

        private void SetupBottomLayout()
        {
            _SendButton = FindViewById<Button>(Resource.Id.dbl_sendBtn);
            _MsgBox = FindViewById<EditText>(Resource.Id.dbl_msgBox);
            _AddressBox = FindViewById<EditText>(Resource.Id.cv_searchEditText);

            _SendButton.Click += _SendButton_Click;
        }

        private void _SendButton_Click(object sender, EventArgs e)
        {
            string msgBody = _MsgBox.Text;
            string address = _AddressBox.Text;

            if(address == string.Empty || msgBody == string.Empty)
            {
                return;
            }

            MessageSender.SendSms(this, address, msgBody);
        }

        //-----------------------------------------------------------------
        //Message Sent

        //문자 전송 이후 호출됨
        private void _SmsSentReceiver_SentCompleteEvent(int resultCode)
        {
            //문자 전송 성공
            if (resultCode.Equals((int)Result.Ok))
            {
                //DB에 삽입
                MessageDBManager.Get().InsertMessage(_AddressBox.Text, _MsgBox.Text, 1, (int)TextMessage.MESSAGE_TYPE.SENT);

                //DB 새로고침
                MessageDBManager.Get().Refresh();

                Context context = Android.App.Application.Context;

                long thread_id = MessageDBManager.Get().GetThreadId(_AddressBox.Text);

                Intent intent = new Intent(context, typeof(DialogueActivity));
                intent.PutExtra("thread_id", thread_id);
                intent.PutExtra("category", (int)Dialogue.LableType.UNKNOWN);

                Android.App.Application.Context.StartActivity(intent);
            }
            else
            {
                //문자 전송 실패시
                Toast.MakeText(this, "문자 전송에 실패하였습니다.", ToastLength.Long).Show();
                //throw new Exception("문자 전송 실패시 코드 짜라");
            }
        }

        //-----------------------------------------------------------------
        //Receiver

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
}