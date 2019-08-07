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

using System.Net;
using System.IO;

namespace LettreForAndroid.Utility
{
    class SpamFilter
    {
        private void Crawl(string address)
        {
            string cookies = GetCookie("https://m.search.daum.net/search?nil_profile=btn&w=tot&DA=SBC&q=" + address);
            int startIdx = cookies.IndexOf("uvkey=") + "uvkey=".Length;
            int endIdx = cookies.IndexOf("; ", startIdx);
            string uk = cookies.Substring(startIdx, endIdx - startIdx);

            try
            {
                string url = "https://m.search.daum.net/qsearch?uk=" + uk + "&q=spam&w=spamcall2&PHONE_NUMBER=" + address;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                if (request.CookieContainer == null)
                    request.CookieContainer = new CookieContainer();

                Cookie a = new Cookie("uvkey", uk) { Domain = ".search.daum.net" }; ;

                request.CookieContainer.Add(a);

                HttpWebResponse myResp = (HttpWebResponse)request.GetResponse();
                string responseText;

                using (var response = request.GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        responseText = reader.ReadToEnd();
                    }
                }

                string[] splitedStr = responseText.Split("\"");
                string code = splitedStr[4];
                if (code != ":9001},")
                {
                    string likeCnt = splitedStr[10];
                    string dislikeCnt = splitedStr[12];
                    string title = splitedStr[15];
                    Toast.MakeText(Application.Context, "좋아요 : " + likeCnt + ", 싫어요 : " + dislikeCnt + ", 유형 : " + title, ToastLength.Short).Show();
                }
                else
                {
                    //일반 연락처임.
                }
            }
            catch (WebException exception)
            {
                string responseText;
                using (var reader = new StreamReader(exception.Response.GetResponseStream()))
                {
                    responseText = reader.ReadToEnd();
                }
            }
        }

        private string GetCookie(string url)
        {
            HttpWebRequest request = null;
            request = HttpWebRequest.Create(url) as HttpWebRequest;
            HttpWebResponse TheRespone = (HttpWebResponse)request.GetResponse();
            string setCookieHeader = TheRespone.Headers[HttpResponseHeader.SetCookie];
            return setCookieHeader;
        }

    }
}