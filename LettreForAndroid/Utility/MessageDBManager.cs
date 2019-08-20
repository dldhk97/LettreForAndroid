using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android;
using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Telephony;
using Android.Views;
using Android.Widget;
using System.IO;

using LettreForAndroid.Class;
using LettreForAndroid.Receivers;
using Uri = Android.Net.Uri;
using Android.Graphics;

namespace LettreForAndroid.Utility
{
    //싱글톤 사용.
    //리사이클 뷰에서 메세지를 가져올때마다 만들면 너무 비효율적이라 판단하여 사용했음.
    //사용할때는 MessageManager.Get().refreshMessages()이런식으로 사용하면 됨.

    public class MessageDBManager
    {
        private SmsManager _smsManager;

        private static MessageDBManager _Instance = null;
        private static List<DialogueSet> _DialogueSets = new List<DialogueSet>();
        private static DialogueSet _TotalDialogue = new DialogueSet();
        private static DialogueSet _UnknownDialogue = new DialogueSet();

        //객체 생성시 DB에서 문자 다 불러옴
        MessageDBManager()
        {
            _smsManager = SmsManager.Default;

            ReLoad();
        }

        public static MessageDBManager Get()
        {
            if (_Instance == null)
                _Instance = new MessageDBManager();
            return _Instance;
        }

        public DialogueSet TotalDialogue
        {
            get { return _TotalDialogue; }
        }

        public DialogueSet UnknownDialogue
        {
            get { return _UnknownDialogue; }
        }

        public List<DialogueSet> DialogueSets
        {
            get { return _DialogueSets; }
        }

        private void ResetDialogueSet()
        {
            if(_DialogueSets != null)
            {
                foreach (DialogueSet objSet in _DialogueSets)
                {
                    objSet.Clear();
                }
                _DialogueSets.Clear();
                _TotalDialogue.Clear();
                _UnknownDialogue.Clear();
            }

            for (int i = 0; i < Dialogue.Lable_COUNT; i++)
            {
                _DialogueSets.Add(new DialogueSet(i));
            }
            _TotalDialogue = new DialogueSet();
            _UnknownDialogue = new DialogueSet();
        }

        public void UpdateLastSMS()
        {
            ContentResolver cr = Application.Context.ContentResolver;

            Uri uri = Uri.Parse("content://sms/");
            string[] projection = { "DISTINCT address", "_id, thread_id, body, read, date, type" };
            string selection = "address IS NOT NULL) GROUP BY (address";
            string sortOrder = "date desc";
            ICursor cursor = cr.Query(uri, projection, selection, null, sortOrder);

            if (cursor != null && cursor.Count > 0)
            {
                while(cursor.MoveToNext())
                {
                    string id = cursor.GetString(cursor.GetColumnIndexOrThrow("_id"));
                    string body = cursor.GetString(cursor.GetColumnIndexOrThrow("body"));
                    string address = cursor.GetString(cursor.GetColumnIndexOrThrow("address"));
                    int readState = cursor.GetInt(cursor.GetColumnIndex("read"));
                    long time = cursor.GetLong(cursor.GetColumnIndexOrThrow("date"));
                    long thread_id = cursor.GetLong(cursor.GetColumnIndexOrThrow("thread_id"));
                    int type = cursor.GetInt(cursor.GetColumnIndexOrThrow("type"));

                    TextMessage objSMS = new TextMessage(id, address, body, readState, time, type, thread_id);
                    UpdateDialogue(objSMS);
                }
                cursor.Close();
            }
            
        }

