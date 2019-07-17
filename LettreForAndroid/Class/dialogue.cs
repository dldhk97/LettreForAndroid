﻿using System;
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
        private string thread_id;   //대화방 고유 ID
        private int category;       //카테고리

        //메세지들의 배열, 대화를 구성함.
        private List<TextMessage> textMessageList;

        public Dialogue()
        {
            textMessageList = new List<TextMessage>();
        }

        public Dialogue(List<TextMessage> iTextMessageList)
        {
            textMessageList = iTextMessageList;
        }

        //대화 내 메세지의 개수
        public int Count
        {
            get { return textMessageList.Count; }
        }

        public int Category
        {
            set { category = value; }
            get { return category; }
        }

        public string Thread_id
        {
            set { thread_id = value; }
            get { return thread_id; }
        }

        //인덱서
        public TextMessage this[int i]
        {
            get { return textMessageList[i]; }
        }

        public void Add(TextMessage textMessage)
        {
            textMessageList.Add(textMessage);
        }

    }

    //일반적인 SMS를 저장하는 객체
    public class TextMessage
    {
        private string id;          //ID
        private string address;     //보낸사람, MMS 메세지는 여기서 번호 안나옴.
        private string msg;         //메세지(body)
        private string person;      //누가 보냈는지 contact와 연관하는 것인데, 뭔지 모름.
        private string readState;   //0은 읽지않음, 1은 읽음.
        private long time;        //메세지를 받거나 보냈던 시간. 밀리세컨드 값으로 나오며, MMS는 여기 안나옴
        private string type;      //폴더, 수신(inbox)인지 발신(sent)인지? 0 혹은 1?
        private string thread_id;   //대화방 고유 ID?

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
        public string Person
        {
            get { return person; }
            set { person = value; }
        }
        public string ReadState
        {
            get { return readState; }
            set { readState = value; }
        }
        public long Time
        {
            get { return time; }
            set { time = value; }
        }
        public string Type
        {
            get { return type; }
            set { type = value; }
        }
        public string Thread_id
        {
            get { return thread_id; }
            set { thread_id = value; }
        }
    }

    public class MultimediaMessage : TextMessage
    {
        private string m_sub;     //MMS의 제목
        private string m_id;
        private string type;  //MMS일때 가짐. 132는 상대방이 보낸 것, 128은 내가보낸 것.
        

        public string M_sub
        {
            get { return m_sub; }
            set { m_sub = value; }
        }
        public string M_id
        {
            get { return m_id; }
            set { m_id = value; }
        }
        public string Type
        {
            get { return type; }
            set { type = value; }
        }
        
    }

}