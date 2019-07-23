using System;
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


namespace LettreForAndroid.Utility
{

    public static class SocketExtensions
    {
        //동기 네트워킹에서 커스텀 타임아웃 지정
        public static void connect(this Socket socket, EndPoint endpoint, TimeSpan timeout)
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


    //Singleton
    class NetworkManager
    {
        private static NetworkManager mInstance = null;

        
        public static NetworkManager Get()
        {
            if (mInstance == null)
                mInstance = new NetworkManager();
            //mAsyncReceiveHandler = new AsyncCallback(mInstance.handleDataReceive);
            //mAsyncSendHandler = new AsyncCallback(mInstance.handleDataSend);
            return mInstance;
        }

        private const string mServerIP = "192.168.0.5";                                  //아이피는 서버에 맞게 설정하시오.
        //private const string mServerIP = "59.151.215.129";
        private const int mPort = 10101;
        private const int mMaxBuffer = 1024;
        private TimeSpan mTimeout = new TimeSpan(0,0,2);

        private bool isConnected = false;

        private Socket mCurrentSocket = null;

        /// <summary>
        /// 여기부터는 동기 메소드
        /// </summary>

        public void makeConnection()
        {
            if (mCurrentSocket != null)
                mCurrentSocket.Close();
            mCurrentSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //타임아웃 설정
            //mCurrentSocket.SendTimeout = 5000;
            //mCurrentSocket.ReceiveTimeout = 5000;
            //mCurrentSocket.LingerState = new LingerOption(true, 0);

            // (2) 서버에 연결
            try
            {
                // 연결 시도
                //m_CurrentSocket.Connect(mServerIP, mPort);    //타임아웃이 긴 기존의 연결메소드
                var ep = new IPEndPoint(IPAddress.Parse(mServerIP), mPort);
                mCurrentSocket.connect(ep, mTimeout);

                // 연결 성공
                isConnected = true;
            }
            catch
            {
                // 연결 실패 (연결 도중 오류가 발생함)
                isConnected = false;
            }
        }

        //데이터를 여러개 보낼 때(어플 -> 서버)
        public void sendAndReceiveData(List<string[]> dataList, int type)
        {
            if (!isConnected)
                makeConnection();

            if (isConnected)
            {
                //타입 전송 (기본 0이라 상정하고 아래 코드 작성함)
                byte[] typeByte = stringToByteArray(type.ToString(), 1);
                mCurrentSocket.Send(typeByte, SocketFlags.None);
                
                //데이터 수량 전송
                byte[] amountByte = stringToByteArray(dataList.Count.ToString(), 2);
                mCurrentSocket.Send(amountByte, SocketFlags.None);

                //실 데이터 전송
                for (int i = 0; i < dataList.Count; i++)
                {
                    //변수 명시
                    string addr = dataList[i][0];
                    string msg = dataList[i][1];

                    //미리 바이트 배열로 변환
                    byte[] addrByte = stringToByteArray(addr, addr.Length);                     //연락처를 바이트로 바꾼 값
                    byte[] addr_lengthByte = stringToByteArray(addrByte.Length.ToString(), 2);  //연락처를 바이트로 바꾼 값의 길이

                    byte[] msgByte = stringToByteArray(msg, msg.Length);                        //문자내용을 바이트로 바꾼 값
                    byte[] msg_lengthByte = stringToByteArray(msgByte.Length.ToString(), 2);    //문자내용을 바이트로 바꾼 값의 길이

                    //연락처 길이 전송
                    mCurrentSocket.Send(addr_lengthByte, SocketFlags.None);

                    //연락처 전송
                    mCurrentSocket.Send(addrByte, SocketFlags.None);

                    //메세지 길이 전송
                    mCurrentSocket.Send(msg_lengthByte, SocketFlags.None);

                    //메세지 전송
                    mCurrentSocket.Send(msgByte, SocketFlags.None);
                }

                //---------------------------------------------------------------------------

                //데이터 수량 수신
                byte[] receive_amount_byte = new byte[2];
                mCurrentSocket.Receive(receive_amount_byte, 2, SocketFlags.None);

                //받은 바이트 배열을 string으로 바꾼 뒤 int로 변환
                string receive_amount_str = Encoding.UTF8.GetString(receive_amount_byte);
                int receive_amount = Convert.ToInt32(receive_amount_str);

                for(int i = 0; i < receive_amount; i++)
                {
                    //레이블 수신
                    byte[] receive_lable_byte = new byte[2];
                    mCurrentSocket.Receive(receive_lable_byte, 2, SocketFlags.None);

                    //받은 바이트를 int로 변환
                    string receive_lable_str = Encoding.UTF8.GetString(receive_lable_byte);
                    int receive_lable = Convert.ToInt32(receive_lable_str);

                    //-------------------------------------------------------

                    //연락처 길이 수신
                    byte[] receive_addr_length_byte = new byte[2];
                    mCurrentSocket.Receive(receive_addr_length_byte, 2, SocketFlags.None);

                    //받은 바이트를 int로 변환
                    string receive_addr_length_str = Encoding.UTF8.GetString(receive_addr_length_byte);
                    int receive_addr_length = Convert.ToInt32(receive_addr_length_str);

                    //-------------------------------------------------------

                    //연락처 수신
                    byte[] receive_addr_byte = new byte[receive_addr_length];
                    mCurrentSocket.Receive(receive_addr_byte, receive_addr_length, SocketFlags.None);

                    //받은 바이트를 string으로 변환
                    string receive_addr_str = Encoding.UTF8.GetString(receive_addr_byte);

                    //-------------------------------------------------------

                    //DEBUG : 출력 창에 표시함.
                    Console.WriteLine("---------------------------[" + (i + 1) + "] 번째 데이터----------------------");
                    Console.WriteLine("레이블 : " + receive_lable_str);
                    Console.WriteLine("연락처 길이 : " + receive_addr_length_str);
                    Console.WriteLine("연락처 : " + receive_addr_str);
                }
                Console.WriteLine("수신 완료!!!");

            }

            // (4) 소켓 닫기
            mCurrentSocket.Close();
        }