        public void UpdateLastMMS()
        {
            ContentResolver cr = Application.Context.ContentResolver;
            Uri uri = Uri.Parse("content://mms/");
            string[] projection = new string[] { "DISTINCT thread_id", "_id", "ct_t", "read", "date", "m_type" };
            string selection = "thread_id IS NOT NULL) GROUP BY (thread_id";
            string sortOrder = "date desc";
            ICursor cursor = cr.Query(uri, projection, selection, null, sortOrder);

            if (cursor != null && cursor.Count > 0)
            {
                while (cursor.MoveToNext())
                {
                    string id = cursor.GetString(cursor.GetColumnIndex("_id"));
                    string mmsType = cursor.GetString(cursor.GetColumnIndex("ct_t"));
                    int readState = cursor.GetInt(cursor.GetColumnIndex("read"));
                    long time = cursor.GetLong(cursor.GetColumnIndex("date"));
                    long thread_id = cursor.GetLong(cursor.GetColumnIndexOrThrow("thread_id"));
                    int type = cursor.GetInt(cursor.GetColumnIndexOrThrow("m_type"));
                    if ("application/vnd.wap.multipart.related".Equals(mmsType) || "application/vnd.wap.multipart.mixed".Equals(mmsType))
                    {
                        //this is MMS
                        MultiMediaMessage objMMS = ReadMMS(id);
                        objMMS.Id = id;
                        objMMS.ReadState = readState;
                        objMMS.Time = time * 1000;          //1000을 곱하지 않으면 1970년으로 표기됨
                        objMMS.Thread_id = thread_id;
                        objMMS.Type = type == 132 ? (int)TextMessage.MESSAGE_TYPE.RECEIVED : (int)TextMessage.MESSAGE_TYPE.SENT;

                        UpdateDialogue(objMMS);
                    }
                    else
                    {
                        //this is SMS
                        throw new Exception("알 수 없는 MMS 유형");
                    }
                }
                cursor.Close();
            }
        }

        //해당 대화중 가장 마지막 문자로 갱신함. 대화가 없다면 새로 생성함.
        private void UpdateDialogue(TextMessage objSMS)
        {
            //이 thread_id에 해당하는 대화가 없으면 새로 생성 후 전체탭에 추가
            if (_TotalDialogue.DialogueList.ContainsKey(objSMS.Thread_id) == false)
                _TotalDialogue.DialogueList.Add(objSMS.Thread_id, CreateNewDialogue(objSMS));

            Dialogue objDialogue = _TotalDialogue[objSMS.Thread_id];
            bool isMMS = objSMS.GetType() == typeof(MultiMediaMessage) ? true : false;
            objDialogue.UnreadCnt = CountUnread(objDialogue.Thread_id, isMMS);

            if (objDialogue.Count > 0)
            {
                if (objDialogue[0].Time > objSMS.Time)
                    return;
                else
                    objDialogue.TextMessageList.Clear();
            }
            objDialogue.Add(objSMS);
        }

        //대화가 존재하지 않는 경우 호출되는 메소드
        private Dialogue CreateNewDialogue(TextMessage objSMS)
        {
            Dialogue objDialogue = new Dialogue();
            objDialogue.Address = objSMS.Address;
            objDialogue.Contact = ContactDBManager.Get().getContactDataByAddress(objSMS.Address);
            if (objDialogue.Contact != null)
            {
                objDialogue.DisplayName = objDialogue.Contact.Name;
            }
            else
            {
                objDialogue.DisplayName = getDisplayNameIfUsual(objDialogue.Address);
            }
            objDialogue.Thread_id = objSMS.Thread_id;
            return objDialogue;
        }

        //해당 대화 중 읽지않은 문자의 수를 셈
        public int CountUnread(long thread_id, bool isMMS)
        {
            string typeStr = isMMS ? "mms" : "sms";
            ContentResolver cr = Application.Context.ContentResolver;

            Uri uri = Uri.Parse("content://" + typeStr + "/");
            string[] projection = new string[] { "read", "thread_id" };
            string selection = "thread_id = " + thread_id + " AND read = 0";
            ICursor cursor = cr.Query(uri, projection, selection, null, null);

            int cnt = cursor.Count;

            if(cursor != null)
                cursor.Close();

            return cnt;
        }

