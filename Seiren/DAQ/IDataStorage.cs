using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.DAQ
{
    internal interface IDataStorage: IDisposable
    {
        public void InitializeTimestamp(System.DateTime date, uint start);
        public void WriteRecord(uint time,
                                ReadOnlySpan<byte> diagdata, ReadOnlySpan<byte> txbitdata, ReadOnlySpan<byte> txblkdata,
                                ReadOnlySpan<byte> ctrldata, ReadOnlySpan<byte> rxbitdata, ReadOnlySpan<byte> rxblkdata);
    }
}
