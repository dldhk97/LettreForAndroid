﻿using Android.App;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.App;

using Android.Views;
using Android.Widget;

using Android.Content;
using Android.Runtime;
using System.Collections.Generic;
using System.Threading;

using LettreForAndroid.Class;
using LettreForAndroid.Utility;
using LettreForAndroid.UI;
using LettreForAndroid.Receivers;

using Toolbar = Android.Support.V7.Widget.Toolbar;


namespace LettreForAndroid
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Theme = "@style/BasicTheme")]
    public class MainActivity : AppCompatActivity
    {
        TabFragManager _TabFragManager;

        const int REQUEST_WELCOMECOMPLETE = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.MainActivity);

            string thisPackName = PackageName;
            string defulatPackName = Android.Provider.Telephony.Sms.GetDefaultSmsPackage(this);

            //기본앱이 아니면 Welcompage Activity 시작
            if (!thisPackName.Equals(defulatPackName))
            {
                StartActivityForResult(typeof(DefaultPackActivity), REQUEST_WELCOMECOMPLETE);
            }
            else
            {
                Setup();
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            switch (requestCode)
            {
                case REQUEST_WELCOMECOMPLETE:
                    Setup();
                    break;
            }

        }

        //웰컴페이지가 끝나거나, 처음사용자가 아닌경우 바로 이 메소드로 옮.
        public void Setup()
        {
            ContactDBManager.Get();                         //연락처를 모두 메모리에 올림
            LableDBManager.Get();                           //레이블 DB를 모두 메모리에 올림
            MessageDBManager.Get();                         //메시지를 모두 메모리에 올림

            //레이블 DB가 있나?
            if(LableDBManager.Get().IsDBExist())
            {
                MessageDBManager.Get().CategorizeNewMsg(); //메시지 중 레이블이 붙어있지 않은 대화가 있으면, 그 대화 다시 카테고라이즈함.
                MessageDBManager.Get().CategorizeLocally(
                    MessageDBManager.Get().DialogueSets[(int)Dialogue.LableType.UNKNOWN]);
            }
            else
            {
                //서버와 통신해서 Lable DB 생성 후 메모리에 올림.
                LableDBManager.Get().CreateLableDB(
                MessageDBManager.Get().DialogueSets[(int)Dialogue.LableType.UNKNOWN]);

                //만들어진 Lable DB로 카테고라이징
                if (LableDBManager.Get().IsDBExist())
                {
                    MessageDBManager.Get().CategorizeLocally(
                    MessageDBManager.Get().DialogueSets[(int)Dialogue.LableType.UNKNOWN]);
                }
                else
                {
                    Toast.MakeText(this, "레이블 DB 생성에 실패했습니다.", ToastLength.Long).Show();
                }
            }

            //ThreadPool.QueueUserWorkItem(o => MessageManager.Get().Initialization(this));     //스레드 풀 이용

            SetupLayout();
        }

        public void SetupLayout()
        {
            //툴바 세팅
            SetupToolBar();

            //하단 바 설정
            SetupBottomBar();

            //탭 레이아웃 세팅
            _TabFragManager = new TabFragManager(this, SupportFragmentManager);
            _TabFragManager.SetupTabLayout();
        }

        //-------------------------------------------------------------
        //툴바 세팅
        
         //툴바 적용
        public void SetupToolBar()
        {
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.my_toolbar);

            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "Lettre";
        }
        //툴바에 메뉴 추가
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.toolbar_main, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        //툴바 선택시
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if(item.ItemId == Resource.Id.toolbar_search)
            {

            }
            else
            {

            }
            return base.OnOptionsItemSelected(item);
        }

        //---------------------------------------------------------------------
        //하단 버튼 세팅

        public void SetupBottomBar()
        {
            var dialogueViewBtn = FindViewById<Button>(Resource.Id.ma_bottomBtn1);
            var contactViewBtn = FindViewById<Button>(Resource.Id.ma_bottomBtn2);
            //연락처 액티비티와 메인액티비티 간 전환 메소드 넣으면 됨.
        }
    }
}