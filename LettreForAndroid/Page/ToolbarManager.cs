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

using LettreForAndroid.Utility;

namespace LettreForAndroid.Page
{
    class ToolbarManager : AppCompatActivity
    {
        Activity activity;
        AppCompatActivity appCompatActivity;
        public ToolbarManager(Activity iActivity, AppCompatActivity iAppcompatActivity)
        {
            activity = iActivity;
            appCompatActivity = iAppcompatActivity;
        }
        //툴바 적용
        public void SetupToolBar()
        {
            var toolbar = activity.FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.my_toolbar);

            appCompatActivity.SetSupportActionBar(toolbar);
            appCompatActivity.SupportActionBar.Title = "Lettre";
        }
        //툴바에 메뉴 추가
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            activity.MenuInflater.Inflate(Resource.Menu.toolbar, menu);
            return appCompatActivity.OnCreateOptionsMenu(menu);
        }

        //툴바 선택시
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            //Toast.MakeText(this, "Top ActionBar pressed: " + item.TitleFormatted, ToastLength.Short).Show();
            if (item.ItemId == Resource.Id.toolbar_search)
            {
                string str = DataStorageManager.loadStringData(activity, "temp", "NULL");
                Toast.MakeText(activity, "Top ActionBar pressed: " + str, ToastLength.Short).Show();
            }
            else
            {
                DataStorageManager.saveStringData(activity, "temp", item.TitleFormatted.ToString());
            }
            return appCompatActivity.OnOptionsItemSelected(item);
        }
    }
}