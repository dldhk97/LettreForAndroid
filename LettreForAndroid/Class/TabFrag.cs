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

namespace LettreForAndroid.Class
{
    public class TabFrag
    {
        public enum CATEGORY { ALL = 0, COMMON, IDENTIFICATION, DELIVERY, PUBLIC, CARD, SPAM, UNKNOWN };

        private string mTabTitle;    //탭에 표시될 제목
        private int mCategory;       //탭의 고유 코드, 전체 0, 대화 1, 인증 2, 택배 3, 공공기관 4, 카드 5, 스팸 6, 미분류 7
        private int mPosition;       //위치
        private int mNotiCount;      //알림 카운트

        public TabFrag(string iTabTitle, int iCategory, int iPosition, int iNotiCount)
        {
            mTabTitle = iTabTitle;
            mCategory = iCategory;
            mPosition = iPosition;
            mNotiCount = iNotiCount;
        }
        public string TabTitle
        {
            get { return mTabTitle; }
            set { mTabTitle = value; }
        }
        public int Category
        {
            get { return mCategory; }
            set { mCategory = value; }
        }
        public int Position
        {
            get { return mPosition; }
            set { mPosition = value; }
        }
        public int NotiCount
        {
            get { return mNotiCount; }
            set { mNotiCount = value; }
        }
    }
}