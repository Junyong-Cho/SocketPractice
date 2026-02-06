using System.Net;
using System.Net.Sockets;

abstract partial class Session
{
    object _lock = new();
    int _disconnected = 0;

    Socket? _socket;

    Queue<ArraySegment<byte>> _sendQueue = new();
    List<ArraySegment<byte>> _pendingList = new();

    RecvBuffer _recvBuffer = new();

    SocketAsyncEventArgs _recvArgs = new();
    SocketAsyncEventArgs _sendArgs = new();

    protected abstract int OnRecv(ArraySegment<byte> segment);
    protected abstract void OnSend(int numOfBytes);
    protected abstract void OnDisconnected(EndPoint endPoint);
    protected abstract void OnConnected(EndPoint endPoint);

    public void Start(Socket socket)
    {
        _socket = socket;

        _sendArgs.Completed += OnSendComplete;
        _recvArgs.Completed += OnRecvComplete;

        RegisterRecv();
    }

    void Disconnect()
    {
        if (Interlocked.Exchange(ref _disconnected, 1) == 1)
            return;

        _socket!.Shutdown(SocketShutdown.Both);
        _socket = null;

        lock (_lock)
        {
            _sendQueue.Clear();
            _pendingList.Clear();
        }
    }
}