        public void ReLoad()
        {
            //Dialogue Set 초기화
            ResetDialogueSet();

            //Total Dialogue에 추가
            UpdateLastSMS();
            UpdateLastMMS();

            //레이블 DB를 바탕으로 각 다이얼로그에 넣음. 레이블DB가 없으면 Unknown에 넣음.
            Categorize();

            //각 다이얼로그 대화를 날짜순으로 정렬
            SortDialogueSets();
        }

        public void Categorize()
        {
            foreach(Dialogue objDialogue in _TotalDialogue.DialogueList.Values)
            {
                if(objDialogue.Contact != null)
                {
                    objDialogue.MajorLable = (int)Dialogue.LableType.COMMON;
                }
                else
                {
                    objDialogue.MajorLable = LableDBManager.Get().GetMajorLable(objDialogue.Thread_id);
                }
                if(objDialogue.MajorLable == (int)Dialogue.LableType.UNKNOWN)
                    _UnknownDialogue.DialogueList.Add(objDialogue.Thread_id, objDialogue);
                else
                    _DialogueSets[objDialogue.MajorLable].DialogueList.Add(objDialogue.Thread_id, objDialogue);
            }
        }

        //---------------------------------------------------------------------------------
        // 대화 클릭시 호출됨

        //해당 thread_id에 해당되는 모든 SMS와 MMS를 합친 대화 반환. needRefresh는 treu면 반드시 리로드 한다. 메시지 수신시 필요할거 같아서 미리 만듬.
        public Dialogue LoadDialogue(long thread_id, bool needRefresh)
        {
            //대화가 이전에 불려졌던 것이라면
            Dialogue objDialogue = FindDialogue(thread_id);
            if (objDialogue != null && objDialogue.Count > 1 && needRefresh == false)
                return objDialogue;

            //없으면 새로 로드 해본다.
            List<TextMessage> smss = LoadSMS(thread_id);
            List<TextMessage> mmss = LoadMMS(thread_id);
            List<TextMessage> totalMessages = new List<TextMessage>();

            if (smss != null)
                totalMessages.AddRange(smss);
            if (mmss != null)
                totalMessages.AddRange(mmss);

            if(totalMessages.Count > 0)
            {
                totalMessages = totalMessages.OrderByDescending(i => i.Time).ToList();
                if (objDialogue == null)
                    objDialogue = new Dialogue(totalMessages);
                objDialogue.TextMessageList = totalMessages;
            }
            return objDialogue;
        }

        //해당 thread_id에 해당되는 모든 SMS를 불러옴
        private List<TextMessage> LoadSMS(long thread_id)
        {
            List<TextMessage> messages = null;
            ContentResolver cr = Application.Context.ContentResolver;

            Uri uri = Uri.Parse("content://sms/");
            string[] projection = new string[] { "_id", "address", "body", "read", "date", "thread_id", "type" };
            string selection = "thread_id = " + thread_id;
            string sortOrder = "date desc";
            ICursor cursor = cr.Query(uri, projection, selection, null, sortOrder);

            if (cursor != null && cursor.Count > 0)
            {
                messages = new List<TextMessage>();
                while (cursor.MoveToNext())
                {
                    string id = cursor.GetString(cursor.GetColumnIndexOrThrow("_id"));
                    string address = cursor.GetString(cursor.GetColumnIndexOrThrow("address"));
                    string body = cursor.GetString(cursor.GetColumnIndexOrThrow("body"));
                    int readState = cursor.GetInt(cursor.GetColumnIndex("read"));
                    long time = cursor.GetLong(cursor.GetColumnIndexOrThrow("date"));
                    int type = cursor.GetInt(cursor.GetColumnIndexOrThrow("type"));

                    TextMessage objSMS = new TextMessage(id, address, body, readState, time, type, thread_id);
                    messages.Add(objSMS);
                }
                cursor.Close();
            }
            return messages;
        }

