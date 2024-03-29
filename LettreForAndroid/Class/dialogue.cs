﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using LettreForAndroid.Utility;

namespace LettreForAndroid.Class
{
    //DialogueSet > Dialogue > Message
    //DialogueSetList = 전체, DialogueSet(dialogueList) = 한 탭, Dialgoue = 한 사람과의 대화, TextMessage = 한 문자
    public class DialogueSet
    {
        private Dictionary<long, Dialogue> dialogueList;
        int lable;

        public DialogueSet()
        {
            dialogueList = new Dictionary<long, Dialogue>();
        }

        public DialogueSet(int lable)
        {
            dialogueList = new Dictionary<long, Dialogue>();
            this.lable = lable;
        }

        public Dictionary<long, Dialogue> DialogueList
        {
            get { return dialogueList; }
            set { dialogueList = value; }
        }

        //탭 내 대화(채팅방)의 개수
        public int Count
        {
            get { return dialogueList.Count; }
        }

        //Thread_id로 접근
        public Dialogue this[long thread_id]
        {
            get { return dialogueList[thread_id]; }
        }

        //인덱스로 접근
        public Dialogue this[int index]
        {
            get { return dialogueList.Values.ToList()[index]; }
        }

        public bool IsContain(long thread_id)
        {
            return dialogueList.ContainsKey(thread_id);
        }

        public int Lable
        {
            get { return lable; }
            set { lable = value; }
        }

        public void InsertOrUpdate(Dialogue dialogue)
        {
            if (dialogueList.ContainsKey(dialogue.Thread_id))
                dialogueList[dialogue.Thread_id] = dialogue;
            else
                dialogueList.Add(dialogue.Thread_id, dialogue);
        }

        public void SortByLastMessageTime()
        {
            dialogueList = dialogueList.OrderByDescending(i => i.Value[0].Time).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public void Clear()
        {
            foreach(Dialogue dialogue in dialogueList.Values)
            {
                dialogue.TextMessageList.Clear();
            }
            dialogueList.Clear();
            dialogueList = new Dictionary<long, Dialogue>();
        }
    }


    //TextMessage집합 개체, 특정 인물(연락처)와 대화
    public class Dialogue
    {
        public const int Lable_COUNT = 7;           //시스템 레이블(전체, 미분류)을 제외한 레이블 수
        public static string[] _LableTypeStr = { "대화", "택배", "카드", "인증", "공공기관", "통신사", "기타", "전체", "미분류" };
        public enum LableType { COMMON = 0, DELIVERY, CARD, IDENTIFICATION, PUBLIC, AGENCY, ETC, ALL, UNKNOWN };

        private ContactData contact;
        private int majorLable;                         //대표 레이블
        private int[] lables = new int[Lable_COUNT];    //레이블
        private string displayName;                     //화면상 표시되는 전화번호 혹은 이름
        private int unreadCnt = 0;
        private long thread_id;
        private string address;

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

        public ContactData Contact
        {
            set { contact = value; }
            get { return contact; }
        }

        public int MajorLable
        {
            set { majorLable = value; }
            get { return majorLable; }
        }

        public int[] Lables
        {
            set { lables = value; }
            get { return lables; }
        }

        public string DisplayName
        {
            set { displayName = value; }
            get { return displayName; }
        }

        public int UnreadCnt
        {
            set { unreadCnt = value; }
            get { return unreadCnt; }
        }

        public long Thread_id
        {
            set { thread_id = value; }
            get { return thread_id; }
        }

        public string Address
        {
            set { address = value; }
            get { return address; }
        }

        public List<TextMessage> TextMessageList
        {
            set { textMessageList = value; }
            get { return textMessageList; }
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
        public enum MESSAGE_TYPE { ALL = -1, RECEIVED = 1, SENT = 2 };
        public enum MESSAGE_READSTATE { UNREAD = 0, READ = 1};

        private string id;          //ID
        private string address;     //보낸사람, MMS 메세지는 여기서 번호 안나옴.
        private string msg;         //메세지(body)
        private int readState;   //0은 읽지않음, 1은 읽음.
        private long time;           //메세지를 받거나 보냈던 시간. 밀리세컨드 값으로 나오며, MMS는 여기 안나옴
        private int type;           //1은 상대방이 보낸 것, 2는 내가 보낸 것           mms일때 132는 수신, 128은 송신한 MMS
        private long thread_id;   //대화방 고유 ID?

        public TextMessage()
        {

        }

        public TextMessage(string id, string address, string msg, int readState, long time, int type, long thread_id)
        {
            this.id = id;
            this.address = address;
            this.msg = msg;
            this.readState = readState;
            this.time = time;
            this.type = type;
            this.thread_id = thread_id;
        }

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
        public int ReadState
        {
            get { return readState; }
            set { readState = value; }
        }
        public long Time
        {
            get { return time; }
            set { time = value; }
        }
        public int Type
        {
            get { return type; }
            set { type = value; }
        }
        public long Thread_id
        {
            get { return thread_id; }
            set { thread_id = value; }
        }
    }

    public class MultiMediaMessage : TextMessage
    {
        public enum MEDIA_TYPE { TEXT = 0, IMAGE, VCF }

        private string m_id;
        private int mediaType;
        private Bitmap bitmap;

        public string M_id
        {
            get { return m_id; }
            set { m_id = value; }
        }

        public int MediaType
        {
            get { return mediaType; }
            set { mediaType = value; }
        }

        public Bitmap Bitmap
        {
            get { return bitmap; }
            set { bitmap = value; }
        }

    }

}