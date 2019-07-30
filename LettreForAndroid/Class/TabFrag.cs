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
        private string tabTitle;    //탭에 표시될 제목
        private int category;       //탭의 코드, 전체 0, 대화 1, 택배 2, 카드 3, 인증 4, 공공기관 5, 통신사 6, SPAM 7
        private int position;       //위치
        private int notiCount;      //알림 카운트

        public TabFrag(string iTabTitle, int iCategory, int iPosition, int iNotiCount)
        {
            tabTitle = iTabTitle;
            category = iCategory;
            position = iPosition;
            notiCount = iNotiCount;
        }
        public string TabTitle
        {
            get { return tabTitle; }
            set { tabTitle = value; }
        }
        public int Category
        {
            get { return category; }
            set { category = value; }
        }
        public int Position
        {
            get { return position; }
            set { position = value; }
        }
        public int NotiCount
        {
            get { return notiCount; }
            set { notiCount = value; }
        }
    }
}