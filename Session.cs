using System.Net;
using System.Net.Sockets;

namespace StudyProject;

public abstract class Session
{
    Socket _socket;
    int _disconnected = 0;

    // [수신용]
    RecvBuffer _recvBuffer = new RecvBuffer(65535);

    // [전송용]
    object _lock = new object();
    Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
    List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
    SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
    SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

    public abstract void OnConnected(EndPoint endPoint);
    public abstract int OnRecv(ArraySegment<byte> buffer);
    public abstract void OnSend(int numOfBytes);
    public abstract void OnDisconnected(EndPoint endPoint);

    public void Start(Socket socket)
    {
        _socket = socket;

        // 1. 수신용 이벤트 등록
        _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);

        // 2. 전송용 이벤트 등록
        _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

        // 3. 수신 시작
        RegisterRecv();
    }

    // ---------------------------------------------------------
    // [전송 파트] - Gathering Send (뭉쳐 보내기)
    // ---------------------------------------------------------

    // 외부에서 호출: 패킷을 큐에 넣기만 함 (빠름)
    public void Send(ArraySegment<byte> sendBuff)
    {
        lock (_lock)
        {
            _sendQueue.Enqueue(sendBuff);

            // 현재 전송 중인 패킷이 없으면, 내가 총대 메고 전송 시작
            if (_pendingList.Count == 0)
            {
                RegisterSend();
            }
        }
    }

    public void Send(List<ArraySegment<byte>> sendBuffList)
    {
        if (sendBuffList.Count == 0)
            return;

        lock (_lock)
        {
            foreach (ArraySegment<byte> buff in sendBuffList)
                _sendQueue.Enqueue(buff);

            if (_pendingList.Count == 0)
            {
                RegisterSend();
            }
        }
    }

    // 내부 호출: 큐에 쌓인 걸 리스트로 옮겨서 소켓에 넘김
    void RegisterSend()
    {
        if (_disconnected == 1) return;

        // 큐 -> 리스트로 모두 이동 (Gathering)
        while (_sendQueue.Count > 0)
        {
            ArraySegment<byte> buff = _sendQueue.Dequeue();
            _pendingList.Add(buff);
        }

        // ★핵심: 리스트째로 SocketArgs에 전달
        _sendArgs.BufferList = _pendingList;

        try
        {
            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
            {
                OnSendCompleted(null, _sendArgs);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"RegisterSend Failed {e}");
        }
    }

    void OnSendCompleted(object sender, SocketAsyncEventArgs args)
    {
        lock (_lock)
        {
            if (args.SocketError == SocketError.Success && args.BytesTransferred > 0)
            {
                try
                {
                    // 1. 참조 해제 (중요: 이걸 안 하면 리스트가 잠김)
                    _sendArgs.BufferList = null;

                    // 2. 리스트 비우기
                    _pendingList.Clear();

                    // 3. 알림 (필요하다면 구현)
                    OnSend(_sendArgs.BytesTransferred);

                    // 4. 내가 보내는 동안 또 누가 줄을 섰나?
                    if (_sendQueue.Count > 0)
                    {
                        RegisterSend(); // 쉬지 않고 다음 배송 출발
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnSendCompleted Failed {e}");
                }
            }
            else
            {
                Disconnect();
            }
        }
    }

    // ---------------------------------------------------------
    // [수신 파트] - 기존 코드와 동일
    // ---------------------------------------------------------

    void RegisterRecv()
    {
        if (_disconnected == 1) return;

        _recvBuffer.Clean(); // 커서 정리
        ArraySegment<byte> segment = _recvBuffer.WriteSegment;
        _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

        try
        {
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (pending == false)
            {
                OnRecvCompleted(null, _recvArgs);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"RegisterRecv Failed {e}");
        }
    }

    void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
    {
        if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
        {
            try
            {
                // 1. 버퍼 커서 이동
                if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                {
                    Disconnect();
                    return;
                }

                // 2. 패킷 조립 및 처리 (OnRecv 호출)
                // 여기서 패킷이 완성되지 않았다면 처리된 바이트(processLen)는 0일 수 있음
                int processLen = OnRecv(_recvBuffer.ReadSegment);
                if (processLen < 0)
                {
                    Disconnect();
                    return;
                }

                // 3. 처리한 만큼 읽기 커서 이동
                if (_recvBuffer.OnRead(processLen) == false)
                {
                    Disconnect();
                    return;
                }

                // 4. 다시 수신 대기
                RegisterRecv();
            }
            catch (Exception e)
            {
                Console.WriteLine($"OnRecvCompleted Failed {e}");
            }
        }
        else
        {
            Disconnect();
        }
    }

    // ---------------------------------------------------------
    // [종료 파트]
    // ---------------------------------------------------------
    public void Disconnect()
    {
        if (Interlocked.Exchange(ref _disconnected, 1) == 1)
            return;

        OnDisconnected(_socket.RemoteEndPoint);
        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
        Clear();
    }

    void Clear()
    {
        lock (_lock)
        {
            _sendQueue.Clear();
            _pendingList.Clear();
        }
    }
}