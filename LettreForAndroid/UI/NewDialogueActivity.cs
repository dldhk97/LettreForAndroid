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

using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace LettreForAndroid.UI
{
    [Activity(Label = "NewDialogueActivity", Theme = "@style/BasicTheme")]
    class NewDialogueActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.NewDialogueActivity);

            SetupToolbar();

            SetupContactLayout();
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
    }
}