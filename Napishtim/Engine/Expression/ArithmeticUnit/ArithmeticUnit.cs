using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression.AU
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ArithmeticUnitUsageAttribute : Attribute
    {
        public string Usage { get; }
        public ArithmeticUnitUsageAttribute(string usage) {
            Usage = usage;
        }
    }
    public abstract class ArithmeticUnit
    {
        private static Dictionary<string, string> __COMPATIBLE_ARITHMETIC_UINT_NAMES = new Dictionary<string, string>();
        public static IReadOnlyDictionary<string, string> CompatibleArithmeticUnitNames { get; }
        static ArithmeticUnit()
        {
            var assem = typeof(ArithmeticUnit).Assembly;
            foreach (var t in assem.GetTypes())
            {
                var bt = t.BaseType;
                if(bt != null && bt.FullName == typeof(ArithmeticUnit).FullName)
                {
                    __ARITHMETIC_UNIT_FACTORY[t.Name] = p => Activator.CreateInstance(t, p);
                    __COMPATIBLE_ARITHMETIC_UINT_NAMES.Add(t.Name, t.GetCustomAttribute<ArithmeticUnitUsageAttribute>().Usage);
                }
            }
            CompatibleArithmeticUnitNames = __COMPATIBLE_ARITHMETIC_UINT_NAMES;
        }
        public ArithmeticUnit(IReadOnlyList<Expression> parameters) 
        {
            _parameters = parameters;
        }

        public delegate Object? MakeArithmeticUnit(IReadOnlyList<Expression> parameters);

        protected IReadOnlyList<Expression> _parameters;
        private static Dictionary<string, MakeArithmeticUnit> __ARITHMETIC_UNIT_FACTORY = new Dictionary<string, MakeArithmeticUnit>();

        public abstract double Value();
        public abstract string Name { get; }

        public static ArithmeticUnit MAKE(string name, IReadOnlyList<Expression> parameters)
        {
            try
            {
                if (__ARITHMETIC_UNIT_FACTORY.ContainsKey(name))
                {
                    var au = __ARITHMETIC_UNIT_FACTORY[name](parameters) as ArithmeticUnit;
                    if (au != null)
                        return au;
                    else
                        throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR, $"Unknown sprite name: {name}.");
                }
                else
                    throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EXPRESSION_SPRITE_SYNTAX_ERROR, $"Unknown sprite name: {name}.");
            }
            catch (NaposhtimException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    throw ex.InnerException;
                throw;
            }
        }

        public override string ToString()
        {
            string body = String.Join(',', _parameters.Select(p => p.ToString()));
            return $"{Name}({body})";
        }
    }
}
