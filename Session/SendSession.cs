using System.Net.Sockets;

partial class Session
{

    public void Send(ArraySegment<byte> buffer)
    {
        lock (_lock)
        {
            _sendQueue.Enqueue(buffer);

            if (_pendingList.Count == 0)
                RegisterSend();
        }
    }

    public void Send(IList<ArraySegment<byte>> buffers)
    {
        lock (_lock)
        {
            foreach (var buffer in buffers)
                _sendQueue.Enqueue(buffer);

            if (_pendingList.Count == 0)
                RegisterSend();
        }
    }

    void RegisterSend()
    {
        while (_sendQueue.Count > 0)
            _pendingList.Add(_sendQueue.Dequeue());

        _sendArgs.BufferList = _pendingList;

        try
        {
            bool pending = _socket!.SendAsync(_sendArgs);

            if (pending == false)
                OnSendComplete(null, _sendArgs);
        }
        catch(Exception e)
        {
            Console.WriteLine("RegisterSend Failed");
            Console.WriteLine(e);
            Disconnect();
        }
    }

    void OnSendComplete(object? sender, SocketAsyncEventArgs sendArgs)
    {
        lock (_lock)
        {
            if (_disconnected == 1)
                return;

            if(sendArgs.SocketError != SocketError.Success || sendArgs.BytesTransferred == 0)
            {
                Disconnect();
                return;
            }

            OnSend(sendArgs.BytesTransferred);

            sendArgs.BufferList = null;

            _pendingList.Clear();

            if (_sendQueue.Count > 0)
                RegisterSend();
        }
    }
}
