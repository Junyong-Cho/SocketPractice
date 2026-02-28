using StressTest;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

string hostName = Dns.GetHostName();
var ip = Dns.GetHostAddresses(hostName).First(addr => addr.AddressFamily == AddressFamily.InterNetwork && addr != IPAddress.Loopback);
int port = 8080;

var endPoint = new IPEndPoint(ip, port);

SessionHandler._endPoint = endPoint;

CountReference.watch = Stopwatch.StartNew();
SessionHandler.Start();

Console.ReadLine();