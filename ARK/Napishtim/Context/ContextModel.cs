using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ExceptionHandling;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim
{
    public class ContextModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void _notify_property_changed([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool __dirty = false;
        public bool IsDirty
        {
            get { return __dirty; }
            set
            {
                if (__dirty != value)
                {
                    __dirty = value;
                    _notify_property_changed();
                }
            }
        }

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

        public ContextModel(RecipeDocument doc)
        {
            RecipeDocument = doc;
            if (doc.ExceptionResponseSource is SimpleExceptionResponse_S || doc.ExceptionResponseSource == null)
                ExceptionResponse = new SimpleExceptionResponseModel(doc.ExceptionResponseSource as SimpleExceptionResponse_S, this) { Owner = null };
            else
                throw new ArgumentException("Unsupported exception response source.");

            Initialization = new InitializationModel(doc.InitializationConfiguration, this) { Owner = null };
        }

        public RecipeDocument RecipeDocument { get; }
        public ExceptionResponseModel ExceptionResponse { get; }
        public InitializationModel Initialization { get; }
    }
}
