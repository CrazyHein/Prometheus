using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tirasweel.Storage
{
    interface IDataRetrieval : IDisposable
    {
        public (long counts, DateTime start, DateTime end) Summary(CancellationToken cancel);
        public IEnumerable<byte[]> Latest(int milliseconds, CancellationToken cancel);
        public IEnumerable<byte[]> Head(int counts, CancellationToken cancel);
        public IEnumerable<byte[]> Tail(int counts, CancellationToken cancel);
    }
}