        //해당 thread_id에 해당되는 모든 MMS를 불러옴
        private List<TextMessage> LoadMMS(long thread_id)
        {
            List<TextMessage> messages = null;
            ContentResolver cr = Application.Context.ContentResolver;
            Uri uri = Uri.Parse("content://mms/");
            string[] projection = new string[] { "_id", "ct_t", "read", "date", "thread_id", "m_type" };
            string selection = "thread_id = " + thread_id;
            string sortOrder = "date desc";
            ICursor cursor = cr.Query(uri, projection, selection, null, sortOrder);

            if (cursor != null && cursor.Count > 0)
            {
                messages = new List<TextMessage>();
                while (cursor.MoveToNext())
                {
                    string id = cursor.GetString(cursor.GetColumnIndex("_id"));
                    string mmsType = cursor.GetString(cursor.GetColumnIndex("ct_t"));
                    int readState = cursor.GetInt(cursor.GetColumnIndex("read"));
                    long time = cursor.GetLong(cursor.GetColumnIndex("date"));
                    int type = cursor.GetInt(cursor.GetColumnIndexOrThrow("m_type"));
                    if ("application/vnd.wap.multipart.related".Equals(mmsType) || "application/vnd.wap.multipart.mixed".Equals(mmsType))
                    {
                        //this is MMS
                        MultiMediaMessage objMMS = ReadMMS(id);
                        objMMS.Id = id;
                        objMMS.ReadState = readState;
                        objMMS.Time = time * 1000;
                        objMMS.Thread_id = thread_id;
                        objMMS.Type = type == 132 ? (int)TextMessage.MESSAGE_TYPE.RECEIVED : (int)TextMessage.MESSAGE_TYPE.SENT;

                        //Add to List
                        messages.Add(objMMS);
                    }
                    else
                    {
                        //this is SMS
                        throw new Exception("알 수 없는 MMS 유형");
                    }
                }
                cursor.Close();
            }
            return messages;
        }


        // ---------------------------------------------------------------------------------
        // For MMS

        private MultiMediaMessage ReadMMS(string id)
        {
            ContentResolver cr = Application.Context.ContentResolver;
            string selection = "mid = " + id;
            string[] projection = new string[] { "_id", "ct", "_data", "text" };
            Uri uri = Uri.Parse("content://mms/part");
            ICursor cursor = cr.Query(uri, projection, selection, null, null);

            MultiMediaMessage objMMS = new MultiMediaMessage(); ;

            if (cursor != null && cursor.Count > 0)
            {
                while (cursor.MoveToNext())
                {
                    string partId = cursor.GetString(cursor.GetColumnIndexOrThrow("_id"));
                    string mediaType = cursor.GetString(cursor.GetColumnIndexOrThrow("ct"));
                    if ("application/smil".Equals(mediaType))
                    {
                        //smil은 무시한다?
                        continue;
                    }

                    objMMS.Address = GetAddress(id);

                    if ("text/plain".Equals(mediaType))
                    {
                        string data = cursor.GetString(cursor.GetColumnIndexOrThrow("_data"));
                        string body;
                        if (data != null)
                        {
                            body = GetMMSText(partId);
                        }
                        else
                        {
                            body = cursor.GetString(cursor.GetColumnIndexOrThrow("text"));
                        }
                        objMMS.Msg = body;
                        objMMS.MediaType = (int)MultiMediaMessage.MEDIA_TYPE.TEXT;
                    }
                    else if ("image/jpeg".Equals(mediaType) || "image/bmp".Equals(mediaType) || "image/gif".Equals(mediaType) || 
                        "image/jpg".Equals(mediaType) || "image/png".Equals(mediaType))
                    {
                        objMMS.Bitmap = GetMMSImage(partId); 
                        objMMS.MediaType = (int)MultiMediaMessage.MEDIA_TYPE.IMAGE;
                    }
                    else if("text/x-vCard".Equals(mediaType))
                    {
                        //.vcf는 처리 어케하지
                        objMMS.MediaType = (int)MultiMediaMessage.MEDIA_TYPE.VCF;
                    }
                    else
                    {
                        //throw new Exception("알 수 없는 MMS 타입");
                    }
                }
                cursor.Close();
            }
            return objMMS;
        }

