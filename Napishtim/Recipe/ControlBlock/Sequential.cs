using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.StepMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using System.Text.Json.Nodes;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression.AU;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.Process;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock
{
    public class Sequential_S: ControlBlockSource
    {
        private LinkedList<ProcessStepSource> __original_process_steps;
        
        public IEnumerable<ProcessStepSource> OriginalProcessSteps { get { return __original_process_steps; } }
        private SortedDictionary<uint, int> __global_event_reference = new SortedDictionary<uint, int>();
        public Dictionary<SimpleStepWithTimeout_S, Expression> TimeToTimeout = new Dictionary<SimpleStepWithTimeout_S, Expression>();

        public Sequential_S(string name, IEnumerable<ProcessStepSource> steps):base(name)
        {
            __original_process_steps = new LinkedList<ProcessStepSource> ();
            foreach(var step in steps)
            {
                if ((step as SimpleStepWithTimeout_S)?.EmployPreceding != null)
                {
                    if (__original_process_steps.Find((step as SimpleStepWithTimeout_S).EmployPreceding) == null)
                        throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The 'SimpleStepWithTimeout' that is not in the same 'Sequential' Control Block is referenced or the referenced 'SimpleStepWithTimeout' is located after the referer.");
                }
                __original_process_steps.AddLast(step);
                //StepFootprint += step.StepFootprint;
                //UserVariableFootprint += step.UserVariableFootprint;
                _AddGlobalEventReference(step.GlobalEventReference);
            }
        }

        private Sequential_S(JsonObject node) : base(node["NAME"].GetValue<string>())
        {
            __original_process_steps = new LinkedList<ProcessStepSource>();
            foreach (var step in node["STEPS"].AsArray())
            {
                ProcessStepSource st = ProcessStepSource.MAKE_STEP(step.AsObject(), this);
                AddProcessStepLast(st);
            }
        }

        public override int StepFootprint => __original_process_steps.Sum(x => x.StepFootprint);
        public override int UserVariableFootprint => __original_process_steps.Sum(x => x.UserVariableFootprint);

        static public ControlBlockSource MAKE(JsonObject node, ControlBlockSource? owner)
        {
            try
            {
                if (node["ASSEMBLY"].GetValue<string>() != typeof(Sequential_S).FullName)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, $"Assmebly name mismatch: {node["ASSEMBLY"].GetValue<string>()} vs {typeof(Sequential_S).FullName}.");

                return new Sequential_S(node) { Owner = owner };
            }
            catch (Exception ex)
            {
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, $"Can not restore Sequential_S object from node:\n{node.ToString()}", ex);
            }
        }

        public override IEnumerable<uint> ShaderUserVariablesUsage => __original_process_steps.SelectMany(x => x.ShaderUserVariablesUsage).Distinct();

        public int IndexOf(ProcessStepSource? step)
        {
            var node = __original_process_steps.FindLast(step);
            if (node != null)
            {
                int i = 0;
                while (node.Previous != null)
                {
                    i++;
                    node = node.Previous;
                }
                return i;
            }
            else
                return -1;
        }

        public LinkedListNode<ProcessStepSource>? NodeAt(int pos)
        {
            if (pos >= __original_process_steps.Count || pos < 0)
                return null;
            int i = 0;
            var node = __original_process_steps.First;
            while (i++ != pos)
                node = node.Next;
            return node;
        }

        public ProcessStepSource? ProcessStepAt(int pos)
        {
            if(pos >= __original_process_steps.Count || pos < 0)
                return null;
            int i = 0;
            var node = __original_process_steps.First;
            while(i++ != pos)
                node = node.Next;
            return node.Value;
        }

        public void AddProcessStepFirst(ProcessStepSource step)
        {
            SimpleStepWithTimeout_S? employ = (step as SimpleStepWithTimeout_S)?.EmployPreceding;
            if (employ != null)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The 'SimpleStepWithTimeout' that is not in the same 'Sequential' Control Block is referenced or the referenced 'SimpleStepWithTimeout' is located after the referer.");

            __original_process_steps.AddFirst(step);
            //StepFootprint += step.StepFootprint;
            //UserVariableFootprint += step.UserVariableFootprint;
            _AddGlobalEventReference(step.GlobalEventReference);
            GlobalEventPublisher?.AddEventReference(step.GlobalEventReference);
        }

        public void AddProcessStepLast(ProcessStepSource step)
        {
            SimpleStepWithTimeout_S? employ = (step as SimpleStepWithTimeout_S)?.EmployPreceding;
            if (employ != null && __original_process_steps.Find(employ) == null)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The 'SimpleStepWithTimeout' that is not in the same 'Sequential' Control Block is referenced or the referenced 'SimpleStepWithTimeout' is located after the referer.");

            __original_process_steps.AddLast(step);
            //StepFootprint += step.StepFootprint;
            //UserVariableFootprint += step.UserVariableFootprint;
            _AddGlobalEventReference(step.GlobalEventReference);
            GlobalEventPublisher?.AddEventReference(step.GlobalEventReference);
        }

        public void AddProcessStepAfter(LinkedListNode<ProcessStepSource> node, ProcessStepSource step)
        {
            if (node.List != __original_process_steps)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The specified node does not in the Linked List.");

            SimpleStepWithTimeout_S? employ = (step as SimpleStepWithTimeout_S)?.EmployPreceding;
            if (employ != null && (__original_process_steps.Find(employ) == null || (IndexOf(node.Value) < IndexOf(employ))))
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The 'SimpleStepWithTimeout' that is not in the same 'Sequential' Control Block is referenced or the referenced 'SimpleStepWithTimeout' is located after the referer.");

            __original_process_steps.AddAfter(node, step);
            //StepFootprint += step.StepFootprint;
            //UserVariableFootprint += step.UserVariableFootprint;
            _AddGlobalEventReference(step.GlobalEventReference);
            GlobalEventPublisher?.AddEventReference(step.GlobalEventReference);
        }
        public void AddProcessStepBefore(LinkedListNode<ProcessStepSource> node, ProcessStepSource step)
        {
            if (node.List != __original_process_steps)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The specified node does not in the Linked List.");

            SimpleStepWithTimeout_S? employ = (step as SimpleStepWithTimeout_S)?.EmployPreceding;
            if (employ != null && (__original_process_steps.Find(employ) == null || (IndexOf(node.Value) <= IndexOf(employ))))
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The 'SimpleStepWithTimeout' that is not in the same 'Sequential' Control Block is referenced or the referenced 'SimpleStepWithTimeout' is located after the referer.");

            __original_process_steps.AddBefore(node, step);
            //StepFootprint += step.StepFootprint;
            //UserVariableFootprint += step.UserVariableFootprint;
            _AddGlobalEventReference(step.GlobalEventReference);
            GlobalEventPublisher?.AddEventReference(step.GlobalEventReference);
        }
        public void RemoveProcessStepFirst()
        {
            if (__original_process_steps.Count == 0)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "Can not find any process step in 'Sequential' Control Block.");

            if(__original_process_steps.Any(s => IndexOf((s as SimpleStepWithTimeout_S)?.EmployPreceding) == 0))
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The 'SimpleStepWithTimeout' is referenced by another 'SimpleStepWithTimeout' located in the same 'Sequential' Control Block.");

            var step = __original_process_steps.First.Value;
            //StepFootprint -= step.StepFootprint;
            //UserVariableFootprint -= step.UserVariableFootprint;
            _RemoveGlobalEventReference(step.GlobalEventReference);
            __original_process_steps.RemoveFirst();
            GlobalEventPublisher?.RemoveEventReference(step.GlobalEventReference);
        }

        public void RemoveProcessStepLast()
        {
            if (__original_process_steps.Count == 0)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "Can not find any process step in 'Sequential' Control Block.");
            var step = __original_process_steps.Last.Value;
            //StepFootprint -= step.StepFootprint;
            //UserVariableFootprint -= step.UserVariableFootprint;
            _RemoveGlobalEventReference(step.GlobalEventReference);
            GlobalEventPublisher?.RemoveEventReference(step.GlobalEventReference);
            __original_process_steps.RemoveLast();
        }
        public void RemoveProcessStep(LinkedListNode<ProcessStepSource> node)
        {
            if (node.List != __original_process_steps)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The specified node does not in the Linked List.");

            if (__original_process_steps.Any(s => (s as SimpleStepWithTimeout_S)?.EmployPreceding == node.Value))
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The 'SimpleStepWithTimeout' is referenced by another 'SimpleStepWithTimeout' located in the same 'Sequential' Control Block.");

            var step = node.Value;
            //StepFootprint -= step.StepFootprint;
            //UserVariableFootprint -= step.UserVariableFootprint;
            _RemoveGlobalEventReference(step.GlobalEventReference);
            GlobalEventPublisher?.RemoveEventReference(step.GlobalEventReference);
            __original_process_steps.Remove(node);
        }

        public ProcessStepSource this[int idx]
        {
            set
            {
                var originalStep = ProcessStepAt(idx);
                if (originalStep == null)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"The index {idx} is invalid to a linked-list of length {__original_process_steps.Count}.");

                SimpleStepWithTimeout_S? employ = (value as SimpleStepWithTimeout_S)?.EmployPreceding;
                if (employ != null && (__original_process_steps.Find(employ) == null || idx <= IndexOf(employ)))
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The 'SimpleStepWithTimeout' that is not in the same 'Sequential' Control Block is referenced or the referenced 'SimpleStepWithTimeout' is located after the referer.");

                if (originalStep is SimpleStepWithTimeout_S)
                {
                    if (__original_process_steps.Any(s => (s as SimpleStepWithTimeout_S)?.EmployPreceding == originalStep))
                    {
                        if (value is SimpleStepWithTimeout_S)
                        {
                            foreach (var s in __original_process_steps.Where(s => (s as SimpleStepWithTimeout_S)?.EmployPreceding == originalStep))
                                (s as SimpleStepWithTimeout_S).EmployPreceding = value as SimpleStepWithTimeout_S;
                        }
                        else
                            throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The 'SimpleStepWithTimeout_S' that has been referenced by others can not be replaced with a 'SimpleStep_S'.");
                    }
                }
                //StepFootprint -= originalStep.StepFootprint;
                //UserVariableFootprint -= originalStep.UserVariableFootprint;
                _RemoveGlobalEventReference(originalStep.GlobalEventReference);
                GlobalEventPublisher?.RemoveEventReference(originalStep.GlobalEventReference);

                //StepFootprint += value.StepFootprint;
                //UserVariableFootprint += value.UserVariableFootprint;
                _AddGlobalEventReference(value.GlobalEventReference);
                GlobalEventPublisher?.AddEventReference(value.GlobalEventReference);

                var node = NodeAt(idx);
                __original_process_steps.AddBefore(node, value);
                __original_process_steps.Remove(node);

            }
            get
            {
                var step = ProcessStepAt(idx);
                if (step == null)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"The index {idx} is invalid to a linked-list of length {__original_process_steps.Count}.");
                return step;
            }
        }

        public void ReplaceProcessStepWith(LinkedListNode<ProcessStepSource> node, ProcessStepSource step)
        {
            if (node.List != __original_process_steps)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The specified node does not in the Linked List.");

            SimpleStepWithTimeout_S? employ = (step as SimpleStepWithTimeout_S)?.EmployPreceding;
            if (employ != null && (__original_process_steps.Find(employ) == null || (IndexOf(node.Value) <= IndexOf(employ))))
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The 'SimpleStepWithTimeout' that is not in the same 'Sequential' Control Block is referenced or the referenced 'SimpleStepWithTimeout' is located after the referer.");

            if (node.Value is SimpleStepWithTimeout_S)
            {
                if (__original_process_steps.Any(s =>(s as SimpleStepWithTimeout_S)?.EmployPreceding == node.Value))
                {
                    if(step is SimpleStepWithTimeout_S)
                    {
                        foreach (var s in __original_process_steps.Where(s => (s as SimpleStepWithTimeout_S)?.EmployPreceding == node.Value))
                            (s as SimpleStepWithTimeout_S).EmployPreceding = step as SimpleStepWithTimeout_S;
                    }
                    else
                        throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The 'SimpleStepWithTimeout_S' that has been referenced by others can not be replaced with a 'SimpleStep_S'.");
                }
            }
            var stp = node.Value;
            //StepFootprint -= stp.StepFootprint;
            //UserVariableFootprint -= stp.UserVariableFootprint;
            _RemoveGlobalEventReference(stp.GlobalEventReference);
            GlobalEventPublisher?.RemoveEventReference(stp.GlobalEventReference);

            //StepFootprint += step.StepFootprint;
            //UserVariableFootprint += step.UserVariableFootprint;
            _AddGlobalEventReference(step.GlobalEventReference);
            GlobalEventPublisher?.AddEventReference(step.GlobalEventReference);

            __original_process_steps.AddBefore(node, step);
            __original_process_steps.Remove(node);
        }

        public void MoveAfter(LinkedListNode<ProcessStepSource> source, LinkedListNode<ProcessStepSource> target)
        {
            if (source.List != __original_process_steps || target.List != __original_process_steps)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The specified node does not in the Linked List.");
            if(source == target)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The source node and target node cannot be the same node.");
            
            int sourcePos = IndexOf(source.Value);
            int targetPos = IndexOf(target.Value);

            if (sourcePos < targetPos)
            {
                var employs = __original_process_steps.Where(x => (x as SimpleStepWithTimeout_S)?.EmployPreceding == source.Value);
                if (employs.Any(x => IndexOf(x) <= targetPos))
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The source node has been referenced by some nodes which are located between source node and target node(include target node).");
            }
            else
            {
                var employ = (source.Value as SimpleStepWithTimeout_S)?.EmployPreceding;
                if (employ != null && IndexOf(employ) < targetPos)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The 'SimpleStepWithTimeout_S' referenced by source node is located between source node and target node(exclude target node).");
            }

            __original_process_steps.Remove(source);
            __original_process_steps.AddAfter(target, source);
        }

        public void MoveBefore(LinkedListNode<ProcessStepSource> source, LinkedListNode<ProcessStepSource> target)
        {
            if (source.List != __original_process_steps || target.List != __original_process_steps)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The specified node does not in the Linked List.");

            if (source == target)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The source node and target node cannot be the same node.");

            int sourcePos = IndexOf(source.Value);
            int targetPos = IndexOf(target.Value);

            if (sourcePos < targetPos)
            {
                var employs = __original_process_steps.Where(x => (x as SimpleStepWithTimeout_S)?.EmployPreceding == source.Value);
                if (employs.Any(x => IndexOf(x) < targetPos))
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The source node has been referenced by some nodes which are located between source node and target node(exclude target node).");
            }
            else
            {
                var employ = (source.Value as SimpleStepWithTimeout_S)?.EmployPreceding;
                if (employ != null && IndexOf(employ) <= targetPos)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The 'SimpleStepWithTimeout_S' referenced by source node is located between source node and target node(include target node).");
            }

            __original_process_steps.Remove(source);
            __original_process_steps.AddBefore(target, source);
        }

        public void ClearProcessStep()
        {
            //StepFootprint = 0;
            //UserVariableFootprint = 0;
            GlobalEventPublisher?.RemoveEventReference(__original_process_steps.SelectMany(x => x.GlobalEventReference));
            __global_event_reference.Clear();
            __original_process_steps.Clear();
        }


        public override ControlBlockObject ResolveTarget(uint next,uint abort, Context context, IReadOnlyDictionary<uint, Event> globals, ReadOnlyMemory<uint> stepLinkMapping, ReadOnlyMemory<uint> userVariableMapping, Dictionary<uint, string> stepNameMapping)
        {
            if (__original_process_steps.Count == 0)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, "Can not find any process step in 'Sequential' Control Block.");

            var compiledProcessSteps = new LinkedList<ProcessStepObject>();
            var step = __original_process_steps.Last;
            int st0 = StepFootprint - step.Value.StepFootprint, st1 = UserVariableFootprint - step.Value.UserVariableFootprint;

            TimeToTimeout.Clear();

            compiledProcessSteps.AddFirst(step.Value.ResolveTarget(next, abort, context, globals, stepLinkMapping.Slice(st0, step.Value.StepFootprint), userVariableMapping.Slice(st1, step.Value.UserVariableFootprint), this, stepNameMapping));

            while (step.Previous != null)
            {
                step = step.Previous;
                st0 = st0 - step.Value.StepFootprint;
                st1 = st1 - step.Value.UserVariableFootprint;
                if(compiledProcessSteps.First.Value.ID == null)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The next step ID is unresolved.");
                compiledProcessSteps.AddFirst(step.Value.ResolveTarget(compiledProcessSteps.First.Value.ID.Value, abort,//step.Next.Value.ID.Value,
                                                context, globals,
                                                stepLinkMapping.Slice(st0, step.Value.StepFootprint),
                                                userVariableMapping.Slice(st1, step.Value.UserVariableFootprint), this, stepNameMapping));
            }

            return new Sequential_O(Name, compiledProcessSteps, TimeToTimeout, StepFootprint, UserVariableFootprint);
        }

        public override IEnumerable<uint> GlobalEventReference => __global_event_reference.Keys;

        public override int Height => 1;

        protected void _AddGlobalEventReference(IEnumerable<uint> idxes)
        {
            foreach (var index in idxes)
            {
                if (__global_event_reference.ContainsKey(index))
                    __global_event_reference[index]++;
                else
                    __global_event_reference[index] = 1;
            }
        }
        protected void _AddGlobalEventReference(JsonArray? conditions)
        {
            _AddGlobalEventReference(ProcessStep.SearchGlobalEventIndex(conditions));
        }

        protected void _RemoveGlobalEventReference(IEnumerable<uint> idxes)
        {
            foreach (var index in idxes)
            {
                if (__global_event_reference.ContainsKey(index))
                {
                    __global_event_reference[index]--;
                    if(__global_event_reference[index] == 0)
                        __global_event_reference.Remove(index);
                }
            }
        }

        protected void _RemoveGlobalEventReference(JsonArray? conditions)
        {
            _RemoveGlobalEventReference(ProcessStep.SearchGlobalEventIndex(conditions));
        }

        public override JsonObject SaveAsJson()
        {
            JsonObject node = new JsonObject();
            node["ASSEMBLY"] = this.GetType().FullName;
            node["NAME"] = Name;
            JsonArray stepsNode = new JsonArray();
            node["STEPS"] = stepsNode;
            foreach (var step in __original_process_steps)
                stepsNode.Add(step.SaveAsJson(this));

            return node;
        }

        public override bool ContainsGlobalEventReference(uint index)
        {
            return __global_event_reference.ContainsKey(index);
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder($"{Name}({typeof(Sequential_S).FullName})");
            var step = __original_process_steps.First;
            int i = 0;
            while(step != null)
            {
                sb.Append($"\n[{i}]");
                var lines = step.Value.ToString(this).Split('\n').Skip(1);
                foreach (var line in lines.SkipLast(1))
                    sb.Append($"\n\x20\u2503\t{line}");
                sb.Append($"\n\x20\u2193\t{lines.Last()}");
                i++;
                step = step.Next;
            }
            return sb.ToString();
        }
    }

    public class Sequential_O: ControlBlockObject
    {
        public Dictionary<SimpleStepWithTimeout_S, Expression> TimeToTimeout = new Dictionary<SimpleStepWithTimeout_S, Expression>();
        private LinkedList<ProcessStepObject> __compiled_process_steps;
        private IEnumerable<ProcessStepObject> CompiledProcessSteps { get { return __compiled_process_steps; } }

        public override int StepFootprint => __compiled_process_steps.Sum(x => x.StepFootprint);

        public override int UserVariableFootprint => __compiled_process_steps.Sum(x => x.UserVariableFootprint);

        internal Sequential_O(string name, IEnumerable<ProcessStepObject> steps, IReadOnlyDictionary<SimpleStepWithTimeout_S, Expression> timeToTimeout, int stepFootprint, int userVariableFootprint): base(name)
        {
            __compiled_process_steps = new LinkedList<ProcessStepObject>(steps);
            ID = __compiled_process_steps.First.Value.ID;
            TimeToTimeout = new Dictionary<SimpleStepWithTimeout_S, Expression>(timeToTimeout);
            //StepFootprint = stepFootprint;
            //UserVariableFootprint = userVariableFootprint;
            //Level = level;
        }

        public override IEnumerable<Step> Build(Context context, IReadOnlyDictionary<uint, Event> globals)
        {
            int i = 0;
            foreach (var step in __compiled_process_steps)
            {
                Step ret;
                try
                {
                    ret = step.Build(context, globals, this);
                }
                catch (NaposhtimException e)
                {
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_STEP_BUILD_ERROR,
                        $"Can not build Step in Sequential({step.Name} @{i}/{__compiled_process_steps.Count}).", e);
                }
                yield return ret;
                i++;
            }
        }
    }
}
