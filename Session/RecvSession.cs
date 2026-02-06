using System.Net.Sockets;

partial class Session
{
    void RegisterRecv()
    {
        if (_recvBuffer.FreeSize < 1024)
            _recvBuffer.Clean();

        var segment = _recvBuffer.WriteSegment;

        _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

        try
        {
            bool recving = _socket!.ReceiveAsync(_recvArgs);

            if (recving == false)
                OnRecvComplete(null, _recvArgs);
        }
        catch(Exception e)
        {
            Console.WriteLine("RegisterRecv Failed");
            Console.WriteLine(e);
            Disconnect();
        }
    }

    void OnRecvComplete(object? sender, SocketAsyncEventArgs recvArgs)
    {
        if (_disconnected == 1)
            return;

        int byteCount = recvArgs.BytesTransferred;

        if (recvArgs.SocketError != SocketError.Success || byteCount == 0 || _recvBuffer.OnWrite(byteCount) == false)
        {
            Disconnect();
            return;
        }

        int processCount = OnRecv(_recvBuffer.ReadSegment);

        if (processCount < 0 || _recvBuffer.OnRead(processCount) == false)
        {
            Disconnect();
            return;
        }

        RegisterRecv();
    }
}