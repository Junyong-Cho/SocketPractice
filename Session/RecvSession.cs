using System.Net.Sockets;

partial class Session
{
    void RegisterRecv()
    {
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
            Console.WriteLine("RegisterSend Failed");
            Console.WriteLine(e);
        }
    }

    void OnRecvComplete(object? sender, SocketAsyncEventArgs recvArgs)
    {

        lock (_lock)
        {
            if (_disconnected == 1)
                return;
        }

        int byteCount = recvArgs.BytesTransferred;

        if (recvArgs.SocketError != SocketError.Success || byteCount == 0 || _recvBuffer.OnWrite(byteCount) == false)
        {
            Disconnect();
            return;
        }

        int processCount = OnRecv(_recvBuffer.ReadSegment);

        if (processCount == 0 || _recvBuffer.OnRead(processCount) == false)
        {
            Disconnect();
            return;
        }

        RegisterRecv();
    }
}