        //str을 고정길이 length만큼 바이트 배열로 변환함.
        static byte[] stringToByteArray(string str, int length)
        {
            return Encoding.UTF8.GetBytes(str.PadRight(length, ' '));       //빈공간을 공백으로 채운다
        }


















        //동기메소드인데 연습용임.
        //public void receiveDataNonAsync()
        //{
        //    if (!isConnected)
        //        makeConnection();

        //    if (isConnected)
        //    {

        //        byte[] receivedBuffer = new byte[mMaxBuffer];

        //        //서버에서 데이터 수신
        //        int numberOfByte = m_CurrentSocket.Receive(receivedBuffer);

        //        Console.WriteLine("연결 성공!\n");
        //        string receivedData = Encoding.UTF8.GetString(receivedBuffer, 0, numberOfByte);
        //        Console.WriteLine(receivedData);
        //    }
        //    else
        //    {
        //        Console.WriteLine("연결 실패!");
        //        isConnected = false;
        //    }
        //    m_CurrentSocket.Close();
        //}

        //public void sendDataNonAsync(string data)
        //{
        //    if (!isConnected)
        //        makeConnection();

        //    if (isConnected)
        //    {
        //        byte[] dataBuffer = Encoding.UTF8.GetBytes(data);

        //        // (3) 서버에 데이터 전송
        //        m_CurrentSocket.Send(dataBuffer, SocketFlags.None);

        //    }

        //    // (4) 소켓 닫기
        //    m_CurrentSocket.Close();
        //}


























        ///// <summary>
        ///// 여기부터는 비동기 메소드임. 만약 비동기로 짜야될 때 참고할 것
        ///// 출처 https://slaner.tistory.com/52
        ///// </summary>

        //비동기 작업에 사용될 대리자 선언
        //private static AsyncCallback mAsyncReceiveHandler;
        //private static AsyncCallback mAsyncSendHandler;



        //public class AsyncObject
        //{
        //    public byte[] buffer;
        //    public Socket workingSocket;
        //    public AsyncObject(int bufferSize)
        //    {
        //        buffer = new byte[bufferSize];
        //    }
        //}

        //public void ConnectToServer()
        //{
        //    // TCP 통신을 위한 소켓을 생성합니다.
        //    m_CurrentSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //    try
        //    {
        //        // 연결 시도
        //        m_CurrentSocket.Connect(mServerIP, mPort);

        //        // 연결 성공
        //        isConnected = true;
        //    }
        //    catch
        //    {
        //        // 연결 실패 (연결 도중 오류가 발생함)
        //        isConnected = false;
        //    }

        //    if (isConnected)
        //    {

