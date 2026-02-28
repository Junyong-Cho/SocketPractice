using System.Diagnostics;
using System.Net.Sockets;

namespace StressTest;

static internal class CountReference
{
    static public Stopwatch? watch;
    static public int currentCount = 0;
    static public int MAX_COUNT = 100000;
    static public bool success = false;
    static public int connectFailCount = 0;
    static public int socketErrorCount = 0;
}
