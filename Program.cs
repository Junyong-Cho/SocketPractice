using System.Net;
using System.Net.Http.Headers;

string serverIp = "127.0.0.1";
int port = 8080;

ClientListener listener = new();

listener.Init(new IPEndPoint(IPAddress.Parse(serverIp), port), () => new GameSession());

while (true)
{
    string ord = Console.ReadLine()!;

    if (ord == "exit")
    {
        Console.WriteLine("서버를 종료합니다.");
        return;
    }
}