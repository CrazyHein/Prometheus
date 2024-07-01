using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.IOUtility
{
    public interface SocketInterface : IDisposable
    {
        int Send(ReadOnlySpan<byte> buffer, SocketFlags socketFlags = SocketFlags.None);
        int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags = SocketFlags.None);
        string Name();
    }
}
