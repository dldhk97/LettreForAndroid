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
    //TextMessage집합 개체, 특정 인물(연락처)와 대화
    public class Dialogue
    {
        private string address;     //이 대화의 주체
        private string name;
        private string category;    //카테고리

        //메세지들의 배열, 대화를 구성함.
        private TextMessage[] textMessages;

        //임시 데이터, 데이터베이스로 대체할 것.
        TextMessage[] tempTextMessages =
        {
            new TextMessage {Id = "1", Address = "010-1234-1234", Msg = "헬로 월드!", ReadState = "0", Time = "2019-07-11 14:02", Folder = "0"},
            new TextMessage {Id = "2", Address = "010-1234-2234", Msg = "안녕 세상아!", ReadState = "0", Time = "2019-07-12 14:02", Folder = "0"},
            new TextMessage {Id = "3", Address = "010-1234-3234", Msg = "Hello World!", ReadState = "0", Time = "2019-07-13 14:02", Folder = "0"},
            new TextMessage {Id = "4", Address = "010-1234-4234", Msg = "더 월드!", ReadState = "0", Time = "2019-07-14 14:02", Folder = "0"},
            new TextMessage {Id = "5", Address = "010-1234-1234", Msg = "헬로 월드!", ReadState = "0", Time = "2019-07-11 14:02", Folder = "0"},
            new TextMessage {Id = "6", Address = "010-1234-2234", Msg = "안녕 세상아!", ReadState = "0", Time = "2019-07-12 14:02", Folder = "0"},
            new TextMessage {Id = "7", Address = "010-1234-3234", Msg = "Hello World!", ReadState = "0", Time = "2019-07-13 14:02", Folder = "0"},
            new TextMessage {Id = "8", Address = "010-1234-4234", Msg = "더 월드!", ReadState = "0", Time = "2019-07-14 14:02", Folder = "0"},
            new TextMessage {Id = "9", Address = "010-1234-1234", Msg = "헬로 월드!", ReadState = "0", Time = "2019-07-11 14:02", Folder = "0"},
            new TextMessage {Id = "10", Address = "010-1234-2234", Msg = "안녕 세상아!", ReadState = "0", Time = "2019-07-12 14:02", Folder = "0"},
            new TextMessage {Id = "11", Address = "010-1234-3234", Msg = "Hello World!", ReadState = "0", Time = "2019-07-13 14:02", Folder = "0"},
            new TextMessage {Id = "12", Address = "010-1234-4234", Msg = "더 월드!", ReadState = "0", Time = "2019-07-14 14:02", Folder = "0"},
        };

        public Dialogue()
        {
            //임시 데이터 삽입
            textMessages = tempTextMessages;
        }

        //대화 내 메세지의 개수
        public int NumMessages
        {
            get { return textMessages.Length; }
        }

        //인덱서
        public TextMessage this[int i]
        {
            get { return textMessages[i]; }
        }

    }

    //일반적인 SMS를 저장하는 객체
    public class TextMessage
    {
        private string id;          //ID
        private string address;     //보낸사람
        private string msg;         //메세지
        private string readState;   //0은 읽지않음, 1은 읽음.
        private string time;        //시간
        private string folder;      //폴더, 수신(inbox)인지 발신(sent)인지

        public string Id
        {
            get { return id; }
            set { id = value; }
        }
        public string Address
        {
            get { return address; }
            set { address = value; }
        }
        public string Msg
        {
            get { return msg; }
            set { msg = value; }
        }
        public string ReadState
        {
            get { return readState; }
            set { readState = value; }
        }
        public string Time
        {
            get { return time; }
            set { time = value; }
        }
        public string Folder
        {
            get { return folder; }
            set { folder = value; }
        }
    }

    public class MultimediaMessage : TextMessage
    {
        private string smil;
    }

}