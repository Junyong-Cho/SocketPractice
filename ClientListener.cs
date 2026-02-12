using System.Net.Sockets;

internal class ClientListener
{
    Socket _socket;
    SocketAsyncEventArgs _acceptArgs;

    Func<Session> _sessionFactory;

    public ClientListener(Socket socket)
    {
        _socket = socket;
        _acceptArgs = new();

        _acceptArgs.Completed += OnAcceptCompleted;

        RegisterAccept();
    }

    void RegisterAccept()
    {
        try
        {
            bool accept = _socket.AcceptAsync(_acceptArgs);

            if (accept == false)
            {
                OnAcceptCompleted(null, _acceptArgs);
            }
        }
        catch(Exception e)
        {
            Console.WriteLine(e);
        }
    }

    void OnAcceptCompleted(object? obj, SocketAsyncEventArgs args)
    {
        if (args.SocketError == SocketError.Success)
        {
            Socket client = args.AcceptSocket!;

            args.AcceptSocket = null;
            
            // client 처리
        }



        RegisterAccept();
    }
}
