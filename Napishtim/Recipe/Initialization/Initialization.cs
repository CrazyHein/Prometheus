using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.Initialization
{
    public class UserVariableInitialValue
    {
        public static readonly Regex USER_VARIABLE_NAME_PATTERN = new Regex("^&(USER([0-9]{1,3}))$", RegexOptions.Compiled);
        public string Comment { get; private set; } = "unnamed_initial_value";

        public string Name { get; private set; } = "&USER0";
        public double Value { get; private set; } = 0.0;

        public UserVariableInitialValue()
        {

        }

        public UserVariableInitialValue(string comment, string name, double value)
        {
            if (USER_VARIABLE_NAME_PATTERN.IsMatch(name.Trim()) == false)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.INITIALIZATION_BLOCK_ARGUMENTS_ERROR, $"Invalid user variable name '{name.Trim()}'. The name should match the pattern: {USER_VARIABLE_NAME_PATTERN}.");

            Comment = comment.Trim();
            Name = name.Trim();
            Value = value;
        }

        public UserVariableInitialValue(JsonObject node)
        {
            try
            {
                Comment = node["COMMENT"].AsValue().GetValue<string>().Trim();
                Name = node["NAME"].AsValue().GetValue<string>().Trim();
                Value = node["VALUE"].AsValue().GetValue<double>();
            }
            catch (Exception ex)
            {
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.INITIALIZATION_BLOCK_ARGUMENTS_ERROR, $"Can not restore UserVariableInitialValue from node:\n{node.ToString()}", ex);
            }
        }

        public JsonObject SaveAsJson()
        {
            JsonObject node = new JsonObject();
            node["COMMENT"] = Comment;
            node["NAME"] = Name;
            node["VALUE"] = Value;
            return node;
        }
    }
    public class InitializationConfiguration
    {
        public List<UserVariableInitialValue> UserVariableInitialValues { get; private set; }

        public InitializationConfiguration()
        {
            UserVariableInitialValues = new List<UserVariableInitialValue>();
            //UserVariableInitialValues.Add(new UserVariableInitialValue("demo101", "&USER101", 10.1));
            //UserVariableInitialValues.Add(new UserVariableInitialValue("demo102", "&USER102", 10.2));
        }

        public InitializationConfiguration(JsonObject settings)
        {
            UserVariableInitialValues = new List<UserVariableInitialValue>();
            try
            {
                if (settings.TryGetPropertyValue("ASSEMBLY", out var node) && node.GetValue<string>() != typeof(InitializationConfiguration).FullName)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, $"Assmebly name mismatch: {node.GetValue<string>()} vs {typeof(InitializationConfiguration).FullName}.");

                if (settings.TryGetPropertyValue("USER_VARIABLES", out node))
                {
                    foreach (var sub in node.AsArray())
                        UserVariableInitialValues.Add(new UserVariableInitialValue(sub.AsObject()));
                }
            }
            catch(NaposhtimException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.INITIALIZATION_BLOCK_ARGUMENTS_ERROR, $"Can not restore InitializationConfiguration from node:\n{settings.ToString()}", ex);
            }
        }

        public InitializationConfiguration(IEnumerable<(string, string, double)> userVariables)
        {
            UserVariableInitialValues = new List<UserVariableInitialValue>(userVariables.Select(v => new UserVariableInitialValue(v.Item1, v.Item2, v.Item3)));
        }

        public JsonObject SaveAsJson()
        {
            JsonObject node = new JsonObject();
            node["ASSEMBLY"] = this.GetType().FullName;
            node["USER_VARIABLES"] = new JsonArray();

            foreach (var v in UserVariableInitialValues)
            {
                node["USER_VARIABLES"].AsArray().Add(v.SaveAsJson());
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
                    sb.Append($"\n\t{v.Comment}: {v.Name} = {v.Value}");
            }
            else
                sb.Append($"\n\t<EMPTY>");

            return sb.ToString();
        }

        public InitializationList Export()
        {
            return new InitializationList(UserVariableInitialValues.Select(v => (v.Name, v.Value)));
        }
    }




}
