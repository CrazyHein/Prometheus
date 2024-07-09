using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.ExceptionMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.ShaderMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.StepMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ExceptionHandling
{
    public class SimpleExceptionResponse_S: ExceptionResponseSource
    {
        public override IEnumerable<(string name, JsonArray condition, ProcessShaders postShaders)> Branches
        {
            get
            {
                foreach(var s in _exception_response["END_POINTS"].AsArray())
                {
                    var shaderNode = s["POST_SHADERS"][0].AsObject();
                    var shaders = new ReservedProcessShaders([(DEFAULT_NAME(shaderNode["NAME"]), shaderNode["OBJECT"].GetValue<string>(), shaderNode["VALUE"].GetValue<string>())]);

                    yield return (s["NAME"].GetValue<string>(), s["TRIGGER"].AsArray(), shaders);
                }
            }
        }

        public override IEnumerable<KeyValuePair<uint, (string name, Event evt)>> LocalEvents
        {
            get
            {
                if (_exception_response.TryGetPropertyValue("EVENTS", out var locals))
                {
                    return locals.AsArray().Select(x => KeyValuePair.Create(x["ID"].GetValue<uint>(), (DEFAULT_NAME(x["NAME"]), Event.MAKE(x["EVENT"]))));
                }
                else
                    return Enumerable.Empty<KeyValuePair<uint, (string name, Event evt)>>();

            }
        }

        public IEnumerable<string> TerminationConditions
        {
            get
            {
                foreach(var e in _exception_response["END_POINTS"].AsArray())
                    yield return string.Join('\n', e["TRIGGER"].AsArray().Select(x => x.GetValue<string>()));
            }
        }

        public SimpleExceptionResponse_S(string name, IReadOnlyDictionary<uint, (string name, Event evt)>? locals, IEnumerable<(string name, JsonArray condition, int returnCode)> responses) : base(name)
        {
            if (locals != null && locals.Count > 0)
            {
                _exception_response["EVENTS"] = new JsonArray();
                foreach (var local in locals)
                {
                    JsonObject localEvt = new JsonObject();
                    localEvt["ID"] = local.Key;
                    localEvt["NAME"] = local.Value.name;
                    localEvt["EVENT"] = local.Value.evt.ToJson();
                    _exception_response["EVENTS"].AsArray().Add(localEvt);
                }
            }

            _exception_response["END_POINTS"] = new JsonArray();
            foreach (var x in responses)
            {
                JsonObject branch = new JsonObject();
                branch["NAME"] = x.name;
                branch["TRIGGER"] = x.condition.DeepClone();
                branch["POST_SHADERS"] = new ReservedProcessShaders([($"RETURN {x.returnCode}", $"&RETURN", $"{x.returnCode}")]).ToJson();
                _exception_response["END_POINTS"].AsArray().Add(branch);
                AddGlobalEventRefernce(x.condition);
            }
        }

        private SimpleExceptionResponse_S(JsonObject node) : base(node["NAME"].GetValue<string>())
        {
            _exception_response = node["RESPONSE"].DeepClone().AsObject();
            foreach (var e in _exception_response["END_POINTS"].AsArray())
                AddGlobalEventRefernce(e["TRIGGER"].AsArray());
        }

        public override JsonObject SaveAsJson()
        {
            JsonObject node = new JsonObject();
            node["ASSEMBLY"] = GetType().FullName;
            node["NAME"] = Name;
            node["RESPONSE"] = _exception_response.DeepClone();

            return node;
        }

        public override ExceptionResponseObject ResolveTarget(uint next, Context context, IReadOnlyDictionary<uint, Event> globals)
        {
            if (_exception_response["END_POINTS"].AsArray().Count == 0)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.EXCEPTION_HANDLING_ARGUMENTS_ERROR, "At least one exception condition must be defined.");

            JsonObject chewed;
            chewed = _exception_response.DeepClone().AsObject();
            foreach(var e in chewed["END_POINTS"].AsArray())
                e["TARGET"] = next;

            return new SimpleExceptionResponse_O(Name, chewed);
        }

        public static ExceptionResponseSource MAKE(JsonObject node)
        {
            try
            {
                if (node["ASSEMBLY"].GetValue<string>() != typeof(SimpleExceptionResponse_S).FullName)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.EXCEPTION_HANDLING_ARGUMENTS_ERROR, $"Assmebly name mismatch: {node["ASSEMBLY"].GetValue<string>()} vs {typeof(SimpleExceptionResponse_S).FullName}.");

                return new SimpleExceptionResponse_S(node);
            }
            catch (Exception ex)
            {
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.EXCEPTION_HANDLING_ARGUMENTS_ERROR, $"Can not restore SimpleExceptionResponse_S object from node:\n{node.ToString()}", ex);
            }
        }
    }

    class SimpleExceptionResponse_O : ExceptionResponseObject
    {
        public SimpleExceptionResponse_O(string name, JsonObject rsp) : base(name, rsp)
        {

        }

        public override ExceptionResponse Build(Context context, IReadOnlyDictionary<uint, Event> globals)
        {
            try
            {
                uint inlineEvent = 10000;
                var s = new ExceptionResponse(_response, globals, ref inlineEvent);
                return s;
            }
            catch (NaposhtimException e)
            {
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_EXCEPTION_RSP_BUILD_ERROR,
                    $"Can not build SimpleExceptionResponse with the following JSON node:\n{_response.ToString()}", e);
            }
        }
    }
}
