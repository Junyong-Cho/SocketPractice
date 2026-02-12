using System.Net;
using System.Net.Sockets;

internal class ClientListener
{
    Socket _listenSocket;
    SocketAsyncEventArgs _acceptArgs;

    Func<Session> _sessionFactory;

    public void Init(EndPoint endPoint, Func<Session> sessionFactory)
    {
        _listenSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        _sessionFactory = sessionFactory;

        _listenSocket.Bind(endPoint);
        _listenSocket.Listen(10);

        _acceptArgs.Completed += OnAcceptCompleted;

        
    }

    void RegisterAccept()
    {
        try
        {
            bool accept = _listenSocket.AcceptAsync(_acceptArgs);

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