        private string GetMMSText(string partId)
        {
            ContentResolver cr = Application.Context.ContentResolver;
            Uri uri = Uri.Parse("content://mms/part/" + partId);
            Stream stream = null;
            StringBuilder sb = new StringBuilder();
            try
            {
                stream = cr.OpenInputStream(uri);
                if (stream != null)
                {
                    StreamReader sr = new StreamReader(stream, Encoding.UTF8);
                    string temp = sr.ReadLine();
                    while (temp != null)
                    {
                        sb.Append(temp);
                        temp = sr.ReadLine();
                    }
                }
            }
            catch (IOException e) { }
            finally
            {
                if(stream != null)
                {
                    try { stream.Close(); }
                    catch (IOException e) { }
                }
            }
            return sb.ToString();
        }

        private Bitmap GetMMSImage(string partId)
        {
            ContentResolver cr = Application.Context.ContentResolver;
            Uri uri = Uri.Parse("content://mms/part/" + partId);
            Stream stream = null;
            Bitmap bitmap = null;
            try
            {
                stream = stream = cr.OpenInputStream(uri);
                bitmap = BitmapFactory.DecodeStream(stream);
            }
            catch (IOException e) { }
            finally
            {
                if(stream != null)
                {
                    try { stream.Close(); }
                    catch (IOException e) { }
                }
            }
            return bitmap;
        }

        private string GetAddress(string mmsId)
        {
            ContentResolver cr = Application.Context.ContentResolver;
            Uri uri = Uri.Parse("content://mms/" + mmsId + "/addr");
            string[] projection = new string[] { "type", "address" };
            string selection = "msg_id = " + mmsId;
            ICursor cursor = cr.Query(uri, projection, selection, null, null);

            string address = string.Empty;

            if(cursor != null && cursor.Count > 0)
            {
                while(cursor.MoveToNext())
                {
                    int type = cursor.GetInt(cursor.GetColumnIndexOrThrow("type"));     //137은 from, 151은 to... 나중에 ENUM 으로 바꿔라 DEBUG!
                    if(type == 137)
                    {
                        address = cursor.GetString(cursor.GetColumnIndexOrThrow("address"));
                        break;
                    }
                        
                }
                cursor.Close();
            }
            return address;
        }

        //모든 문자메세지를 대화로 묶어 _DialogueSet[0] = 전체에만 저장
        //public void Load()
        //{
        //    ResetDialogueSet();

        //    ContentResolver cr = Application.Context.ContentResolver;

        //    //DB 탐색 SQL문 설정
        //    Uri uri = Uri.Parse("content://sms/");
        //    string[] projection = new string[] {"_id", "address", "thread_id", "body", "read", "date", "type" };  //SELECT 절에 해당함. DISTINCT는 반드시 배열 앞부분에 등장해야함.
        //    string sortOrder = "thread_id asc, date desc";                                                                  //정렬조건
        //    ICursor cursor = cr.Query(uri, projection, null, null, sortOrder);

        //    //탐색 시작
        //    if (cursor != null && cursor.Count > 0)
        //    {
        //        long prevThreadId = -1;
        //        Dialogue objDialogue = new Dialogue();

        //        while(cursor.MoveToNext())
        //        {
        //            string id = cursor.GetString(cursor.GetColumnIndexOrThrow("_id"));
        //            string address = cursor.GetString(cursor.GetColumnIndexOrThrow("address"));
        //            string msg = cursor.GetString(cursor.GetColumnIndexOrThrow("body"));
        //            int readState = cursor.GetInt(cursor.GetColumnIndex("read"));
        //            long time = cursor.GetLong(cursor.GetColumnIndexOrThrow("date"));
        //            long thread_id = cursor.GetLong(cursor.GetColumnIndexOrThrow("thread_id"));
        //            int type = cursor.GetInt(cursor.GetColumnIndexOrThrow("type"));

