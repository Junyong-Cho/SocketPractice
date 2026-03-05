using ServerCore.Sessions;
using System.Net;
using System.Net.Sockets;

namespace Server;

internal class ClientListener
{
    Socket _listener;

    int _argsCount;

    public ClientListener(EndPoint endPoint)
    {
        _listener = new(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        _listener.Bind(endPoint);
        _argsCount = 0;
    }

    public void Start(int argsCount = 10, int listenCount = 100)
    {
        _listener.Listen(listenCount);

        _argsCount = argsCount;

        while (argsCount-- > 0)
        {
            SocketAsyncEventArgs accpArgs = new();

            accpArgs.Completed += OnAccpComplete;

            RegisterAccp(accpArgs);
        }

        Console.WriteLine($"Complete Reigst {_argsCount}");
    }

    void RegisterAccp(SocketAsyncEventArgs accpArgs)
    {
        while (true)
        {
            accpArgs.AcceptSocket = null;
        
            try
            {
                bool pending = _listener.AcceptAsync(accpArgs);

                if (pending == true)
                    return;

                OnAccpComplete(null, accpArgs);
            }
            catch(ObjectDisposedException e)
            {
                Console.WriteLine("Server Closed");
                Console.WriteLine(e);
                Environment.Exit(0);
            }
            catch(Exception e)
            {
                Console.WriteLine($"[FATAL] Listener Socket Broken: {e}");
                Environment.Exit(0);
            }
        }
    }

    void OnAccpComplete(object? sender, SocketAsyncEventArgs accpArgs)
    {
        if(accpArgs.SocketError == SocketError.Success)
            SessionPool<ServerSession>.Rent()!.Start(accpArgs.AcceptSocket!);

        if (sender == null)
            return;

        RegisterAccp(accpArgs);
    }
}
