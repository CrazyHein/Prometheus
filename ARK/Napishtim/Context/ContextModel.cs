using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim
{
    public class ContextModel
    {
        public static IReadOnlyDictionary<uint, ProcessData>? Tags { get; set; }

        protected static Regex _PROCESS_DATA_PATTERN = new Regex("@0[xX][0-9a-fA-F]{1,8}", RegexOptions.Compiled);
        public static string PROCESS_DATA_INDEX_TO_TAG(string input)
        {
            return _PROCESS_DATA_PATTERN.Replace(input, new MatchEvaluator(ProcessDataIndexToTag));
        }

        public static string ProcessDataIndexToTag(Match match)
        {
            string x = match.ToString();
            uint id = Convert.ToUInt32(x.Substring(1), 16);
            if (ContextModel.Tags?.ContainsKey(id) == true)
                return $"({ContextModel.Tags[id].ProcessObject.Variable.Name})";
            else
                return x;
        }
    }
}
