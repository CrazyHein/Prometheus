using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.BranchMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim.TriggerMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression.AU;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.StepMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock
{
    public class Switch_S : ControlBlockSource
    {
        public IReadOnlyList<(string name, JsonArray condition, ControlBlockSource action)> Branches { get { return __branches; } }

        public override IEnumerable<uint> GlobalEventReference => 
            __branches.SelectMany(x => ProcessStep.SearchGlobalEventIndex(x.condition)).Concat(__branches.SelectMany(x => x.action.GlobalEventReference)).Distinct();

        private Dictionary<uint, (string name, Event evt)> __locals;
        private List<(string name, JsonArray condition, ControlBlockSource action)> __branches;

        public Switch_S(string name, IReadOnlyDictionary<uint, (string name, Event evt)>? locals, IEnumerable<(string name, JsonArray condition, ControlBlockSource action)> branches): base(name)
        {
            var height = branches.Count() == 0? 0 : branches.Max(x => x.action.Height);
            if (this.Nesting + height > MAX_NESTING_DEPTH)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"The nesting depth of the a Control Block exceeds the limit(MAX: {ControlBlockSource.MAX_NESTING_DEPTH}).");

            if (locals != null)
                __locals = new Dictionary<uint, (string name, Event evt)>(locals);
            else
                __locals = new Dictionary<uint, (string name, Event evt)>();

            __branches = new List<(string name, JsonArray condition, ControlBlockSource action)>(branches);
            //StepFootprint = 1 + __branches.Sum(x => x.action.StepFootprint);
            //UserVariableFootprint = 0 + __branches.Sum(x => x.action.UserVariableFootprint);
        }

        public Switch_S(string name) : base(name)
        {
            __locals = new Dictionary<uint, (string name, Event evt)>();
            __branches = new List<(string name, JsonArray condition, ControlBlockSource action)>();
            //StepFootprint = 1;
            //UserVariableFootprint = 0;
        }

        private Switch_S(JsonObject node) : base(node["NAME"].GetValue<string>())
        {
            __branches = new List<(string name, JsonArray condition, ControlBlockSource action)>();
            //StepFootprint = 1;
            var locals = new Dictionary<uint, (string name, Event evt)>();
            foreach (var evt in node["EVENTS"].AsArray())
            {
                locals[evt["ID"].GetValue<uint>()] = (ProcessStepSource.DEFAULT_NAME(evt["NAME"]), Event.MAKE(evt["EVENT"]));
            }
            __locals = locals;
            foreach (var branch in node["BRANCHES"].AsArray())
            {
                string name = branch["NAME"].GetValue<string>();
                JsonArray condition = branch["CONDITION"].DeepClone().AsArray();
                ControlBlockSource action = ControlBlockSource.MAKE_BLK(branch["ACTION"].AsObject(), null);
                AddBranch(name, condition, action);
            }
        }

        public override int StepFootprint => 1 + __branches.Sum(x => x.action.StepFootprint);

        public override int UserVariableFootprint => 0 + __branches.Sum(x => x.action.UserVariableFootprint);

        public override IEnumerable<uint> ShaderUserVariablesUsage => __branches.SelectMany(x => x.action.ShaderUserVariablesUsage).Distinct();


        public override JsonObject SaveAsJson()
        {
            JsonObject node = new JsonObject();
            node["ASSEMBLY"] = this.GetType().FullName;
            node["NAME"] = Name;
            JsonArray eventsNode = new JsonArray();
            node["EVENTS"] = eventsNode;
            foreach(var evt in __locals)
            {
                JsonObject eventNode = new JsonObject();
                eventNode["ID"] = evt.Key;
                eventNode["NAME"] = evt.Value.name;
                eventNode["EVENT"] = evt.Value.evt.ToJson();
                eventsNode.Add(eventNode);
            }
            JsonArray branchesNode = new JsonArray();
            node["BRANCHES"] = branchesNode;
            foreach(var branch in __branches)
            {
                JsonObject branchNode = new JsonObject();
                branchNode["NAME"] = branch.name;
                branchNode["CONDITION"] = branch.condition.DeepClone();
                branchNode["ACTION"] = branch.action.SaveAsJson();
                branchesNode.Add(branchNode);
            }
            return node;
        }
        static public ControlBlockSource MAKE(JsonObject node, ControlBlockSource? owner)
        {
            try
            {
                if (node["ASSEMBLY"].GetValue<string>() != typeof(Switch_S).FullName)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, $"Assmebly name mismatch: {node["ASSEMBLY"].GetValue<string>()} vs {typeof(Switch_S).FullName}.");

                return new Switch_S(node) { Owner = owner };
            }
            catch (NaposhtimException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, $"Can not restore Switch_S object from node:\n{node.ToString()}", ex);
            }
        }

        public IReadOnlyDictionary<uint, (string name, Event evt)> LocalEvents { get { return __locals; } }

        public override int Height
        {
            get 
            {
                if (__branches.Count == 0)
                    return 1;
                else
                    return __branches.Max(x => x.action.Height) + 1;
            }
        }

        public override IEnumerable<(Sequential_S container, ProcessStepSource step)> ProcessSteps => __branches.SelectMany(x => x.action.ProcessSteps);

        public void MergeLocalEvents(IEnumerable<(uint idx, string name, Event evt)> locals)
        {
            foreach(var evt in locals)
                __locals[evt.idx] = (evt.name, evt.evt);
        }

        public void AddLocalEvent(uint index, string name, Event evt)
        {
            if(__locals.ContainsKey(index))
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION,
                     $"The local event with the same index({index}) has existed:\n{__locals[index].name}\n{__locals[index].evt.ToJson().ToString()}");

            __locals[index] = (name, evt);
        }

        public void RemoveLocalEvent(uint index)
        {
            if (__locals.ContainsKey(index) == false)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"Local event with index({index}) does not exist.");

            __locals.Remove(index);
        }

        public void ClearLocalEvents()
        {
            __locals.Clear();
        }

        public void ClearBranches()
        {
            int pos = __branches.Count;
            while (pos-- > 0)
                RemoveBranchAt(pos);
        }

        public void AddBranches(IEnumerable<(string name, JsonArray condition, ControlBlockSource action)> branches)
        {
            var height = branches.Count() == 0 ? 0 : branches.Max(x => x.action.Height);
            if (this.Nesting + height > MAX_NESTING_DEPTH)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"The nesting depth of a Control Block exceeds the limit(MAX: {ControlBlockSource.MAX_NESTING_DEPTH}).");
            if(branches.Any(x => x.action.Owner != null))
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"At least one Control Block already has an owner.");

            foreach (var b in branches)
            {
                b.action.Owner = this;
                __branches.Add((b.name, b.condition, b.action));
                //StepFootprint += b.action.StepFootprint;
                //UserVariableFootprint += b.action.UserVariableFootprint;
            }
        }

        public void AddBranch(string name, JsonArray condition, ControlBlockSource action)
        {
            if (this.Nesting + action.Height > MAX_NESTING_DEPTH)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"The nesting depth of the Control Block exceeds the limit(MAX: {ControlBlockSource.MAX_NESTING_DEPTH}).");

            action.Owner = this;
            __branches.Add((name, condition, action));
            //StepFootprint += action.StepFootprint;
            //UserVariableFootprint += action.UserVariableFootprint;
        }

        public void InsertBranch(int pos, string name, JsonArray condition, ControlBlockSource action)
        {
            if (pos >= __branches.Count || pos < 0)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"The insertion position {pos} is invalid to a list of length {__branches.Count}.");
            if (this.Nesting + action.Height > MAX_NESTING_DEPTH)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"The nesting depth of the the Control Block exceeds the limit(MAX: {ControlBlockSource.MAX_NESTING_DEPTH}).");

            action.Owner = this;
            __branches.Insert(pos, (name, condition, action));
            //StepFootprint += action.StepFootprint;
            //UserVariableFootprint += action.UserVariableFootprint;
        }

        public void MoveBranch(int oldIndex, int newIndex)
        {
            if (oldIndex >= __branches.Count || oldIndex < 0)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"The old position {oldIndex} is invalid to a list of length {__branches.Count}.");
            if (newIndex >= __branches.Count || newIndex < 0)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"The new position {newIndex} is invalid to a list of length {__branches.Count}.");
        
            var temp = __branches[oldIndex];
            __branches.RemoveAt(oldIndex);
            __branches.Insert(newIndex, temp);
        }

        public void RemoveBranchAt(int index)
        {
            if(index >= __branches.Count)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"The removal position {index} is invalid to a list of length {__branches.Count}.");
        
            var branch = __branches[index];
            branch.action.Owner = null;
            __branches.RemoveAt(index);
            
            //StepFootprint -= branch.action.StepFootprint;
            //UserVariableFootprint -= branch.action.UserVariableFootprint;
            //Level = __branches.Max(x => x.action.Level) + 1;
        }

        public override ControlBlockObject ResolveTarget(uint next, uint abort, uint? breakp, uint? continuep, Context context, IReadOnlyDictionary<uint, Event> globals, ReadOnlyMemory<uint> stepLinkMapping, ReadOnlyMemory<uint> userVariableMapping, Dictionary<uint, string> stepNameMapping)
        {
            if (__branches.Count == 0)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, $"The number of Switch({FullName}) Control Block branches is zero.");
            int pos = 1;
            var trans = delegate (ValueTuple<string, JsonArray, ControlBlockSource> x)
            {
                if(pos >= stepLinkMapping.Length)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, "Can not find any Control Step in sub control blocks of 'Switch' Control Block.");
                var ret = new ValueTuple<string, JsonArray, ProcessShaders?, uint>(x.Item1, x.Item2, null, stepLinkMapping.Span[pos]);
                pos += x.Item3.StepFootprint;
                return ret;
            };
            var parameters = __branches.Select(x => trans(x));
            var step = new BranchStep_S("#BranchStep#", __locals, parameters);
            var conditionalStatement = new Sequential_S("#BranchControlBlock#", [step]) { Owner = this};

            //__conditional_statement.Next = Next;
            //foreach (var b in __branches)
                //b.action.Next = __conditional_statement.Next;

            var c = conditionalStatement.ResolveTarget(next, abort, breakp, continuep, context, globals, stepLinkMapping, userVariableMapping, stepNameMapping);
            List<ControlBlockObject> a = new List<ControlBlockObject>(__branches.Count);
            int spos = 1, upos = 0;
            foreach (var b in __branches)
            {
                a.Add(b.action.ResolveTarget(next, abort, breakp, continuep, context, globals, stepLinkMapping.Slice(spos), userVariableMapping.Slice(upos), stepNameMapping));
                spos += b.action.StepFootprint;
                upos += b.action.UserVariableFootprint;
            }

            return new Switch_O(Name, c, a, StepFootprint, UserVariableFootprint);
        }

        public override bool ContainsGlobalEventReference(uint index)
        {
            return GlobalEventReference.Contains(index);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder($"{Name}({typeof(Switch_S).FullName})");
            /*
            if (__locals?.Count != 0)
            {
                sb.Append("\nLocal Events:");
                foreach (var evt in __locals)
                    sb.Append($"\n\t{evt.Key}: {evt.Value.ToJson().ToJsonString()}");
            }*/
            int numOfBranches = __branches.Count;
            sb.Append($"\n<Switch NumberOfBranches = \"{numOfBranches}\">");
            int i = 0;
            foreach (var b in __branches)
            {
                string case_fix = string.Concat(Enumerable.Repeat("\x20\u2503\t", i + 0));
                string con_fix = string.Concat(Enumerable.Repeat("\x20\u2503\t", i + 1));
                sb.Append($"\n{case_fix}<Case Index = \"{i}\" Name = \"{b.name}\"]>");
                foreach (var con in b.condition)
                {
                    string conline = con.GetValue<string>();
                    conline = Trigger.LOCAL_EVENT_PATTERN.Replace(conline, delegate (Match m)
                    {
                        var localEvtIdx = uint.Parse(m.Value.Substring(5));
                        return $"{localEvtIdx:D10} {__locals[localEvtIdx].name}: {__locals[localEvtIdx].evt.ToJson().ToJsonString()}";
                    }, 1);
                    sb.Append($"\n{con_fix}{conline}");
                }
                i++;
            }
            foreach (var b in __branches.Reverse<(string name, JsonArray condition, ControlBlockSource action)>())
            {
                string action_fix = string.Concat(Enumerable.Repeat("\x20\u2503\t", i - 1));
                foreach(var line in b.action.ToString().Split('\n').Skip(1))
                    sb.Append($"\n{action_fix}{line}");
                sb.Append($"\n{action_fix}</Case>");
                i--;
            }
            sb.Append("\n</Switch>");
            //foreach (var b in __branches)
            //sb.Append(b.condition.ToString()).Append(b.action.ToString()).Append('\n');
            return sb.ToString();
        }
    }

    public class Switch_O: ControlBlockObject
    {
        ControlBlockObject __condition;
        List<ControlBlockObject> __actions;
        internal Switch_O(string name, ControlBlockObject condition, IEnumerable<ControlBlockObject> actions, int stepFootprint, int userVariableFootprint):base(name)
        {
            __condition = condition;
            __actions = new List<ControlBlockObject>(actions);
            //StepFootprint = stepFootprint;
            //UserVariableFootprint = userVariableFootprint;
            //Level = level;
            ID = condition.ID;
        }

        public override int StepFootprint => 1 + __actions.Sum(x => x.StepFootprint);

        public override int UserVariableFootprint => 0 + __actions.Sum(x => x.UserVariableFootprint);

        public override IEnumerable<Step> Build(Context context, IReadOnlyDictionary<uint, Event> globals)
        {
            var x = __condition.Build(context, globals);
            var y = __actions.SelectMany(b => b.Build(context, globals));

            return x.Concat(y);
        }

        public override IEnumerable<ProcessStepObject> ProcessSteps => __actions.SelectMany(b => b.ProcessSteps);
    }
}
