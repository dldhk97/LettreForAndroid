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
        readonly Activity mActivity;
        AppCompatActivity mAppCompatActivity;
        public ToolbarManager(Activity iActivity, AppCompatActivity iAppcompatActivity)
        {
            mActivity = iActivity;
            mAppCompatActivity = iAppcompatActivity;
        }
        //툴바 적용
        public void SetupToolBar()
        {
            var toolbar = mActivity.FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.my_toolbar);

            mAppCompatActivity.SetSupportActionBar(toolbar);
            mAppCompatActivity.SupportActionBar.Title = "Lettre";
        }
        //툴바에 메뉴 추가
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            mActivity.MenuInflater.Inflate(Resource.Menu.toolbar, menu);
            return mAppCompatActivity.OnCreateOptionsMenu(menu);
        }

        //툴바 선택시
        public override bool OnOptionsItemSelected(IMenuItem iItem)
        {
            //Toast.MakeText(this, "Top ActionBar pressed: " + item.TitleFormatted, ToastLength.Short).Show();
            if (iItem.ItemId == Resource.Id.toolbar_search)
            {
                string str = DataStorageManager.loadStringData(mActivity, "temp", "NULL");
                Toast.MakeText(mActivity, "Top ActionBar pressed: " + str, ToastLength.Short).Show();
            }
            else
            {
                DataStorageManager.saveStringData(mActivity, "temp", iItem.TitleFormatted.ToString());
            }
            return mAppCompatActivity.OnOptionsItemSelected(iItem);
        }
    }
}