using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression.AU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim.TriggerMechansim
{
    public enum TRIGGER_TYPE_T
    {
        OPERAND,
        TREE,
    }
    public enum TRIGGER_OPERATOR_T
    {
        AND,
        OR,
        NOT,
        XOR,
        NAND,
        NOR
    }
    public abstract class Element
    {
        private readonly LogicTree? __root;

        public Element(LogicTree? root)
        {
            __root = root;
            Layer = root == null ? 0 : root.Layer + 1;
        }

        public abstract List<string> ToPlainText();
        public abstract List<string> ToPlainText(IReadOnlyDictionary<uint, Event> globalEvts, IReadOnlyDictionary<uint, Event> localEvts);
        public abstract TRIGGER_TYPE_T Type();
        public int Layer { get; private init; }
    }

    public class LogicOperand : Element
    {
        //private readonly Event __internal_event;
        private readonly uint __index;
        private readonly bool __global_region;
        public LogicOperand(/*Event evt,*/ uint idx, bool region, LogicTree root) : base(root)
        {
            //__internal_event = evt;
            __index = idx;
            __global_region = region;
        }
        public override List<string> ToPlainText()
        {
            string indent = new string('\x20', Layer);
            return new List<string> { $"{indent}{(__global_region ? "GEVENT" : "EVENT")}{__index}" };
        }

        public override List<string> ToPlainText(IReadOnlyDictionary<uint, Event> globalEvts, IReadOnlyDictionary<uint, Event> localEvts)
        {
            if (__global_region == true && globalEvts.ContainsKey(__index) == false)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_REF_NOT_FOUND, $"GEVENT{__index} has not been defined.");
            if (__global_region == false && localEvts.ContainsKey(__index) == false)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_REF_NOT_FOUND, $"EVENT{__index} has not been defined.");

            var evt = __global_region ? globalEvts[__index] : localEvts[__index];
            string indent = new string('\x20', Layer);
            return new List<string> { $"{indent}{evt.ToJson().ToJsonString()}" };
        }

        public override TRIGGER_TYPE_T Type()
        {
            return TRIGGER_TYPE_T.OPERAND;
        }
    }

    public class LogicTree : Element
    {
        public TRIGGER_OPERATOR_T Operator { get; private init; }
        public List<Element> Elements { get; private init; } = new List<Element>();

        public LogicTree(TRIGGER_OPERATOR_T op, LogicTree root) : base(root)
        {
            Operator = op;
        }

        public override TRIGGER_TYPE_T Type()
        {
            return TRIGGER_TYPE_T.TREE;
        }

        public override List<string> ToPlainText()
        {
            string indent = new string('\x20', Layer);
            List<string> lines = new List<string>();
            lines.Add($"{indent}{Operator.ToString()}");
            for (int i = 0; i < Elements.Count; ++i)
                lines.AddRange(Elements[i].ToPlainText());
            return lines;
        }

        public override List<string> ToPlainText(IReadOnlyDictionary<uint, Event> globalEvts, IReadOnlyDictionary<uint, Event> localEvts)
        {
            string indent = new string('\x20', Layer);
            List<string> lines = new List<string>();
            lines.Add($"{indent}{Operator.ToString()}");
            for (int i = 0; i < Elements.Count; ++i)
                lines.AddRange(Elements[i].ToPlainText(globalEvts, localEvts));
            return lines;
        }
    }

    internal class __Statement
    {
        public required int Tabs { get; init; }
        //public required Event? Evt { get; init; }
        public required uint Idx { get; init; }
        public required bool Region { get; init; }
        public required TRIGGER_OPERATOR_T? Operator { get; init; }
    }

    public class Trigger
    {
        public const int MaxNesting = 5;
        private Element __logic_tree;

        public static Regex GLOBAL_EVENT_PATTERN = new Regex("^GEVENT(([0-9])|([1-9][0-9]{0,}))$", RegexOptions.Compiled);
        public static Regex LOCAL_EVENT_PATTERN = new Regex("^EVENT(([0-9])|([1-9][0-9]{0,}))$", RegexOptions.Compiled);
        public static Regex INLINE_EVENT_PATTERN = new Regex("^\\{.+\\}$", RegexOptions.Compiled);
        private SortedSet<uint> __referenced_global_events = new System.Collections.Generic.SortedSet<uint>();
        private SortedSet<uint> __referenced_local_events = new System.Collections.Generic.SortedSet<uint>();
        public IEnumerable<uint> ReferencedGlobalEvents { get; }
        public IEnumerable<uint> ReferencedLocalEvents { get; }

        public Trigger(JsonNode node, IReadOnlyDictionary<uint, Event> globalEvts, Dictionary<uint, Event> localEvts, ref uint inlineEventIndex)
        {
            int start = 0;
            List<__Statement> buffer = new List<__Statement>();
            __converter(node, buffer, globalEvts, localEvts, ref inlineEventIndex);
            __logic_tree = __search_logic_element(buffer, ref start, null);
            ReferencedGlobalEvents = __referenced_global_events;
            ReferencedLocalEvents = __referenced_local_events;
        }

        public Trigger(IEnumerable<string> node, IReadOnlyDictionary<uint, Event> globalEvts, Dictionary<uint, Event> localEvts, ref uint inlineEventIndex)
        {
            JsonArray jsonArray = new JsonArray() { node};
            int start = 0;
            List<__Statement> buffer = new List<__Statement>();
            __converter(jsonArray, buffer, globalEvts, localEvts, ref inlineEventIndex);
            __logic_tree = __search_logic_element(buffer, ref start, null);
            ReferencedGlobalEvents = __referenced_global_events;
            ReferencedLocalEvents = __referenced_local_events;
        }


        private void __converter(JsonNode node, List<__Statement> buffer, IReadOnlyDictionary<uint, Event> globalEvts, Dictionary<uint, Event> localEvts, ref uint inlineEventIndex)
        {
            if(node.GetValueKind() != System.Text.Json.JsonValueKind.Array || node.AsArray().Count == 0)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_TRIGGER_PARSE_ERROR, $"{node.ToString()}\nis not a valid array or the array length is 0.");
            foreach(var line in node.AsArray())
            {
                if(line.GetValueKind() != System.Text.Json.JsonValueKind.String)
                    throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_TRIGGER_PARSE_ERROR, $"{line.ToString()}\nis not a valid string.");

                string sline = line.GetValue<string>();
                int tabs = sline.IndexOf(sline.First(c => c != '\x20'));

                if(sline.Substring(tabs) == TRIGGER_OPERATOR_T.AND.ToString())
                {
                    buffer.Add(new __Statement() { Tabs = tabs, Operator = TRIGGER_OPERATOR_T.AND, Idx = 0, Region = false });
                }
                else if (sline.Substring(tabs) == TRIGGER_OPERATOR_T.OR.ToString())
                {
                    buffer.Add(new __Statement() { Tabs = tabs, Operator = TRIGGER_OPERATOR_T.OR, Idx = 0, Region = false });
                }
                else if (sline.Substring(tabs) == TRIGGER_OPERATOR_T.NOT.ToString())
                {
                    buffer.Add(new __Statement() { Tabs = tabs, Operator = TRIGGER_OPERATOR_T.NOT, Idx = 0, Region = false });
                }
                else if (sline.Substring(tabs) == TRIGGER_OPERATOR_T.XOR.ToString())
                {
                    buffer.Add(new __Statement() { Tabs = tabs, Operator = TRIGGER_OPERATOR_T.XOR, Idx = 0, Region = false });
                }
                else if (sline.Substring(tabs) == TRIGGER_OPERATOR_T.NAND.ToString())
                {
                    buffer.Add(new __Statement() { Tabs = tabs, Operator = TRIGGER_OPERATOR_T.NAND, Idx = 0, Region = false });
                }
                else if (sline.Substring(tabs) == TRIGGER_OPERATOR_T.NOR.ToString())
                {
                    buffer.Add(new __Statement() { Tabs = tabs, Operator = TRIGGER_OPERATOR_T.NOR, Idx = 0, Region = false });
                }
                else
                {
                    uint eventIdx = 0;
                    if (GLOBAL_EVENT_PATTERN.IsMatch(sline.Substring(tabs)))
                    {
                        eventIdx = uint.Parse(sline.Substring(tabs + "GEVENT".Length));
                        if(globalEvts.ContainsKey(eventIdx))
                            buffer.Add(new __Statement() { Tabs = tabs, Operator = null, Idx = eventIdx, Region = true });
                        else
                            throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_REF_NOT_FOUND, $"GEVENT{eventIdx} has not been defined.");
                    }
                    else if (LOCAL_EVENT_PATTERN.IsMatch(sline.Substring(tabs)))
                    {
                        eventIdx = uint.Parse(sline.Substring(tabs + "EVENT".Length));
                        if (localEvts.ContainsKey(eventIdx))
                            buffer.Add(new __Statement() { Tabs = tabs, Operator = null, Idx = eventIdx, Region = false });
                        else
                            throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_EVENT_REF_NOT_FOUND, $"EVENT{eventIdx} has not been defined.");
                    }
                    else if(INLINE_EVENT_PATTERN.IsMatch(sline.Substring(tabs)))
                    {
                        JsonNode inlineNode;
                        try
                        {
                            inlineNode = JsonNode.Parse(sline.Substring(tabs));
                        }
                        catch
                        {
                            throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_TRIGGER_PARSE_ERROR, $"{line.ToString()}\nis not a valid trigger line(Parsing Error).");
                        }
                        Event inlineEvent = Event.MAKE(inlineNode);
                        uint inlineIdx;
                        for(inlineIdx = inlineEventIndex; ;++inlineIdx)
                        {
                            if (localEvts.ContainsKey(inlineIdx) == false)
                                break;
                        }
                        localEvts[inlineIdx] = inlineEvent;
                        inlineEventIndex = inlineIdx + 1;
                        buffer.Add(new __Statement() { Tabs = tabs, Operator = null, Idx = inlineIdx, Region = false });
                    }
                    else
                        throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_TRIGGER_PARSE_ERROR, $"{line.ToString()}\nis not a valid trigger line.");
                }
            }

        }

        private Element __search_logic_element(IReadOnlyList<__Statement> buffer, ref int start, LogicTree? root)
        {
            if (root != null && root.Layer == MaxNesting)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_TRIGGER_LOGIC_TREE_DEPTH_OUT_OF_RANGE, $"The maximum nesting depth of a logical tree is {MaxNesting}.");

            LogicTree tree = null;
            Element inner = null;
            var st = buffer[start];
            if (st.Operator == null)
            {
                var operand = new LogicOperand(st.Idx, st.Region, root);
                if (st.Region)
                {
                    if(__referenced_global_events.Contains(st.Idx) == false)
                        __referenced_global_events.Add(st.Idx);
                }
                else
                {
                    if (__referenced_local_events.Contains(st.Idx) == false)
                        __referenced_local_events.Add(st.Idx);
                }
                return operand;
            }
            else
                tree = new LogicTree((TRIGGER_OPERATOR_T)st.Operator, root);

            while(start + 1 < buffer.Count)
            {
                if (buffer[start + 1].Tabs == st.Tabs + 1)
                {
                    start++;
                    inner = __search_logic_element(buffer, ref start, tree);
                    if((tree.Elements.Count == 0 && tree.Operator == TRIGGER_OPERATOR_T.NOT) || tree.Operator != TRIGGER_OPERATOR_T.NOT)
                        tree.Elements.Add(inner);
                    else
                        throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_TRIGGER_PARSE_ERROR, $"The number of operands or sub logic trees for '{tree.Operator}' is incorrect.");
                }
                else if (buffer[start + 1].Tabs > st.Tabs + 1)
                    throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_TRIGGER_PARSE_ERROR, "Too much indentation.");
                else
                    break;
            }
            if(tree.Elements.Count == 0)
                throw new NaposhtimScriptException(NaposhtimExceptionCode.SCRIPT_TRIGGER_PARSE_ERROR, "Operand or sub logic tree missing.");
            return tree;
        }

        public JsonNode ToJson()
        {
            JsonArray array = new JsonArray();
            foreach(var line in __logic_tree.ToPlainText())
                array.Add(line);
            return array;
        }

        public JsonNode ToJson(IReadOnlyDictionary<uint, Event> globalEvts, IReadOnlyDictionary<uint, Event> localEvts)
        {
            JsonArray array = new JsonArray();
            foreach (var line in __logic_tree.ToPlainText(globalEvts, localEvts))
                array.Add(line);
            return array;
        }
    }
}
