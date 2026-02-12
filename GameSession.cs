using System.Buffers;
using System.Net;
using System.Text;

internal class GameSession : Session
{
    protected override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"Connected: {endPoint.ToString()}");

        byte[] sendBuff = ArrayPool<byte>.Shared.Rent(1 << 16);

        int len = Encoding.UTF8.GetBytes("Hello world", sendBuff);

        Send(new ArraySegment<byte>(sendBuff, 0, len));
    }

    protected override void OnDisconnected(EndPoint endPoint)
    {
        Console.WriteLine($"Disconnected: {endPoint.ToString()}");
    }

    protected override int OnRecv(ArraySegment<byte> segment)
    {
        string data = Encoding.UTF8.GetString(segment.Array!, segment.Offset, segment.Count);

        Console.WriteLine($"[From]: {data}");

        Send(segment);

        return segment.Count;
    }

    protected override void OnSend(int numOfBytes)
    {
        
    }
}
