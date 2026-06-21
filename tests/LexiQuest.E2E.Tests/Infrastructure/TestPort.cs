using System.Net;
using System.Net.Sockets;

namespace LexiQuest.E2E.Tests.Infrastructure;

internal static class TestPort
{
    public static int GetFreeTcpPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}
