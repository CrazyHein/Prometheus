using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Utility.MXRL
{
    public enum ErrorClassification : ushort
    {
        linuxrt 				= 0x0001,
	    posix 					= 0x0002,
	    crt						= 0x0004,
	    mitsubishi				= 0x0008,
	    orbment 				= 0x0010,
	    dlink					= 0x0020,
	    ilink					= 0x0040,
	    pugixml					= 0x0080,
	    user					= 0x0100,
	    others					= 0x0200
    }
    public record Log(DateTime DateTime, UInt16 Cls, UInt32 Error, string Source, string Content);
    class EventLog
    {
        private static Regex __PATTERN;
        private List<Log> __logs = new List<Log>();

        //public IEnumerable<Log> Records { get { return __logs.OrderByDescending(i => i.DateTime); }}
        public IEnumerable<Log> Records { get { return (__logs as IEnumerable<Log>).Reverse(); } }

        static EventLog()
        {
            __PATTERN = new Regex(@"^\s*\[([0-9.:\- ]+)\]\s*\[([0-9,a-f,A-F]{4})\|([0-9,a-f,A-F]{8})\]\s*(.+)\s*->\s*(.+)$", RegexOptions.Compiled);
        }

        public void Append(string logFileContent)
        {
            System.IO.StringReader sr = new(logFileContent);

            string line;
            while ((line = sr.ReadLine()) != null)
            {
                var match = __PATTERN.Match(line);
                if (match.Success)
                {
                    Log log = new Log(DateTime.Parse(match.Groups[1].Value.Trim(), CultureInfo.InvariantCulture), Convert.ToUInt16(match.Groups[2].Value, 16), Convert.ToUInt32(match.Groups[3].Value, 16),
                                     match.Groups[4].Value.Trim(), match.Groups[5].Value.Trim());
                    __logs.Add(log);
                }
            }
        }


    }
}
