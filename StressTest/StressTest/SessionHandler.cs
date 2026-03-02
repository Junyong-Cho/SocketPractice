using ServerCore.Sessions;
using System.Net;
using System.Net.Sockets;

namespace StressTest;

static class SessionHandler
{
    public static EndPoint _endPoint;

    public static void Start()
    {
        for (int i = 0; i < 1000; i++)
            AddConnect();
        Console.WriteLine("Started");
    }

    public static void AddConnect()
    {
        if(Interlocked.Increment(ref CountReference.currentCount) > CountReference.MAX_COUNT)
        {
            if (Interlocked.Exchange(ref CountReference.success, true) == true)
                return;
            CountReference.watch!.Stop();
            StreamWriter writer = new(Console.OpenStandardOutput());
            writer.WriteLine($"Time: {CountReference.watch.ElapsedMilliseconds}");
            writer.WriteLine($"ConnectFailCount : {CountReference.connectFailCount}");
            writer.WriteLine($"SocketErrorCount : {CountReference.socketErrorCount}");
            writer.WriteLine("Complete");
            writer.Flush();
        }

        Socket socket = new(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        SocketAsyncEventArgs args = new();

        args.RemoteEndPoint = _endPoint;
        args.Completed += OnConnectComplete;

        try
        {
            bool pending = socket.ConnectAsync(args);

            if (pending == false)
                OnConnectComplete(null, args);
        }
        catch
        {
            Interlocked.Increment(ref CountReference.connectFailCount);
            AddConnect();
        }
    }

    static void OnConnectComplete(object? sender, SocketAsyncEventArgs args)
    {
        if (args.SocketError != SocketError.Success)
        {
            Console.WriteLine(args.SocketError);
            Interlocked.Increment(ref CountReference.socketErrorCount);
            return;
        }

        var session = SessionPool<TestSession>.Rent();

        session!.Start(args.ConnectSocket!);
    }
}
