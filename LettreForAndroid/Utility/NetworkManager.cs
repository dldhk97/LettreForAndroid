﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using LettreForAndroid.Class;
using System.Text.RegularExpressions;

namespace LettreForAndroid.Utility
{

    public static class SocketExtensions
    {
        //동기 네트워킹에서 커스텀 타임아웃 지정
        public static void Connect(this Socket socket, EndPoint endpoint, TimeSpan timeout)
        {
            var result = socket.BeginConnect(endpoint, null, null);
            bool success = result.AsyncWaitHandle.WaitOne(timeout, true);
            if (success)
            {
                socket.EndConnect(result);
            }
            else
            {
                socket.Close();
                throw new SocketException(10060);   //connection time out
            }
        }
    }


    //--------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------

    class NetworkManager
    {
        private static NetworkManager _Instance = null;

        public static NetworkManager Get()
        {
            if (_Instance == null)
                _Instance = new NetworkManager();
            return _Instance;
        }
       
        private const string _ServerIP = "202.31.202.153";                            //아이피는 서버에 맞게 설정
        private const int _Port = 10101;                                                //포트 설정            
        private TimeSpan _Timeout = new TimeSpan(0, 0, 1);                            //타임아웃 시간 설정
        private bool _IsConnected = false;
        private Socket _CurrentSocket = null;

        

        private void MakeConnection()
        {
            if (_CurrentSocket != null)
                _CurrentSocket.Close();
            _CurrentSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // (2) 서버에 연결
            try
            {
                // 연결 시도
                //_CurrentSocket.Connect(_ServerIP, _Port);    //타임아웃이 긴 기존의 연결메소드
                var ep = new IPEndPoint(IPAddress.Parse(_ServerIP), _Port);
                _CurrentSocket.Connect(ep, _Timeout);

                // 연결 성공
                _IsConnected = true;
            }
            catch
            {
                // 연결 실패 (연결 도중 오류가 발생함)
                _IsConnected = false;
            }
        }

        public List<string[]> GetLablesFromServer(DialogueSet dialogues)
        {
            List<string[]> toSendData = new List<string[]>();

            foreach (Dialogue objDialogue in dialogues.DialogueList.Values)
            {
                toSendData.AddRange(ConvertToSendData(objDialogue.TextMessageList));
            }

            //모든 메시지가 toSendData 배열에 들어갔음. 이것을 서버로 전송하고, 결과값을 받는다.
            return SendAndReceiveData(toSendData);
        }

        public List<string[]> GetLablesFromServer(Dialogue dialogue)
        {
            List<string[]> toSendData = ConvertToSendData(dialogue.TextMessageList);
            //모든 메시지가 toSendData 배열에 들어갔음. 이것을 서버로 전송하고, 결과값을 받는다.
            return SendAndReceiveData(toSendData);
        }

        public List<string[]> GetLableFromServer(TextMessage message)
        {
            List<string[]> toSendData = ConvertToSendData(new List<TextMessage>() { message });
            //모든 메시지가 toSendData 배열에 들어갔음. 이것을 서버로 전송하고, 결과값을 받는다.
            return SendAndReceiveData(toSendData);
        }

        //데이터를 서버에 보내기 전 전처리과정.
        private List<string[]> ConvertToSendData(List<TextMessage> messageList)
        {
            List<string[]> toSendData = new List<string[]>();

            //메시지 목록을 만들기 위해 Foreach로 메시지 각각 탐색
            foreach (TextMessage objMessage in messageList)
            {
                string ProcessedMsg = Regex.Replace(objMessage.Msg, @"[^가-힣]", " ");       //메시지 내용 중 한글을 제외한 것은 다 공백으로 치환함.
                ProcessedMsg = ProcessedMsg.Trim();                                          //좌우 공백 제거
                //문자가 비어있지 않으면 리스트에 추가
                if (ProcessedMsg.Length > 0)
                    toSendData.Add(new string[] { objMessage.Address, ProcessedMsg });
            }

            return toSendData;
        }

