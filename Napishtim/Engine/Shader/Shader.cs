using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression.AU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.ShaderMechansim
{
    public class Shader: IEquatable<Shader>
    {
        public Operand Operand { get; private init; }
        public Expression.Expression Expr { get; private init; }
        public Shader(JsonNode node) 
        {
            try
            {
                string o = node["OBJECT"].GetValue<string>();
                
                if(ObjectReference.PATTERN.IsMatch(o))
                    Operand = new ObjectReference(o);
                else if(EnvVariableReference.PATTERN.IsMatch(o))
                    Operand = new EnvVariableReference(o);
                else
                    throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_SHADER_PARSE_ERROR, $"'{o}' is not a valid lvalue of SHADER.");

                Expr = new Expression.Expression(node["VALUE"].GetValue<string>(), null);
            }
            catch (Exception ex)
            {
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_SHADER_PARSE_ERROR, $"{node.ToString()}\nis not a valid SHADER object.", ex);
            }
        }

        public Shader(string lvalue, string rvalue)
        {
            if (ObjectReference.PATTERN.IsMatch(lvalue))
                Operand = new ObjectReference(lvalue);
            else if (EnvVariableReference.PATTERN.IsMatch(lvalue))
                Operand = new EnvVariableReference(lvalue);
            else
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_SHADER_ARGUMENTS_ERROR, $"'{lvalue}' can not be used as the lvalue of Shader.");

            Expr = new Expression.Expression(rvalue, null);
        }

        public JsonNode ToJson()
        {
            JsonObject o = new JsonObject();
            o["OBJECT"] = Operand.ToString();
            o["VALUE"] = Expr.ToString();
            return o;
        }

        public override string ToString()
        {
            return ToJson().ToJsonString();
        }

        public IEnumerable<ObjectReference> ObjectReferences
        {
            get
            {
                var temp = Expr.ObjectReferences;
                if(Operand is ObjectReference)
                    temp = temp.Concat(Enumerable.Repeat<ObjectReference>((ObjectReference)Operand, 1));
                return temp.Distinct();
            }
        }

        public IEnumerable<uint> UserVariablesUsage
        {
            get
            {
                if (Operand is EnvVariableReference && (Operand as EnvVariableReference).EnvType == ENV_VARIABLE_TYPE_T.USER)
                    return Enumerable.Repeat<uint>((Operand as EnvVariableReference).UserVariableIndex, 1).Concat(Expr.UserVariablesUsage).Distinct();
                else
                    return Expr.UserVariablesUsage;
            }
        }

        public bool Equals(Shader? other)
        {
            if (other == null)
                return false;
            else
                return Operand.Equals(other.Operand) && Expr.Equals(other.Expr);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