        //            TextMessage objSms = new TextMessage(id, address, msg, readState, time, type, thread_id);

        //            //탐색한 메세지의 Thread_id가 이전과 다르다면 새 대화임.
        //            if (objSms.Thread_id != prevThreadId)
        //            {
        //                objDialogue = new Dialogue();
        //                objDialogue.Contact = ContactDBManager.Get().getContactDataByAddress(objSms.Address);
        //                objDialogue.Thread_id = objSms.Thread_id;
        //                objDialogue.Address = objSms.Address;

        //                //연락처에 있으면 대화로 분류
        //                if (objDialogue.Contact != null)
        //                {
        //                    objDialogue.DisplayName = objDialogue.Contact.Name;
        //                    objDialogue.MajorLable = (int)Dialogue.LableType.COMMON;
        //                }
        //                else
        //                {
        //                    //연락처에 없는 대화는, 레이블 분석에 따라 MajorLable이 변경될수 있으므로 여기서 MajorLable을 결정하지 않음. 미분류로 보냄.
        //                    objDialogue.DisplayName = getDisplayNameIfUsual(objDialogue.Address);
        //                    objDialogue.MajorLable = (int)Dialogue.LableType.UNKNOWN;
        //                }

        //                _DialogueSets[objDialogue.MajorLable].InsertOrUpdate(objDialogue);                           //알맞게 리스트에 추가
        //                _DialogueSets[(int)Dialogue.LableType.ALL].InsertOrUpdate(objDialogue);                      //전체 리스트에 추가

        //                prevThreadId = objSms.Thread_id;
        //            }
        //            if (objSms.ReadState == (int)TextMessage.MESSAGE_READSTATE.UNREAD)                              //읽지 않은 문자면, 카운트 추가
        //                objDialogue.UnreadCnt++;

        //            objDialogue.Add(objSms);
        //        }
        //    }
        //    cursor.Close();
        //}

        public void SortDialogueSets()
        {
            foreach (DialogueSet dialgoueSet in _DialogueSets)
            {
                dialgoueSet.SortByLastMessageTime();
            }
            _TotalDialogue.SortByLastMessageTime();
            _UnknownDialogue.SortByLastMessageTime();
        }

        public long GetThreadId(string address)
        {
            return Telephony.Threads.GetOrCreateThreadId(Application.Context, address);      //make new Thread_id
        }

        public Dialogue FindDialogue(long thread_id)
        {
            if (_TotalDialogue.IsContain(thread_id))
                return _TotalDialogue[thread_id];

            return null;
        }

        public void ChangeReadState(TextMessage msg, int readState)
        {
            ContentValues values = new ContentValues();
            values.Put("read", readState);

            ContentResolver cr = Application.Context.ContentResolver; 
            cr.Update(Uri.Parse("content://sms/"), values, "_id=" + msg.Id, null);
        }
        
        //해당 대화집합이 모두 LableDB에 포함되어있다는 가정 하에 문자를 분류해서 메모리에 올림. 네트워크에 연결하지 않음.
        //public void CategorizeLocally(DialogueSet dialogueSet)
        //{
        //    List<Dialogue> unlabledTarget = new List<Dialogue>();                 //연락처에 없는 대화들은 모두 미분류 카테고리에 있음. 분류가 되면 미분류탭에서 삭제해야함.
        //    List<KeyValuePair<int, Dialogue>> lableChangedTarget = new List<KeyValuePair<int, Dialogue>>();          //분류가 변경된 대화들

        //    //대화들을 탐색. Lable DB에 있는지 확인함.
        //    foreach (Dialogue objDialogue in dialogueSet.DialogueList.Values)
        //    {
        //        //연락처가 있으면 대화이므로 패스
        //        if (objDialogue.Contact != null)
        //            continue;

        //        int prevMajorLable = objDialogue.MajorLable;
        //        int majorLable = LableDBManager.Get().GetMajorLable(objDialogue.Thread_id);
        //        int[] lables = LableDBManager.Get().GetLables(objDialogue.Thread_id);