        //        // 4096 바이트의 크기를 갖는 바이트 배열을 가진 AsyncObject 클래스 생성
        //        AsyncObject ao = new AsyncObject(mMaxBuffer);

        //        // 작업 중인 소켓을 저장하기 위해 sockClient 할당
        //        ao.workingSocket = m_CurrentSocket;

        //        // 비동기적으로 들어오는 자료를 수신하기 위해 BeginReceive 메서드 사용!
        //        m_CurrentSocket.BeginReceive(ao.buffer, 0, ao.buffer.Length, SocketFlags.None, mAsyncReceiveHandler, ao);

        //        Console.WriteLine("연결 성공!");

        //    }
        //    else
        //    {

        //        Console.WriteLine("연결 실패!");
        //        isConnected = false;

        //    }
        //}

        //public void sendData(string message)
        //{
        //    // 추가 정보를 넘기기 위한 변수 선언
        //    // 크기를 설정하는게 의미가 없습니다.
        //    // 왜냐하면 바로 밑의 코드에서 문자열을 유니코드 형으로 변환한 바이트 배열을 반환하기 때문에
        //    // 최소한의 크기르 배열을 초기화합니다.
        //    AsyncObject ao = new AsyncObject(1);

        //    // 문자열을 바이트 배열으로 변환
        //    ao.buffer = Encoding.UTF8.GetBytes(message);

        //    ao.workingSocket = m_CurrentSocket;

        //    // 전송 시작!
        //    try
        //    {
        //        m_CurrentSocket.BeginSend(ao.buffer, 0, ao.buffer.Length, SocketFlags.None, mAsyncSendHandler, ao);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("전송 중 오류 발생!\n메세지: {0}", ex.Message);
        //    }

        //}

        //public void closeSocket()
        //{
        //    m_CurrentSocket.Close();
        //}

        //private void handleDataReceive(IAsyncResult ar)
        //{
        //    // 넘겨진 추가 정보를 가져옵니다.
        //    // AsyncState 속성의 자료형은 Object 형식이기 때문에 형 변환이 필요합니다~!
        //    AsyncObject ao = (AsyncObject)ar.AsyncState;

        //    // 자료를 수신하고, 수신받은 바이트를 가져옵니다.
        //    Int32 recvBytes = ao.workingSocket.EndReceive(ar);

        //    // 수신받은 자료의 크기가 1 이상일 때에만 자료 처리
        //    if (recvBytes > 0)
        //    {
        //        /*
        //            여기에 자료를 처리하는 작업을 하시면 됩니다.
        //        */
        //        string receivedData = Encoding.UTF8.GetString(ao.buffer, 0, recvBytes);
        //        Console.WriteLine("받은 데이터 : " + receivedData);
        //    }

        //    // 자료 처리가 끝났으면~
        //    // 이제 다시 데이터를 수신받기 위해서 수신 대기를 해야 합니다.
        //    // Begin~~ 메서드를 이용해 비동기적으로 작업을 대기했다면
        //    // 반드시 대리자 함수에서 End~~ 메서드를 이용해 비동기 작업이 끝났다고 알려줘야 합니다!
        //    ao.workingSocket.BeginReceive(ao.buffer, 0, ao.buffer.Length, SocketFlags.None, mAsyncReceiveHandler, ao);
        //}

        //private void handleDataSend(IAsyncResult ar)
        //{

        //    // 넘겨진 추가 정보를 가져옵니다.
        //    AsyncObject ao = (AsyncObject)ar.AsyncState;

        //    // 보낸 바이트 수를 저장할 변수 선언
        //    Int32 sentBytes;

        //    try
        //    {
        //        // 자료를 전송하고, 전송한 바이트를 가져옵니다.
        //        sentBytes = ao.workingSocket.EndSend(ar);
        //    }
        //    catch (Exception ex)
        //    {
        //        // 예외가 발생하면 예외 정보 출력 후 함수를 종료한다.
        //        Console.WriteLine("자료 송신 도중 오류 발생! 메세지: {0}", ex.Message);
        //        return;
        //    }

        //    if (sentBytes > 0)
        //    {
        //        // 여기도 마찬가지로 보낸 바이트 수 만큼 배열 선언 후 복사한다.
        //        Byte[] msgByte = new Byte[sentBytes];
        //        Array.Copy(ao.buffer, msgByte, sentBytes);

        //        Console.WriteLine("메세지 보냄: {0}", Encoding.Unicode.GetString(msgByte));
        //    }
        //}

    }
}