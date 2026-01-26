using System.Net;
using System.Net.Sockets;

string serverIp = "127.0.0.1";
int port = 8080;

Socket server = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
server.Bind(new IPEndPoint(IPAddress.Parse(serverIp), port));

server.Listen(100);

Console.WriteLine($"서버가 {server.LocalEndPoint?.ToString()}에서 시작되었습니다.");

while (true)
{
    var client = await server.AcceptAsync();

    if(client.RemoteEndPoint is IPEndPoint)
    {
        Console.WriteLine($"Client Accpeted at {client.RemoteEndPoint.ToString()}");
    }

    _ = HandleClient(client);
}

async Task HandleClient(Socket client)
{
    await Task.Delay(1000);
}

