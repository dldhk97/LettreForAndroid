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
using LettreForAndroid.UI;

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
        private static DialogueSet _TotalDialogueSet = new DialogueSet();
        private static DialogueSet _UnknownDialogueSet = new DialogueSet();

        //객체 생성시 DB에서 문자 다 불러옴
        MessageDBManager()
        {
            _smsManager = SmsManager.Default;
        }

        public static MessageDBManager Get()
        {
            if (_Instance == null)
                _Instance = new MessageDBManager();
            return _Instance;
        }

        public DialogueSet TotalDialogueSet
        {
            get { return _TotalDialogueSet; }
        }

        public DialogueSet UnknownDialogueSet
        {
            get { return _UnknownDialogueSet; }
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
                _TotalDialogueSet.Clear();
                _UnknownDialogueSet.Clear();
            }

            for (int i = 0; i < Dialogue.Lable_COUNT; i++)
            {
                _DialogueSets.Add(new DialogueSet(i));
            }
            _TotalDialogueSet = new DialogueSet();
            _UnknownDialogueSet = new DialogueSet();
        }

        //메시지 DB를 탐색하여, 각 대화중 가장 최신 SMS를 찾아 메모리에 올림.
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
                    UpdateLastMessage(objSMS);
                }
                cursor.Close();
            }
        }

        //메시지 DB를 탐색하여, 각 대화중 가장 최신 MMS를 찾아 메모리에 올림.
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

                        UpdateLastMessage(objMMS);
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

        //해당 대화중 가장 마지막 문자로 갱신함. 대화가 없다면 생성하여 전체탭에 추가.
        private void UpdateLastMessage(TextMessage objSMS)
        {
            //이 thread_id에 해당하는 대화가 없으면 새로 생성 후 전체탭에 추가
            if (FindDialogue(objSMS.Thread_id) == null)
                _TotalDialogueSet.DialogueList.Add(objSMS.Thread_id, CreateNewDialogue(objSMS));

            Dialogue objDialogue = _TotalDialogueSet[objSMS.Thread_id];

            objDialogue.UnreadCnt = CountUnread(objDialogue.Thread_id, true) + CountUnread(objDialogue.Thread_id, false);

            if (objDialogue.Count > 0)
            {
                //대상 문자가 최신이 아니라면 기존 문자 유지, 대상 문자가 더 최신이면 대상 문자로 갱신.
                if (objDialogue[0].Time > objSMS.Time)
                    return;
                else
                    objDialogue.TextMessageList.Clear();
            }
            objDialogue.Add(objSMS);
        }

        //대화가 존재하지 않는 경우 호출되어 대화를 새로 생성함.
        private Dialogue CreateNewDialogue(TextMessage objSMS)
        {
            Dialogue objDialogue = new Dialogue();
            objDialogue.Address = objSMS.Address;
            objDialogue.Contact = ContactDBManager.Get().GetContactDataByAddress(objSMS.Address, false);
            if (objDialogue.Contact != null)
            {
                objDialogue.DisplayName = objDialogue.Contact.Name;
            }
            else
            {
                objDialogue.DisplayName = GetDisplayNameIfUsual(objDialogue.Address);
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

        //문자 DB를 모두 탐색하여, 가장 최신문자만 가져와 메모리에 올린다.
        public void RefreshLastMessageAll()
        {
            //Dialogue Set 초기화
            ResetDialogueSet();

            //SMS와 MMS를 모두 탐색하여 최신문자를 메모리에 올림.
            UpdateLastSMS();
            UpdateLastMMS();

            //로컬 레이블 DB를 바탕으로 각 다이얼로그에 넣음. 레이블DB가 없으면 Unknown에 넣음. (네트워킹 X)
            CategorizeSet(_TotalDialogueSet);

            //각 다이얼로그 대화를 날짜순으로 정렬
            SortDialogueSets();
        }

        //해당 대화가 메모리에 있으면 최신 문자 갱신, 없으면 대화 생성 후 메모리에 올림. 카테고라이즈와 정렬도 함.
        public Dialogue RefreshLastMessage(long thread_id)
        {
            Dialogue objDialogue = LoadDialogue(thread_id, true, (int)TextMessage.MESSAGE_TYPE.ALL);

            if (objDialogue.Count <= 0 || objDialogue == null)
                throw new Exception("메시지 발송 이후, 대화를 찾을 수 업음.");

            //대화 갱신 혹은 신규 생성
            UpdateLastMessage(objDialogue[objDialogue.Count - 1]);

            //갱신된 대화 로드
            objDialogue = LoadDialogue(thread_id, true, (int)TextMessage.MESSAGE_TYPE.ALL);

            //연락처 혹은 레이블 DB를 바탕으로 알맞게 탭에 삽입.
            Categorize(objDialogue);
            
            //대화내 문자들을 시간순으로 정렬
            SortDialogueSets();

            return objDialogue;
        }

        //---------------------------------------------------------------------------------

        //로컬 Lable DB를 바탕으로 대화셋 내 대화를 분류
        public void CategorizeSet(DialogueSet objDialogueSet)
        {
            foreach(Dialogue objDialogue in objDialogueSet.DialogueList.Values)
            {
                Categorize(objDialogue);
            }
        }

        //해당 대화를 로컬 Lable DB를 바탕으로 분류, 네트워크와 연결은 하지 않음.
        public void Categorize(Dialogue objDialogue)
        {
            //연락처에 있으면 대화로, 없으면 로컬 레이블로 설정
            if (objDialogue.Contact != null)
            {
                objDialogue.MajorLable = (int)Dialogue.LableType.COMMON;
            }
            else
            {
                objDialogue.MajorLable = LableDBManager.Get().GetMajorLable(objDialogue.Thread_id);
            }

            //대상 대화목록에 추가
            DialogueSet targetDialogueSet;
            if (objDialogue.MajorLable == (int)Dialogue.LableType.UNKNOWN)
            {
                targetDialogueSet = _UnknownDialogueSet;
            }
            else
            {
                targetDialogueSet = _DialogueSets[objDialogue.MajorLable];
            }

            //해당 대화셋에 현 대화가 포함되어 있지 않으면 삽입.
            if (targetDialogueSet.IsContain(objDialogue.Thread_id) == false)
                targetDialogueSet.DialogueList.Add(objDialogue.Thread_id, objDialogue);
        }

        //카테고라이즈 해야되는 대화는 서버에 보내 분류함. DEBUG!! 아직 호출되지 않음.
        public void ReCategorizeAll()
        {
            foreach (Dialogue objDialogue in _TotalDialogueSet.DialogueList.Values)
            {
                int[] objLables = LableDBManager.Get().GetLables(objDialogue.Thread_id);

                int lablesSum = 0;

                foreach (int lable in objLables)
                    lablesSum += lable;

                //레이블의 합과 문자의 합이 다르다면, 새로 분류해야됨.
                if(lablesSum != objDialogue.Count)
                    ReCategorize(objDialogue);
            }
        }

        //서버로부터 이 대화의 레이블을 받고, 레이블 DB에 업데이트 한 뒤, 이 대화를 메모리에 올림. DEBUG!! 옛날 코드라 제대로 동작안할 가능성이 큼.
        //문자가 최신순으로 되있다면, 마지막 (개수 - 레이블수)개의 문자만 서버로 전송하고, 누적시켜도 될듯?
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

            _UnknownDialogueSet.DialogueList.Remove(dialogue.Thread_id);           //미분류 탭에 남아있는 대화를 삭제함.
            return true;
        }

        //---------------------------------------------------------------------------------
        //로드 메소드들, DB에서 메시지들을 직접 긁어옴

        //해당 thread_id에 해당되는 모든 SMS와 MMS를 합친 대화 반환. needRefresh는 treu면 반드시 리로드 한다. 일반적으로 대화 클릭시 호출됨
        public Dialogue LoadDialogue(long thread_id, bool needRefresh, int inboxType)
        {
            //대화가 이전에 불려졌던 것이라면
            Dialogue objDialogue = FindDialogue(thread_id);
            if (objDialogue != null && objDialogue.Count > 1 && needRefresh == false)
                return objDialogue;

            //없으면 새로 로드 해본다.
            List<TextMessage> smss = LoadSMS(thread_id, inboxType);
            List<TextMessage> mmss = LoadMMS(thread_id, inboxType);
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

        public void ReLoadDialogue(Dialogue objDialogue, int inboxType)
        {
            List<TextMessage> smss = LoadSMS(objDialogue.Thread_id, inboxType);
            List<TextMessage> mmss = LoadMMS(objDialogue.Thread_id, inboxType);
            List<TextMessage> totalMessages = new List<TextMessage>();

            if (smss != null)
                totalMessages.AddRange(smss);
            if (mmss != null)
                totalMessages.AddRange(mmss);

            if (totalMessages.Count > 0)
            {
                totalMessages = totalMessages.OrderByDescending(i => i.Time).ToList();
                if (objDialogue == null)
                    objDialogue = new Dialogue(totalMessages);
                objDialogue.TextMessageList = totalMessages;
            }
        }

        //해당 thread_id에 해당되는 모든 SMS를 불러옴, inboxType에 따라 송신/수신한 메시지만 불러올 수도 있다.
        private List<TextMessage> LoadSMS(long thread_id, int inboxType)
        {
            List<TextMessage> messages = null;
            ContentResolver cr = Application.Context.ContentResolver;

            Uri uri = Uri.Parse("content://sms/");
            string[] projection = new string[] { "_id", "address", "body", "read", "date", "thread_id", "type" };

            string selection = "thread_id = " + thread_id;
            if(inboxType != (int)TextMessage.MESSAGE_TYPE.ALL)
            {
                selection = "thread_id = " + thread_id + " AND type = " + inboxType;
            }

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

        //해당 thread_id에 해당되는 모든 MMS를 불러옴, inboxType에 따라 송신/수신한 메시지만 불러올 수도 있다.
        private List<TextMessage> LoadMMS(long thread_id, int inboxType)
        {
            List<TextMessage> messages = null;
            ContentResolver cr = Application.Context.ContentResolver;
            Uri uri = Uri.Parse("content://mms/");
            string[] projection = new string[] { "_id", "ct_t", "read", "date", "thread_id", "m_type" };

            string selection = "thread_id = " + thread_id;
            if (inboxType != (int)TextMessage.MESSAGE_TYPE.ALL)
            {
                selection = "thread_id = " + thread_id + " AND type = " + inboxType;
            }

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
        // MMS를 읽기 위한 메소드

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

        //-------------------------------------------------------------
        //처음사용자용

        //SMS DB 조회하여 address, thread_id만 수집한 목록을 반환함.
        private DialogueSet LoadSMSMetaDatas()
        {
            DialogueSet dialogueSet = new DialogueSet();

            ContentResolver cr = Application.Context.ContentResolver;

            Uri uri = Uri.Parse("content://sms/");
            string[] projection = { "DISTINCT address", "thread_id" };
            string selection = "address IS NOT NULL) GROUP BY (address";
            ICursor cursor = cr.Query(uri, projection, selection, null, null);

            if (cursor != null && cursor.Count > 0)
            {
                while (cursor.MoveToNext())
                {
                    string address = cursor.GetString(cursor.GetColumnIndexOrThrow("address"));
                    long thread_id = cursor.GetLong(cursor.GetColumnIndexOrThrow("thread_id"));

                    Dialogue objDialogue = new Dialogue();
                    objDialogue.Address = address;
                    objDialogue.Thread_id = thread_id;

                    dialogueSet.DialogueList.Add(thread_id, objDialogue);
                }
                cursor.Close();
            }
            return dialogueSet;
        }

        //MMS DB 조회하여 address, thread_id만 수집한 목록을 반환함.
        private DialogueSet LoadMMSMetaDatas()
        {
            DialogueSet dialogueSet = new DialogueSet();

            ContentResolver cr = Application.Context.ContentResolver;
            Uri uri = Uri.Parse("content://mms/");
            string[] projection = new string[] { "DISTINCT thread_id", "_id", "ct_t"};
            string selection = "thread_id IS NOT NULL) GROUP BY (thread_id";
            ICursor cursor = cr.Query(uri, projection, selection, null, null);

            if (cursor != null && cursor.Count > 0)
            {
                while (cursor.MoveToNext())
                {
                    string id = cursor.GetString(cursor.GetColumnIndex("_id"));
                    string mmsType = cursor.GetString(cursor.GetColumnIndex("ct_t"));
                    long thread_id = cursor.GetLong(cursor.GetColumnIndexOrThrow("thread_id"));
                    if ("application/vnd.wap.multipart.related".Equals(mmsType) || "application/vnd.wap.multipart.mixed".Equals(mmsType))
                    {
                        //this is MMS
                        Dialogue objDialogue = new Dialogue();
                        objDialogue.Thread_id = thread_id;
                        objDialogue.Address = GetAddress(id);

                        dialogueSet.DialogueList.Add(thread_id, objDialogue);
                    }
                    else
                    {
                        //this is SMS
                        throw new Exception("알 수 없는 MMS 유형");
                    }
                }
                cursor.Close();
            }
            return dialogueSet;
        }


        //연락처에 없는 대화 중 수신 메시지만 메모리에 올린다. inboxType으로 송신/수신 메시지만 불러오는것도 가능.
        public void LoadUnknownMetaDatas()
        {
            //SMS, MMS 메타데이터(주소와 thread_id)만 수집
            DialogueSet smsMetaDatas = LoadSMSMetaDatas();
            DialogueSet mmsMetaDatas = LoadMMSMetaDatas();

            //smsMetaData에 병합
            foreach(Dialogue objDialogue in mmsMetaDatas.DialogueList.Values)
            {
                if(!smsMetaDatas.IsContain(objDialogue.Thread_id))
                {
                    smsMetaDatas.DialogueList.Add(objDialogue.Thread_id, objDialogue);
                }
            }

            //연락처에 없는 놈들만 찾는다.
            foreach(Dialogue objDialogue in smsMetaDatas.DialogueList.Values)
            {
                if(ContactDBManager.Get().GetContactDataByAddress(objDialogue.Address, false) == null)
                {
                    _UnknownDialogueSet.DialogueList.Add(objDialogue.Thread_id, objDialogue);
                }
            }
        }

        //--------------------------------------------------------------
        //유틸리티들

        public void SortDialogueSets()
        {
            foreach (DialogueSet dialgoueSet in _DialogueSets)
            {
                dialgoueSet.SortByLastMessageTime();
            }
            _TotalDialogueSet.SortByLastMessageTime();
            _UnknownDialogueSet.SortByLastMessageTime();
        }

        public long GetThreadId(string address)
        {
            return Telephony.Threads.GetOrCreateThreadId(Application.Context, address);      //make new Thread_id
        }

        public Dialogue FindDialogue(long thread_id)
        {
            if (_TotalDialogueSet.IsContain(thread_id))
                return _TotalDialogueSet[thread_id];

            return null;
        }

        public void ChangeReadState(TextMessage msg, int readState)
        {
            ContentValues values = new ContentValues();
            values.Put("read", readState);

            ContentResolver cr = Application.Context.ContentResolver; 
            cr.Update(Uri.Parse("content://sms/"), values, "_id=" + msg.Id, null);
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

        public string GetDisplayNameIfUsual(string address)
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