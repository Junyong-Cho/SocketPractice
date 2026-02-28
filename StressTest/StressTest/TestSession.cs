using ServerCore.Sessions;

namespace StressTest;

internal class TestSession : Session
{
    protected override void OnConnect()
    {
    }

    protected override void OnDisconnect()
    {
        SessionPool<TestSession>.Return(this);
        SessionHandler.AddConnect();
    }

    protected override int OnRecv(ArraySegment<byte> segment)
    {
        Disconnect();
        return segment.Count;
    }

    protected override void OnSend(int numOfBytes)
    {
    }
}
