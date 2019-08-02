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
using LettreForAndroid.Class;
using LettreForAndroid.Utility;

namespace LettreForAndroid.UI
{
    [Activity(Label = "WelcomeActivity", Theme = "@style/Theme.AppCompat.Light.NoActionBar")]
    class WelcomeActivity : AppCompatActivity
    {
        Button _CreateDBButton;
        Button _ExitButton;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.WelcomeActivity);

            _CreateDBButton = FindViewById<Button>(Resource.Id.wa_button1);
            _ExitButton = FindViewById<Button>(Resource.Id.wa_button2);

            _CreateDBButton.Click += async (sender, e) =>
            {
                CreateButtonClickAction();
            };

            _ExitButton.Click += async (sender, e) =>
            {
                Finish();
            };
        }

        private void CreateButtonClickAction()
        {
            if (CreateLableDB())
            {
                Toast.MakeText(this, "레이블 DB가 생성되었습니다.", ToastLength.Long).Show();
                Finish();
            }
            else
            {
                Android.App.AlertDialog.Builder builder = new Android.App.AlertDialog.Builder(this);
                builder.SetTitle("레이블 DB 생성에 실패했습니다.");
                builder.SetMessage("다시 시도하시겠습니까?");
                builder.SetPositiveButton("예", (senderAlert, args) =>
                {
                    CreateButtonClickAction();
                });
                builder.SetNegativeButton("아니오", (senderAlert, args) =>
                {
                });
                Dialog dialog = builder.Create();
                dialog.Show();
            }
        }

        private bool CreateLableDB()
        {
            //미분류 메시지가 하나도 없는 경우
            if (MessageDBManager.Get().DialogueSets[(int)Dialogue.LableType.UNKNOWN].Count <= 0)
                return true;

            //서버와 통신해서 Lable DB 생성 후 메모리에 올림.
            LableDBManager.Get().CreateLableDB(
            MessageDBManager.Get().DialogueSets[(int)Dialogue.LableType.UNKNOWN]);

            if (LableDBManager.Get().IsDBExist())
            {
                MessageDBManager.Get().CategorizeLocally(
                    MessageDBManager.Get().DialogueSets[(int)Dialogue.LableType.UNKNOWN]);
                return true;
            }
            else
            {
                Toast.MakeText(this, "레이블 DB 생성에 실패했습니다.", ToastLength.Long).Show();
                return false;
            }
        }
    }
}