using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine
{
    public class InitializationList
    {
        public IReadOnlyList<(string name, double value)> UserVariableInitialValues { get; private set; }

        public InitializationList()
        {
            UserVariableInitialValues = new List<(string, double)>();
        }

        public InitializationList(IEnumerable<(string, double)> userVariables)
        {
            UserVariableInitialValues = new List<(string, double)>(userVariables);
        }

        public InitializationList(JsonObject node)
        {
            var userVariableInitialValues = new List<(string, double)>();
            if (node.TryGetPropertyValue("USER_VARIABLES", out var subnode))
            {
                try
                {
                    foreach (var p in subnode.AsObject())
                        userVariableInitialValues.Add((p.Key, p.Value.AsValue().GetValue<double>()));
                }
                catch (Exception ex)
                {
                    throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_INITIALIZATION_PARSE_ERROR, $"Can not restore InitializationList from node:\n{node.ToString()}", ex);
                }
            }
            UserVariableInitialValues = userVariableInitialValues;
        }

        public JsonNode ToJson()
        {
            JsonObject node = new JsonObject(); ;

            node["USER_VARIABLES"] = new JsonObject();

            if (UserVariableInitialValues.Count != 0)
            {
                foreach (var kv in UserVariableInitialValues)
                    node["USER_VARIABLES"][kv.name] = kv.value;
            }
            return node;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"USER_VARIABLES:");
            if (UserVariableInitialValues.Count != 0)
            {
                foreach (var v in UserVariableInitialValues)
                    sb.Append($"\n\t{v.name} = {v.value}");
            }
            else
                sb.Append($"\n\t<EMPTY>");

            return sb.ToString();
        }
    }
}
