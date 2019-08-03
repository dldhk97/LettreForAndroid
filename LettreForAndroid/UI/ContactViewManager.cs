using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

using LettreForAndroid.Utility;
using LettreForAndroid.Class;

namespace LettreForAndroid.UI
{
    class ContactViewManager
    {
        RecyclerView _RecyclerView;
        TextView _GuideText;
        public void SetContactViewLayout(Activity activity)
        {
            _RecyclerView = activity.FindViewById<RecyclerView>(Resource.Id.cv_recyclerview);
            _GuideText = activity.FindViewById<TextView>(Resource.Id.cv_guideText);

            Refresh();
        }

        public void Refresh()
        {
            List<Contact> _ContactList = ContactDBManager.Get().ContactList;

            //문자가 있으면 리사이클러 뷰 내용안에 표시하도록 함
            if (_ContactList.Count > 0)
            {
                LinearLayoutManager layoutManager = new LinearLayoutManager(Application.Context);

                RecyclerContactAdpater Adapter = new RecyclerContactAdpater(_ContactList);
                _RecyclerView.SetAdapter(Adapter);
                _RecyclerView.SetLayoutManager(layoutManager);
                _RecyclerView.ScrollToPosition(0);

                _GuideText.Visibility = ViewStates.Gone;
            }
            else
            {
                //문자가 없으면... 여긴 버그 영역임...
                //throw new Exception("어케들어왔노");

                _GuideText.Visibility = ViewStates.Visible;
            }
        }
    }

    

    //----------------------------------------------------------------------
    //----------------------------------------------------------------------
    // CONTACT VIEW HOLDER

    public class ContactHolder : RecyclerView.ViewHolder
    {
        public TextView name { get; private set; }
        public ImageButton photoThumnail { get; private set; }

        // 카드뷰 레이아웃(message_view) 내 객체들 참조.
        public ContactHolder(View iItemView, System.Action<int> iListener) : base(iItemView)
        {
            photoThumnail = iItemView.FindViewById<ImageButton>(Resource.Id.cf_profileIB);
            name = iItemView.FindViewById<TextView>(Resource.Id.cf_nameTV);

            iItemView.Click += (sender, e) =>
            {
                iListener(base.LayoutPosition);
            };
        }

    }

    //----------------------------------------------------------------------
    //----------------------------------------------------------------------
    // ADAPTER

    public class RecyclerContactAdpater : RecyclerView.Adapter
    {
        private const int VIEW_TYPE_CONTACT = 1;
        
        // 현 페이지 대화 목록
        public List<Contact> _ContactList;

        // Load the adapter with the data set (photo album) at construction time:
        public RecyclerContactAdpater(List<Contact> iContact)
        {
            _ContactList = iContact;
        }

        public override int GetItemViewType(int position)
        {
            if (_ContactList[position].GetType().Name == "Contact")
            {
                return VIEW_TYPE_CONTACT;
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

            if (iViewType == VIEW_TYPE_CONTACT)
            {
                itemView = LayoutInflater.From(iParent.Context).Inflate(Resource.Layout.contact_frag, iParent, false);
                return new ContactHolder(itemView, OnClick);
            }
            throw new InvalidProgramException("존재하지 않는 뷰홀더 타입입니다!");
        }

        // 뷰 홀더에 데이터를 설정하는 부분
        public override void OnBindViewHolder(RecyclerView.ViewHolder iHolder, int iPosition)
        {
            if(GetItemViewType(iPosition) == VIEW_TYPE_CONTACT)
            {
                ContactHolder ch = iHolder as ContactHolder;
                ch.name.Text = _ContactList[iPosition].Name;
                if (_ContactList[iPosition].PhotoThumnail_uri != null)
                    ch.photoThumnail.SetImageURI(Android.Net.Uri.Parse(_ContactList[iPosition].PhotoThumnail_uri));
                else
                    ch.photoThumnail.SetImageURI(Android.Net.Uri.Parse("@drawable/dd9_send_256"));
            }
        }

        // Return the number of photos available in the photo album:
        public override int ItemCount
        {
            get { return _ContactList.Count; }
        }

        // 연락처를 클릭했을 때 발생하는 메소드
        void OnClick(int iPosition)
        {
            //해당 연락처와의 대화가 있었는지 탐색
            Contact objContact = _ContactList[iPosition];
            Dialogue objDialogue = null;
            foreach(Dialogue dialogue in MessageDBManager.Get().DialogueSets[(int)Dialogue.LableType.ALL].DialogueList.Values)
            {
                if(dialogue.Contact != null)
                {
                    if (dialogue.Contact.Address == objContact.Address)
                    {
                        objDialogue = dialogue;
                        break;
                    }
                }
                
            }

            //연락처와의 대화 페이지를 보여준다.
            Context context = Application.Context;
            Intent intent = new Intent(context, typeof(DialogueActivity));

            //기존 대화가 존재하면 기존 대화를 보여준다.
            if (objDialogue != null)
            {
                intent.PutExtra("thread_id", objDialogue.Thread_id);
            }
            intent.PutExtra("category", (int)Dialogue.LableType.COMMON);        //연락처로부터 나온 대화이므로, 레이블은 일반 대화임.
            intent.PutExtra("address", objContact.Address);

            Application.Context.StartActivity(intent);
        }

    }
}