        //데이터를 여러개 보낼 때(어플 -> 서버)
        private List<string[]> SendAndReceiveData(List<string[]> dataList)
        {
            if (dataList.Count <= 0)
                return null;

            if (!_IsConnected)
                MakeConnection();

            List<string[]> receivedData = null;

            if (_IsConnected)
            {
                receivedData = new List<string[]>();

                //타입 전송, 타입은 1이면 데이터 제공, 0이면 데이터 제공 X, 현재 서버측에서 타입1일때 처리를 못하므로 0으로 보냄
                //int type = DataStorageManager.LoadBoolData(Application.Context, "supportMachineLearning", false) ? 1 : 0 ;
                int type = 0;
                byte[] typeByte = IntToByteArray(type, 1);      //일단 0으로 보낸다고 가정함.
                _CurrentSocket.Send(typeByte, SocketFlags.None);

                //데이터 수량 전송
                byte[] amountByte = IntToByteArray(dataList.Count, 2);
                _CurrentSocket.Send(amountByte, SocketFlags.None);

                //실 데이터 전송
                for (int i = 0; i < dataList.Count; i++)
                {
                    //변수 명시
                    string addr = dataList[i][0];
                    string msg = dataList[i][1];

                    //미리 바이트 배열로 변환
                    byte[] addrByte = StringToByteArray(addr, addr.Length);                     //연락처를 바이트로 바꾼 값
                    byte[] addr_lengthByte = IntToByteArray(addrByte.Length, 2);                //연락처를 바이트로 바꾼 값의 길이

                    byte[] msgByte = StringToByteArray(msg, msg.Length);                        //문자내용을 바이트로 바꾼 값
                    byte[] msg_lengthByte = IntToByteArray(msgByte.Length, 2);                  //문자내용을 바이트로 바꾼 값의 길이

                    //디버깅용 출력부분<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                    //System.Diagnostics.Debug.Print("연락처 : \n" + addr+"\n\n연락처 길이 : \n" + addr.Length + "\n\n문자 내용 : \n" + msg + "\n\n문자 길이 : \n" + msg.Length + "\n\n문자내용 바이트 수 : \n" + msgByte.Length + "\n\n바이트 : \n"  );

                    //string debugByte = "";
                    //foreach(byte elem in msgByte)
                    //{
                    //    debugByte += elem.ToString() + " ";
                    //}
                    //System.Diagnostics.Debug.Print(debugByte.ToString() + "\n\n----------------------------");

                    //디버깅용 출력부분<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                    //연락처 길이 전송
                    _CurrentSocket.Send(addr_lengthByte, SocketFlags.None);

                    //연락처 전송
                    _CurrentSocket.Send(addrByte, SocketFlags.None);

                    //메세지 길이 전송
                    _CurrentSocket.Send(msg_lengthByte, SocketFlags.None);

                    //메세지 전송
                    _CurrentSocket.Send(msgByte, SocketFlags.None);
                }

                //---------------------------------------------------------------------------

                //데이터 수량 수신
                byte[] receive_amount_byte = new byte[2];
                _CurrentSocket.Receive(receive_amount_byte, 2, SocketFlags.None);

                //받은 바이트 배열을 string으로 바꾼 뒤 int로 변환
                int receive_amount = ByteToInt(receive_amount_byte);

                for (int i = 0; i < receive_amount; i++)
                {
                    //연락처 길이 수신
                    byte[] receive_addr_length_byte = new byte[2];
                    _CurrentSocket.Receive(receive_addr_length_byte, 2, SocketFlags.None);

                    //받은 바이트를 int로 변환
                    int receive_addr_length = ByteToInt(receive_addr_length_byte);

                    //-------------------------------------------------------

                    //연락처 수신
                    byte[] receive_addr_byte = new byte[receive_addr_length];
                    _CurrentSocket.Receive(receive_addr_byte, receive_addr_length, SocketFlags.None);

                    //받은 바이트를 string으로 변환
                    string receive_addr_str = Encoding.UTF8.GetString(receive_addr_byte);

                    //-------------------------------------------------------

                    //레이블 수신
                    int[] receive_lables = new int[Dialogue.Lable_COUNT];
                    for (int j = 0; j < Dialogue.Lable_COUNT; j++)
                    {
                        byte[] receive_lable_byte = new byte[2];
                        _CurrentSocket.Receive(receive_lable_byte, 2, SocketFlags.None);

                        //받은 바이트를 int로 변환
                        receive_lables[j] = ByteToInt(receive_lable_byte);
                    }

                    //-------------------------------------------------------

                    receivedData.Add(new string[] {
                        receive_addr_str,
                        receive_lables[0].ToString(),
                        receive_lables[1].ToString(),
                        receive_lables[2].ToString(),
                        receive_lables[3].ToString(),
                        receive_lables[4].ToString(),
                        receive_lables[5].ToString(),
                        receive_lables[6].ToString(),
                    });
                }
            }
            // (4) 소켓 닫기
            _CurrentSocket.Close();
            _IsConnected = false;

            return receivedData;
        }

        //str을 고정길이 length만큼 바이트 배열로 변환함.
        static byte[] StringToByteArray(string str, int length)
        {
            return Encoding.UTF8.GetBytes(str.PadRight(length, ' '));       //빈공간을 공백으로 채운다
        }

        static byte[] IntToByteArray(int integer, int length)
        {
            byte[] intBytes = BitConverter.GetBytes(integer);
            byte[] result = new byte[length];
            for (int i = 0; i < length; i++)
                result[i] = intBytes[i];
            return result;
        }

        static public int ByteToInt(byte[] iByte)
        {
            byte[] intByte = new byte[4];
            for (int i = 0; i < iByte.Length; i++)
                intByte[i] = iByte[i];
            int result = BitConverter.ToInt32(intByte);
            return result;
        }
    }
}
