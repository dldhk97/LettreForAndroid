using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using LettreForAndroid.Class;

using Uri = Android.Net.Uri;

namespace LettreForAndroid.Utility
{
    //싱글톤 사용.
    //리사이클 뷰에서 메세지를 가져올때마다 만들면 너무 비효율적이라 판단하여 사용했음.
    //사용할때는 MessageManager.Get().refreshMessages()이런식으로 사용하면 됨.

    public class MessageManager
    {
        private static Activity mActivity;
        private static MessageManager mInstance = null;
        private static List<DialogueSet> mDialogueSets;     //인덱스 = 카테고리인 총 문자 집합, 0번 인덱스는 비어있다.

        public static MessageManager Get()
        {
            if (mInstance == null)
                mInstance = new MessageManager();
            return mInstance;
        }

        public List<DialogueSet> DialogueSets
        {
            get { return mDialogueSets; }
        }

        //activity가 있어야 하기 때문에 처음 한번만 이 메소드로 activity를 설정해줘야 함.
        public void Initialization(Activity iActivity)
        {
            mActivity = iActivity;

            mDialogueSets = new List<DialogueSet>();
            for (int i = 0; i < TabFrag.CATEGORY_COUNT; i++)
            {
                mDialogueSets.Add(new DialogueSet());
                mDialogueSets[i].Category = i;
            }
                
            refreshMessages();
        }

        //모든 문자메세지를 thread_id별로 묶어 mAllDialgoues에 저장
        public void refreshMessages()
        {

            TextMessage objSms = new TextMessage();

            ContentResolver cr = mActivity.BaseContext.ContentResolver;

            //DB 탐색 SQL문 설정
            Uri uri = Uri.Parse("content://sms/");
            string[] projection = new string[] {"_id", "address", "thread_id", "person", "body", "read", "date", "type" };  //SELECT 절에 해당함. DISTINCT는 반드시 배열 앞부분에 등장해야함.
            //string selectionClause = "address = ?";                //WHERE 절에 해당함
            //string[] selectionArgs = {"114"};                     //Selection을 지정했을 때 Where절에 해당하는 값들을 배열로 적어야댐.
            string sortOrder = "thread_id asc, date desc";                   //정렬조건
            ICursor cursor = cr.Query(uri, projection, null, null, sortOrder);

            mActivity.StartManagingCursor(cursor);
            int totalSMS = cursor.Count;

            //탐색 시작
            if (cursor.MoveToFirst())
            {
                string prevThreadId = "NULL";
                Dialogue objDialogue = new Dialogue();
                for (int i = 0; i < totalSMS; i++)
                {
                    objSms = new TextMessage();
                    objSms.Id = cursor.GetString(cursor.GetColumnIndexOrThrow("_id"));
                    objSms.Address = cursor.GetString(cursor.GetColumnIndexOrThrow("address"));
                    if (objSms.Address == "")
                        objSms.Address = "Unknown";
                    objSms.Msg = cursor.GetString(cursor.GetColumnIndexOrThrow("body"));
                    objSms.ReadState = cursor.GetString(cursor.GetColumnIndex("read"));
                    objSms.Time = cursor.GetLong(cursor.GetColumnIndexOrThrow("date"));
                    objSms.Thread_id = cursor.GetString(cursor.GetColumnIndexOrThrow("thread_id"));
                    objSms.Type = cursor.GetString(cursor.GetColumnIndexOrThrow("type"));

                    //탐색한 메세지의 Thread_id가 이전과 다르다면 새 대화임.
                    if(objSms.Thread_id != prevThreadId)
                    {
                        objDialogue = new Dialogue();                                                         //대화를 새로 만듬.
                        objDialogue.Contact = ContactManager.Get().getContactIdByAddress(objSms.Address);    //연락처 가져와 저장

                        //카테고리 분류
                        if (objDialogue.Contact != null)                            //연락처에 있으면 대화로 분류
                        {
                            objDialogue.Category = 1;
                            objDialogue.DisplayName = objDialogue.Contact.Name;
                        }
                        else
                        {
                            objDialogue.Category = 2;                              //DEBUG 임시로 2로 설정, 서버와 통신해서 카테고리 분류를 받는다.
                            objDialogue.DisplayName = objSms.Address;
                            if (objSms.Address == "#CMAS#CMASALL")
                                objDialogue.DisplayName = "긴급 재난 문자";
                        }

                        if (objSms.ReadState == "0")                               //읽지 않은 문자면, 대화에 읽지않은 문자가 존재한다고 체크함.
                            objDialogue.IsUnreadExist = true;

                        mDialogueSets[objDialogue.Category].Add(objDialogue);                                                     //카테고리 알맞게 대화 집합에 추가

                        prevThreadId = objSms.Thread_id;
                    }
                    objDialogue.Add(objSms);
                    cursor.MoveToNext();
                }
            }
            // else {
            // throw new RuntimeException("You have no SMS");
            // }
            mActivity.StopManagingCursor(cursor);
            cursor.Close();

            //0번 카테고리 생성, 생성된 카테고리들 정렬
            createAllCategory();
            for(int i = 0; i < TabFrag.CATEGORY_COUNT; i++)
            {
                sortDialogueSet(i);
            }
        }

        public void createAllCategory()
        {
            DialogueSet resultDialogueSet = new DialogueSet();
            resultDialogueSet.Category = (int)TabFrag.CATEGORY.ALL;
            //카테고리 0~7까지 병합
            for (int i = 0; i < TabFrag.CATEGORY_COUNT; i++)
            {
                resultDialogueSet.DialogueList.AddRange(mDialogueSets[i].DialogueList);
            }
            mDialogueSets[(int)TabFrag.CATEGORY.ALL] = resultDialogueSet;
        }

        private void sortDialogueSet(int category)
        {
            if (mDialogueSets[category].Count > 0)
            {
                mDialogueSets[category].DialogueList.Sort(delegate (Dialogue A, Dialogue B)    //각 대화별로, 가장 최신 문자의 날짜별로 정렬
                {
                    if (A[0].Time < B[0].Time) return 1;
                    else if (A[0].Time > B[0].Time) return -1;
                    return 0;
                });
            }
        }

    }
}