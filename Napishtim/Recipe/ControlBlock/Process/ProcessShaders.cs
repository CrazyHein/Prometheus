using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.ShaderMechansim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock.Process
{
    public record ProcessShader(string Name, Shader Shader)
    {
        public JsonNode ToJson()
        {
            JsonObject o = new JsonObject();
            o["NAME"] = Name;
            JsonObject s = Shader.ToJson().AsObject();
            foreach(var p in s)
                o[p.Key] = p.Value.DeepClone();
            return o;
        }
    }
    public abstract class ProcessShaders
    {
        protected List<ProcessShader> _shaders;
        public ProcessShaders(IEnumerable<(string name, string lvalue, string rvalue)> shaders)
        {
            _shaders = new List<ProcessShader>();
            foreach (var s in shaders.Select(s => new ProcessShader(s.name, new Shader(s.lvalue, s.rvalue))))
                _shaders.Add(s);
        }

        public JsonArray ToJson()
        {
            var shaderArray = new JsonArray();
            foreach (var s in _shaders)
            {
                JsonObject o = new JsonObject();
                o["NAME"] = s.Name;
                o["OBJECT"] = s.Shader.Operand.ToString();
                o["VALUE"] = s.Shader.Expr.ToString();
                shaderArray.Add(o);
            }
            return shaderArray;
        }

        public ProcessShader this[int i]
        {
            get { return _shaders[i]; }
        }

        public IEnumerable<ProcessShader> Shaders => _shaders;
    }

    internal class ReservedProcessShaders: ProcessShaders
    {
        public ReservedProcessShaders(IEnumerable<(string name, string lvalue, string rvalue)> shaders): base(shaders)
        {
            foreach (var s in _shaders)
            {
                if (s.Shader.Operand is EnvVariableReference && (s.Shader.Operand as EnvVariableReference).EnvType == ENV_VARIABLE_TYPE_T.USER)
                {
                    var idx = (s.Shader.Operand as EnvVariableReference).UserVariableIndex;
                    if (idx >= Context.UserVariableCapacity)
                        throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_ARGUMENTS, $"The reserved user variable index should between 0 and {Context.UserVariableCapacity - 1}.");
                    //if (idx >= context.ReservedUserVariableCapacity)
                    //throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_ARGUMENTS, $"The reserved user variable index should between 0 and {context.ReservedUserVariableCapacity - 1}.");
                }
            }
        }
    }

    public class UserProcessShaders: ProcessShaders
    {
        public UserProcessShaders(IEnumerable<(string name, string lvalue, string rvalue)> shaders) : base(shaders)
        {
            foreach (var s in _shaders)
            {
                if (s.Shader.Operand is EnvVariableReference && (s.Shader.Operand as EnvVariableReference).EnvType == ENV_VARIABLE_TYPE_T.USER)
                {
                    var idx = (s.Shader.Operand as EnvVariableReference).UserVariableIndex;
                    if (idx >= Context.UserVariableCapacity)
                        throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_ARGUMENTS, $"The user variable index should between 0 and {Context.UserVariableCapacity - 1}.");
                    //if (idx < context.ReservedUserVariableCapacity)
                    //throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_ARGUMENTS, $"User variable indexes between {context.ReservedUserVariableCapacity} and {Context.UserVariableCapacity - 1} are reserved for system internal use.");
                    //else if (idx >= Context.UserVariableCapacity)
                    //throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_ARGUMENTS, $"User variable indexes between {context.ReservedUserVariableCapacity} and {Context.UserVariableCapacity - 1} are reserved for system internal use.");
                }
            }
        }
    }
}
