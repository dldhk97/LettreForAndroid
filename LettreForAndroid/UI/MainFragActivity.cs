﻿using System;
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
    public class MainFragActivity : Fragment
    {
        public static MainFragActivity _Instance;

        const string INTENT_CATEGORY = "intentCategory";
        const string INTENT_POSITION = "intentPosition";
        private int _Category;
        private int _Position;

        DialogueSet _DialogueSet;

        TextView _GuideText;
        RecyclerView _RecyclerView;
        RecyclerView.LayoutManager _LayoutManager;

        //새로운 페이지가 만들어질때 호출됨
        public static MainFragActivity newInstance(int iPosition, int iCategory)  //어댑터로부터 현재 탭의 정보를 받음. 이것을 args에 저장함. Static이라서 args를 통해 OnCreate로 전달.
        {
            var args = new Bundle();
            args.PutInt(INTENT_CATEGORY, iCategory);
            args.PutInt(INTENT_POSITION, iPosition);

            var fragment = new MainFragActivity();
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

            RefreshRecyclerView();

            return view;
        }

        public void RefreshFrag()
        {
            if(FragmentManager != null)
            {
                FragmentTransaction a = FragmentManager.BeginTransaction();
                a.Detach(this);
                a.Attach(this);
                a.CommitAllowingStateLoss();
            }
        }

        public void RefreshRecyclerView()
        {
            //데이터 준비 : 현재 탭에 해당되는 대화목록을 가져온다.
            if (_Category == (int)Dialogue.LableType.ALL)
                _DialogueSet = MessageDBManager.Get().TotalDialogueSet;
            else if (_Category == (int)Dialogue.LableType.UNKNOWN)
                _DialogueSet = MessageDBManager.Get().UnknownDialogueSet;
            else
                _DialogueSet = MessageDBManager.Get().DialogueSets[_Category];

            //대화가 있으면 리사이클러 뷰 내용안에 표시하도록 함
            if (_DialogueSet.Count > 0)
            {
                DialogueSetAdpater adapter = new DialogueSetAdpater(_DialogueSet, _Category, _LayoutManager);
                _RecyclerView.SetAdapter(adapter);
            }
            else
            {
                //문자가 없으면 없다고 알려준다.
                _GuideText.Visibility = ViewStates.Visible;
                _RecyclerView.Visibility = ViewStates.Gone;

                if(MainActivity._Instance._MessageLoadedOnce)
                {
                    _GuideText.Text = "메시지가 존재하지 않습니다";
                }
                else
                {
                    _GuideText.Text = "메시지를 불러오는 중";
                }
            }
        }

        public override void OnResume()
        {
            base.OnResume();

			//대화액티비티가 끝나면 스크롤 위치 복원
			int position = TabFragManager._Instance.ScrollPosition[_Category];
            if (position > 0)
            {
                _RecyclerView.ScrollToPosition(position);
            }

        }

        public override void OnPause()
        {
            base.OnPause();

            //현재 스크롤 위치 기억
            LinearLayoutManager layoutManager = (LinearLayoutManager)_LayoutManager;
            TabFragManager._Instance.ScrollPosition[_DialogueSet.Lable] = layoutManager.FindFirstVisibleItemPosition();
        }

        //----------------------------------------------------------------------
        // DialogueFragAll VIEW HOLDER
        // 전체 탭의 프래그먼트 뷰홀더

        public class DialogueFragAllHolder : RecyclerView.ViewHolder
        {
            public TextView mCategoryText { get; private set; }
            public TextView mAddress { get; private set; }
            public TextView mMsg { get; private set; }
            public TextView mTime { get; private set; }
            public RelativeLayout mReadStateRL { get; private set; }
            public TextView mReadStateCnt { get; private set; }

            // 카드뷰 레이아웃(dialogue_frag) 내 객체들 참조.
            public DialogueFragAllHolder(View iItemView, System.Action<int, int> iListener) : base(iItemView)
            {
                mCategoryText = iItemView.FindViewById<TextView>(Resource.Id.dfa_categoryTV);
                mAddress = iItemView.FindViewById<TextView>(Resource.Id.dfa_addressTV);
                mMsg = iItemView.FindViewById<TextView>(Resource.Id.dfa_msgTV);
                mTime = iItemView.FindViewById<TextView>(Resource.Id.dfa_timeTV);
                mReadStateRL = iItemView.FindViewById<RelativeLayout>(Resource.Id.dfa_readstateRL);
                mReadStateCnt = iItemView.FindViewById<TextView>(Resource.Id.dfa_readstateCntTV);

                iItemView.Click += (sender, e) =>
                {
                    iListener(base.LayoutPosition, (int)DialogueSetAdpater.CLICK_TYPE.CLICK);
                };

                iItemView.LongClick += (sender, e) =>
                {
                    iListener(base.LayoutPosition, (int)DialogueSetAdpater.CLICK_TYPE.LONG_CLICK);
                };
            }

            public void Bind(Dialogue dialogue)
            {
                mCategoryText.Text = Dialogue._LableTypeStr[dialogue.MajorLable];   //카테고리 설정
                BindHolder(dialogue, mAddress, mMsg, mTime, mReadStateRL, mReadStateCnt);
            }
        }


        public static void BindHolder(Dialogue dialogue, TextView address, TextView msgText, TextView timeText, RelativeLayout readStateRL, TextView readStateCnt)
        {
            TextMessage lastMessage = dialogue[0];                           //대화 중 가장 마지막 문자

            //이름 혹은 연락처 표시, 문자 내용 표시
            address.Text = dialogue.DisplayName;
            if (lastMessage.GetType() == typeof(MultiMediaMessage))
            {
                MultiMediaMessage objMMS = lastMessage as MultiMediaMessage;
                switch (objMMS.MediaType)
                {
                    case (int)MultiMediaMessage.MEDIA_TYPE.TEXT:
                        msgText.Text = objMMS.Msg;
                        break;
                    case (int)MultiMediaMessage.MEDIA_TYPE.IMAGE:
                        msgText.Text = objMMS.Msg != null ? objMMS.Msg : "이미지 MMS";
                        break;
                    case (int)MultiMediaMessage.MEDIA_TYPE.VCF:
                        msgText.Text = objMMS.Msg != null ? objMMS.Msg : "VCF MMS";
                        break;
                }
            }
            else
            {
                msgText.Text = lastMessage.Msg;
            }

            //날짜 표시
            DateTimeUtillity dtu = new DateTimeUtillity();

            if (dtu.GetNow().Year <= dtu.GetYear(lastMessage.Time))                                  //올해 메시지이면
            {
                if (dtu.GetDatetime(lastMessage.Time) >= dtu.GetToday())
                    timeText.Text = dtu.MilisecondToDateTimeStr(lastMessage.Time, "a hh:mm");          //오늘 메시지이면
                else
                    timeText.Text = dtu.MilisecondToDateTimeStr(lastMessage.Time, "MM월 dd일");        //올해인데 오늘 메시지가 아님
            }
            else
            {
                timeText.Text = dtu.MilisecondToDateTimeStr(lastMessage.Time, "yyyy년 MM월 dd일");    //올해 메시지가 아님
            }

            //문자 읽음 여부에 따른 상태표시기 표시여부 및 카운트설정
            if (dialogue.UnreadCnt > 0)
            {
                readStateRL.Visibility = ViewStates.Visible;
                readStateCnt.Text = dialogue.UnreadCnt.ToString();
            }
            else
            {
                readStateRL.Visibility = ViewStates.Invisible;
            }
        }

        //UI 새로고침. 열려있는 대화창과, 탭, 메인프래그를 새로고침.
        public static void RefreshUI()
        {
            //대화창 존재하면 새로고침
            if (DialogueActivity._Instance != null)
                DialogueActivity._Instance.RefreshRecyclerView();

            //탭, 메인 새로고침
            if(TabFragManager._Instance != null)
                TabFragManager._Instance.RefreshLayout();
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
            public DialogueFragCategoryHolder(View iItemView, System.Action<int, int> iListener) : base(iItemView)
            {
                mProfileImage = iItemView.FindViewById<ImageButton>(Resource.Id.dfc_profileIB);
                mAddress = iItemView.FindViewById<TextView>(Resource.Id.dfc_addressTV);
                mMsg = iItemView.FindViewById<TextView>(Resource.Id.dfc_msgTV);
                mTime = iItemView.FindViewById<TextView>(Resource.Id.dfc_timeTV);
                mReadStateRL = iItemView.FindViewById<RelativeLayout>(Resource.Id.dfc_readstateRL);
                mReadStateCnt = iItemView.FindViewById<TextView>(Resource.Id.dfc_readstateCntTV);

                iItemView.Click += (sender, e) =>
                {
                    iListener(base.LayoutPosition, (int)DialogueSetAdpater.CLICK_TYPE.CLICK);
                };

                iItemView.LongClick += (sender, e) =>
                {
                    iListener(base.LayoutPosition, (int)DialogueSetAdpater.CLICK_TYPE.LONG_CLICK);
                };
            }

            public void Bind(Dialogue dialogue)
            {
                //전체 탭이 아닌 경우
                //연락처에 있는 사람이면
                if (dialogue.Contact != null)
                {
                    ////연락처에 사진이 있다면 사진으로 대체
                    if (dialogue.Contact.PhotoThumnail_uri != null)
                        mProfileImage.SetImageURI(Android.Net.Uri.Parse(dialogue.Contact.PhotoThumnail_uri));
                    else
                        mProfileImage.SetImageResource(Resource.Drawable.profile_icon_256_background);
                }
                else
                {
                    //연락처에 사진이 없으면 기본사진으로 설정
                    mProfileImage.SetImageResource(Resource.Drawable.profile_icon_256_background);
                }

                BindHolder(dialogue, mAddress, mMsg, mTime, mReadStateRL, mReadStateCnt);
            }
        }

        public class DialogueSetAdpater : RecyclerView.Adapter
        {
            public enum CLICK_TYPE { CLICK = 1, LONG_CLICK };

            private const int VIEW_TYPE_ALL = 1;
            private const int VIEW_TYPE_CATEGORY = 2;

            // 현 페이지 대화 목록
            public DialogueSet _DialogueSet;
            private RecyclerView.LayoutManager _LayoutManager;
            int _Category;

            // Load the adapter with the data set (photo album) at construction time:
            public DialogueSetAdpater(DialogueSet iDialogueSet, int iCategory, RecyclerView.LayoutManager layoutManager)
            {
                _DialogueSet = iDialogueSet;
                _Category = iCategory;
                _LayoutManager = layoutManager;
            }

            public void UpdateDialogueSet(DialogueSet iDialogueSet)
            {
                _DialogueSet = iDialogueSet;
            }

            public override int GetItemViewType(int iPosition)
            {
                if (_Category == (int)Dialogue.LableType.ALL)
                {
                    return VIEW_TYPE_ALL;
                }
                else if ( 0 <= _Category && _Category < Dialogue.Lable_COUNT)
                {
                    return VIEW_TYPE_CATEGORY;
                }
                else if (_Category == (int)Dialogue.LableType.UNKNOWN)
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
                        a.Bind(_DialogueSet[iPosition]);
                        break;
                    case VIEW_TYPE_CATEGORY:
                        DialogueFragCategoryHolder b = iHolder as DialogueFragCategoryHolder;
                        b.Bind(_DialogueSet[iPosition]);
                        break;
                }
            }

            // Return the number of photos available in the photo album:
            public override int ItemCount
            {
                get { return _DialogueSet.Count; }
            }

            // 대화를 클릭했을 때 발생하는 메소드
            void OnClick(int iPosition, int type)
            {
                if(type == (int)CLICK_TYPE.CLICK)
                {
					//일반 터치 시 대화 액티비티로 이동
					Context context = MainActivity._Instance;

                    Intent intent = new Intent(context, typeof(DialogueActivity));
                    intent.PutExtra("address", _DialogueSet[iPosition].Address);

					context.StartActivity(intent);

                    //현재 스크롤 위치 기억
                    LinearLayoutManager layoutManager = (LinearLayoutManager)_LayoutManager;
                    TabFragManager._Instance.ScrollPosition[_DialogueSet.Lable] = layoutManager.FindFirstVisibleItemPosition();
                }
                else if(type == (int)CLICK_TYPE.LONG_CLICK)
                {
                    string majorLableStr = "대표 레이블 = " + _DialogueSet[iPosition].MajorLable + "\n";
                    string lableStr = "레이블 = ";
                    foreach(int lableElem in _DialogueSet[iPosition].Lables)
                    {
                        lableStr += lableElem + " ";
                    }
                    string msgCnt = "\n문자 수 = " + _DialogueSet[iPosition].Count.ToString();
                    Toast.MakeText(Android.App.Application.Context, majorLableStr + lableStr + msgCnt, ToastLength.Short).Show();
                    //롱 터치 시 옵션 제공 (현재는 디버깅용으로 레이블 표시)

                }
            }
        }
    }
}