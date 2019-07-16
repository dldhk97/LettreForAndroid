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
        private int code;           //탭의 고유 코드, 전체 0, 대화 1, 인증 2, 택배 3, 공공기관 4, 카드 5, 스팸 6, 미분류 7
        private string tabTitle;    //탭에 표시될 제목
        private int notiCount;      //알림 카운트

        public TabFrag(int iCode, string iTabTitle, int iNotiCount)
        {
            code = iCode;
            tabTitle = iTabTitle;
            notiCount = iNotiCount;
        }
        public string TabTitle
        {
            get { return tabTitle; }
            set { tabTitle = value; }
        }
        public int Code
        {
            get { return code; }
            set { code = value; }
        }
        public int NotiCount
        {
            get { return notiCount; }
            set { notiCount = value; }
        }

    }
}