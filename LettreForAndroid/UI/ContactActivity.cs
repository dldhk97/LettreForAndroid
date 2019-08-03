using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace LettreForAndroid.UI
{
    [Activity(Label = "ContactActivity", Theme = "@style/NulltActivity")]
    class ContactActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.ContactActivity);

            SetupToolbar();

            SetupBottomBar();
        }

        //-----------------------------------------------------------------
        //툴바 설정
        private void SetupToolbar()
        {
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.ca_toolbar);

            SetSupportActionBar(toolbar);

            SupportActionBar.Title = "연락처";

        }
        //툴바 버튼 클릭 시
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            //돌아가기 클릭 시
            if (item.ItemId == Android.Resource.Id.Home)
            {
                BackToMain();
            }
            return base.OnOptionsItemSelected(item);
        }

        //툴바에 메뉴 추가
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.toolbar_dialogue, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        //---------------------------------------------------------------------
        //하단 버튼 세팅

        private void SetupBottomBar()
        {
            var contactViewBtn = FindViewById<Button>(Resource.Id.ca_bottomBtn1);
            contactViewBtn.Click += (sender, o) =>
            {
                BackToMain();
            };
        }

        private void BackToMain()
        {
            Finish();
            //OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_fade_out);     //창 닫을때 페이드효과
            OverridePendingTransition(0, 0);     //창 닫을때 효과 없음
        }
    }
}