        //        if (majorLable == -1 || lables == null)                                 //레이블 DB에 없는것이 있다? -> 서버와 통신이 안됬을때 새 문자가 생긴 경우. 미분류로 처리
        //        {
        //            continue;
        //            //throw new Exception("레이블 DB에 없는 데이터가 있는데 CategorizeLocally 호출됨. 왜 호출됬는지 알아봐라");
        //        }
                    

        //        objDialogue.MajorLable = majorLable;                                    //대표 레이블 설정
        //        lables.CopyTo(objDialogue.Lables, 0);                                   //레이블 DB에서 레이블 배열을 메모리에 복사
        //        _DialogueSets[majorLable].InsertOrUpdate(objDialogue);                  //레이블에 맞는 셋에 대화 추가

        //        unlabledTarget.Add(objDialogue);                                  //미분류탭 삭제대상으로 선정

        //        if (prevMajorLable != majorLable)                                       //분류가 레이블 DB갱신으로 인해 변경된 경우 이전 탭에서 삭제대상으로 선정
        //            lableChangedTarget.Add(new KeyValuePair<int, Dialogue>(prevMajorLable, objDialogue));
        //    }

        //    //미분류 탭에 남아있는 대화를 삭제
        //    foreach(Dialogue objDialogue in unlabledTarget)
        //    {
        //        _DialogueSets[(int)Dialogue.LableType.UNKNOWN].DialogueList.Remove(objDialogue.Thread_id);
        //    }

        //    //분류가 변경된 대화들은 이전 탭에서 삭제
        //    foreach(KeyValuePair<int, Dialogue> objDialogue in lableChangedTarget)
        //    {
        //        _DialogueSets[objDialogue.Key].DialogueList.Remove(objDialogue.Value.Thread_id);
        //    }
        //}

        ////어플이 실행했됬을 때, 네트워크에 다시 연결됬을 때 분류되지 않은 메시지가 있으면 그것을 분류함. 네트워크에 연결함.
        //public void CategorizeNewMsg()
        //{
        //    bool isNetworkConnected = true;
        //    foreach(Dialogue objDialogue in _DialogueSets[(int)Dialogue.LableType.ALL].DialogueList.Values)
        //    {
        //        //연락처가 있는 대화는 분류하지 않는다.
        //        if (objDialogue.Contact != null)
        //            continue;

        //        int[] objLables = LableDBManager.Get().GetLables(objDialogue.Thread_id);

        //        //레이블 DB에 이 대화가 없으면. 새 대화로 간주, 레이블DB에 새 값을 넣음.
        //        if (objLables == null)
        //        {
        //            if (ReCategorize(objDialogue) == false)     //만약 서버 통신 실패면 중단
        //            {
        //                isNetworkConnected = false;
        //                break;
        //            }
        //        }
        //        else
        //        {
        //            //이 대화에서 레이블이 붙여진 문자의 수를 센다.
        //            int msgCnt = 0;
        //            for (int i = 0; i < objLables.Length; i++)
        //                msgCnt += objLables[i];
                    
        //            //실제 존재하는 문자수와 레이블 붙여진 문자수가 다르면, 레이블이 붙여지지 않은 문자가 존재한다는 의미.
        //            if(msgCnt != objDialogue.Count)
        //            {
        //                //신규메시지 존재하는 경우 이 대화를 다시 레이블 매김.
        //                //한 메세지만 보내는게 아니라 대화 전체를 보내는데, 이는 어느 메세지가 분류되지 않은것인지 모르기 때문.
        //                if (ReCategorize(objDialogue) == false) 
        //                {
        //                    isNetworkConnected = false;
        //                    break;      //서버 통신 실패시 중단함.
        //                }
        //            }
        //        }
        //    }

        //    if(isNetworkConnected == false)
        //    {
        //        Toast.MakeText(Application.Context, "서버와 통신이 원할하지 않아 미분류된 문자가 존재합니다.", ToastLength.Long).Show();
        //    }
        //}

