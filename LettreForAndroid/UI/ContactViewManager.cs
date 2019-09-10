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
        EditText _EditText;

        LinearLayoutManager _LayoutManager;
        RecyclerContactAdpater _Adapter;

        public void SetContactViewLayout(Activity activity)
        {
            _RecyclerView = activity.FindViewById<RecyclerView>(Resource.Id.cv_recyclerview);
            _GuideText = activity.FindViewById<TextView>(Resource.Id.cv_guideText);
            _EditText = activity.FindViewById<EditText>(Resource.Id.cv_searchEditText);

            _LayoutManager = new LinearLayoutManager(Application.Context);
            _Adapter = new RecyclerContactAdpater(ContactDBManager.Get().ContactList);

            _EditText.AfterTextChanged += (sender, e) =>
            {
                _Adapter.Filter(_EditText.Text);
            };

            Refresh();
        }


        public void Refresh()
        {
            Dictionary<long, Contact> _ContactList = ContactDBManager.Get().ContactList;

            //연락처가 있으면 리사이클러 뷰 내용안에 표시하도록 함
            if (_ContactList.Count > 0)
            {
                _Adapter.UpdateContactList(_ContactList);

                _RecyclerView.SetAdapter(_Adapter);
                _RecyclerView.SetLayoutManager(_LayoutManager);
                _RecyclerView.ScrollToPosition(0);

                _GuideText.Visibility = ViewStates.Gone;
            }
            else
            {
                //연락처가 없는 경우
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
        public Dictionary<long, Contact> _ContactList;
        private List<Contact> _FilteredItem;

        // Load the adapter with the data set (photo album) at construction time:
        public RecyclerContactAdpater(Dictionary<long, Contact> iContactList)
        {
            _ContactList = iContactList;
            _FilteredItem = new List<Contact>();
        }

        public void UpdateContactList(Dictionary<long, Contact> iContactList)
        {
            _ContactList = iContactList;
            _FilteredItem.Clear();
            _FilteredItem.AddRange(_ContactList.Values.ToList());
            SortItem();
        }

        public override int GetItemViewType(int position)
        {
            if (_FilteredItem[position].GetType().Name == "Contact")
            {
                return VIEW_TYPE_CONTACT;
            }
            else
            {
                return -1;
            }
        }

        public void Filter(string targetStr)
        {
            _FilteredItem.Clear();
            if (targetStr.Length == 0)
            {
                _FilteredItem.AddRange(_ContactList.Values.ToList());
            }
            else
            {
                foreach(Contact objContact in _ContactList.Values.ToList())
                {
                    string address = objContact.PrimaryContactData.Address;
                    string name = objContact.PrimaryContactData.Name.ToLower();
                    if (address.Contains(targetStr) || name.Contains(targetStr.ToLower()))
                    {
                        _FilteredItem.Add(objContact);
                    }
                }
            }
            SortItem();
            NotifyDataSetChanged();
        }

        private void SortItem()
        {
            _FilteredItem.Sort(delegate (Contact A, Contact B)
            {
                if (string.Compare(A.PrimaryContactData.Name, B.PrimaryContactData.Name) > 0) return 1;
                else if (string.Compare(A.PrimaryContactData.Name, B.PrimaryContactData.Name) < 0) return -1;
                return 0;
            });
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
                ch.name.Text = _FilteredItem[iPosition].PrimaryContactData.Name;
                if (_FilteredItem[iPosition].PrimaryContactData.PhotoThumnail_uri != null)
                    ch.photoThumnail.SetImageURI(Android.Net.Uri.Parse(_FilteredItem[iPosition].PrimaryContactData.PhotoThumnail_uri));
                else
                    ch.photoThumnail.SetImageResource(Resource.Drawable.profile_icon_256_background);
            }
        }

        // Return the number of photos available in the photo album:
        public override int ItemCount
        {
            get { return _FilteredItem.Count; }
        }

        // 연락처를 클릭했을 때 대화액티비티를 보여준다.
        void OnClick(int iPosition)
        {
            //해당 연락처와의 대화가 있었는지 탐색
            ContactData objContact = _FilteredItem[iPosition].PrimaryContactData;
            long thread_id = MessageDBManager.Get().GetThreadId(objContact.Address);
            Dialogue objDialogue = MessageDBManager.Get().FindDialogue(thread_id);

            //연락처와의 대화 페이지를 보여준다.
            Context context = MainActivity._Instance;
            Intent intent = new Intent(context, typeof(DialogueActivity));

            //기존 대화가 존재하면 기존 대화를 보여준다.
            if (objDialogue != null)
            {
                intent.PutExtra("thread_id", objDialogue.Thread_id);
            }
            intent.PutExtra("category", (int)Dialogue.LableType.COMMON);        //연락처로부터 나온 대화이므로, 레이블은 일반 대화임.
            intent.PutExtra("address", objContact.Address);

            context.StartActivity(intent);
        }

    }
}