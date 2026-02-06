using System.Net;
using System.Net.Sockets;

string serverIp = "127.0.0.1";
int port = 8080;

Socket server = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
server.Bind(new IPEndPoint(IPAddress.Parse(serverIp), port));

server.Listen(100);

ClientListener listener = new(server);

while (true)
{
    string ord = Console.ReadLine()!;

    if (ord == "exit")
    {
        Console.WriteLine("서버를 종료합니다.");
        return;
    }
}