        //서버로부터 이 대화의 레이블을 받고, 레이블 DB에 업데이트 한 뒤, 이 대화를 메모리에 올림
        public bool ReCategorize(Dialogue dialogue)
        {
            if (dialogue.Count <= 0)
                return true;

            int majorLable = LableDBManager.Get().GetMajorLable(dialogue.Thread_id);    //레이블 DB에 저장되있는 대표 레이블 가져옴 
            int[] lables = LableDBManager.Get().GetLables(dialogue.Thread_id);          //레이블 DB에 저장되있는 레이블들 가져옴

            LableDBManager.Get().UpdateLableDB(dialogue);
            majorLable = LableDBManager.Get().GetMajorLable(dialogue.Thread_id); //업데이트된 대표 레이블 가져옴
            lables = LableDBManager.Get().GetLables(dialogue.Thread_id);         //업데이트된 레이블 가져옴

            if (majorLable == -1 || lables == null)                             //서버 통신 실패면 아무것도 하지 않음.
                return false;

            dialogue.MajorLable = majorLable;                                    //대표 레이블 설정
            lables.CopyTo(dialogue.Lables, 0);                                   //레이블 DB에서 레이블 배열을 메모리에 복사
            _DialogueSets[majorLable].InsertOrUpdate(dialogue);                  //레이블에 맞는 셋에 대화 추가

            _UnknownDialogue.DialogueList.Remove(dialogue.Thread_id); //미분류 탭에 남아있는 대화를 삭제함.
            return true;
        }

        //-------------------------------------------
        //Insert Sent Message
        public void InsertMessage(string address, string msgBody, int readState, int type)
        {
            //문자를 DB에 저장
            ContentValues values = new ContentValues();
            values.Put(Telephony.TextBasedSmsColumns.Address, address);
            values.Put(Telephony.TextBasedSmsColumns.Body, msgBody);
            DateTimeUtillity dtu = new DateTimeUtillity();
            values.Put(Telephony.TextBasedSmsColumns.Date, dtu.getCurrentMilTime());
            values.Put(Telephony.TextBasedSmsColumns.Read, readState);
            values.Put(Telephony.TextBasedSmsColumns.Type, type);
            long thread_id = GetThreadId(address);
            values.Put(Telephony.TextBasedSmsColumns.ThreadId, thread_id);
            Application.Context.ContentResolver.Insert(Telephony.Sms.Sent.ContentUri, values);
        }

        public string getDisplayNameIfUsual(string address)
        {
            switch (address)
            {
                case "":
                    return "알 수 없음";
                case "#CMAS#CMASALL":
                    return "긴급 재난 문자";
                case "#CMAS#Severe":
                    return "안전 안내 문자";
                default:
                    return address;
            }
        }
    }

    //--------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------
    

    public static class MessageSender
    {
        static SmsManager _SmsManager = SmsManager.Default;

        public static void SendSms(Activity activity, string address, string msg)
        {
            //권한 체크
            if (PermissionManager.HasPermission(Application.Context, PermissionManager.sendSMSPermission) == false)
            {
                Toast.MakeText(activity, "메시지 발송을 위한 권한이 없습니다.", ToastLength.Long).Show();
                PermissionManager.RequestPermission(
                    activity,
                    PermissionManager.sendSMSPermission,
                    "버튼을 눌러 권한을 승인해주세요.",
                    (int)PermissionManager.REQUESTS.SENDSMS
                    );
            }
            else
            {
                //권한이 있다면 바로 발송
                var piSent = PendingIntent.GetBroadcast(Application.Context, 0, new Intent(SmsSentReceiver.FILTER_SENT), 0);
                //var piDelivered = PendingIntent.GetBroadcast(Application.Context, 0, new Intent(SmsDeliverer.FILTER_DELIVERED), 0);

                _SmsManager.SendTextMessage(address, null, msg, piSent, null);
            }
        }
    }


    


}