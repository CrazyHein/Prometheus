using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim.TriggerMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.ShaderMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.StepMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.IOUtility;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Protocol;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock.Process;
using Spire.Xls.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe
{
    [Flags]
    public enum RECIPE_AREA_T:byte
    {
        GLOBAL_EVENTS = 0x01,
        CONTROL_STEPS = 0x02,
        BACKGROUND_PROCESSING = 0x40,
        EXCEPTION_PROCESSING = 0x80
    }
    public class RecipeDocument
    {
        private List<Step>? __compiled_control_steps;

        private LinkedList<ControlBlock.ControlBlockSource> __control_block_list = new LinkedList<ControlBlock.ControlBlockSource> ();
        public IEnumerable<ControlBlock.ControlBlockSource> ControlBlocks { get { return __control_block_list; } }
        public IEnumerable<(string, Step)>? CompiledControlSteps => __compiled_control_steps?.Select(x => (__compiled_control_step_names[x.ID], x));

        private GlobalEvents __global_events;
        private Context __context;
        public IReadOnlyDictionary<uint, Event> GlobalEvents { get { return __global_events.Events; } }
        public IReadOnlyDictionary<uint, string> GlobalEventNames { get { return __global_events.Names; } }

        public IEnumerable<(uint, string, Event)> Globals => GlobalEvents.Select(x => (x.Key, GlobalEventNames[x.Key], x.Value));

        public int? StepFootprint { get; private set; }
        public int? UserVariablesFootprint { get; private set; }

        private Dictionary<uint, string>? __compiled_control_step_names;
        public IReadOnlyDictionary<uint, string>? CompiledControlStepNames => __compiled_control_step_names;

        public RecipeDocument()
        {
            __context = new Context();
            __global_events = new GlobalEvents();
        }

        public RecipeDocument(string path)
        {
            __global_events = new GlobalEvents();
            __context = new Context();
            ParseSource(path);
        }

        public void AddGlobalEvent(uint idx, string name, string type, params (string pname, string pvalue)[]? parameters)
        {
            __global_events.AddEvent(idx, name, type, parameters);
        }

        public void ReplaceGlobalEvent(uint idx, string name, string type, params (string pname, string pvalue)[]? parameters)
        {
            __global_events.ReplaceEvent(idx, name, type, parameters);
        }

        public void ReplaceGlobalEvent(uint idx, uint nidx, string name, string type, params (string pname, string pvalue)[]? parameters)
        {
            __global_events.ReplaceEvent(idx, nidx, name, type, parameters);
        }

        public void ReplaceGlobalEvent(uint idx, string name, Event evt)
        {
            __global_events.ReplaceEvent(idx, name, evt);
        }

        public void ReplaceGlobalEvent(uint idx, uint nidx, string name, Event evt)
        {
            __global_events.ReplaceEvent(idx, nidx, name, evt);
        }

        public void RemoveGlobalEvent(uint idx)
        {
            //if(__control_block_list.Any(x => x.ContainsGlobalEventReference(idx)))
                //throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"GEVENT with index({idx}) has been referenced elsewhere and cannot be deleted directly.");
            __global_events.RemoveEvent(idx);
        }

        public void RemoveAllGlobalEvents()
        {
            //if(__control_block_list.SelectMany(x => x.GlobalEventReference).Distinct().Any(x => GlobalEvents.ContainsKey(x)))
                //throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"At least one GEVENT has been referenced elsewhere and cannot be deleted directly.");
            __global_events.RemoveAllGlobalEvents();
        }

        public LinkedListNode<ControlBlock.ControlBlockSource>? ControlBlockNodeAt(int pos)
        {
            if (pos >= __control_block_list.Count || pos < 0)
                return null;
            int i = 0;
            var node = __control_block_list.First;
            while (i++ != pos)
                node = node.Next;
            return node;
        }

        public void AddControlBlockFirst(ControlBlock.ControlBlockSource blk)
        {
            //if(blk.GlobalEventPublisher != null && blk.GlobalEventPublisher != __global_events)
            //throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"The ControlBlock has been linked another recipe document object.");
            //if (blk.GlobalEventPublisher == __global_events || __control_block_list.Contains(blk))
            //throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"Try to add duplicated ControlBlock.");

            //__global_events.AddEventReference(blk.GlobalEventReference);
            if (blk.Owner != null)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_ARGUMENTS, $"The Control Block already has an owner.");
            blk.GlobalEventPublisher = __global_events;
            __control_block_list.AddFirst(blk);
        }

        public void AddControlBlockLast(ControlBlock.ControlBlockSource blk)
        {
            //if (blk.GlobalEventPublisher != null && blk.GlobalEventPublisher != __global_events)
            //throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"The ControlBlock has been linked another recipe document object.");
            //if (blk.GlobalEventPublisher == __global_events || __control_block_list.Contains(blk))
            //throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"Try to add duplicated ControlBlock.");

            //__global_events.AddEventReference(blk.GlobalEventReference);
            if (blk.Owner != null)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_ARGUMENTS, $"The Control Block already has an owner.");
            blk.GlobalEventPublisher = __global_events;
            __control_block_list.AddLast(blk);
        }

        public void AddControlBlockAfter(LinkedListNode<ControlBlock.ControlBlockSource> node, ControlBlock.ControlBlockSource blk)
        {
            //if (blk.GlobalEventPublisher != null && blk.GlobalEventPublisher != __global_events)
                //throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"The ControlBlock has been linked another recipe document object.");
            //if (blk.GlobalEventPublisher == __global_events || __control_block_list.Contains(blk))
                //throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"Try to add duplicated ControlBlock.");
            if(node.List != __control_block_list)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_ARGUMENTS, $"The specified node does not in the Linked List.");
            if (blk.Owner != null)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_ARGUMENTS, $"The Control Block already has an owner.");
            //__global_events.AddEventReference(blk.GlobalEventReference);
            blk.GlobalEventPublisher = __global_events;
            __control_block_list.AddAfter(node, blk);
        }

        public void AddControlBlockBefore(LinkedListNode<ControlBlock.ControlBlockSource> node, ControlBlock.ControlBlockSource blk)
        {
            //if (blk.GlobalEventPublisher != null && blk.GlobalEventPublisher != __global_events)
                //throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"The ControlBlock has been linked another recipe document object.");
            //if (blk.GlobalEventPublisher == __global_events || __control_block_list.Contains(blk))
                //throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"Try to add duplicated ControlBlock.");
            if (node.List != __control_block_list)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_ARGUMENTS, $"The specified node does not in the Linked List.");
            if (blk.Owner != null)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_ARGUMENTS, $"The Control Block already has an owner.");

            //__global_events.AddEventReference(blk.GlobalEventReference);
            blk.GlobalEventPublisher = __global_events;
            __control_block_list.AddBefore(node, blk);
        }

        public void ClearControlBlock()
        {
            foreach(var blk in __control_block_list)
                blk.GlobalEventPublisher = null;
            //__global_events.RemoveEventReference(blk.GlobalEventReference);
            __control_block_list.Clear();
        }

        public void RemoveControlBlockFirst()
        {
            if(__control_block_list.Count == 0)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"Can not find any ControlBlock in recipe document.");
            var blk = __control_block_list.First.Value;
            blk.GlobalEventPublisher = null;
            //__global_events.RemoveEventReference(blk.GlobalEventReference);
            __control_block_list.RemoveFirst();
        }

        public void RemoveControlBlockLast()
        {
            if (__control_block_list.Count == 0)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"Can not find any ControlBlock in recipe document.");
            var blk = __control_block_list.Last.Value;
            blk.GlobalEventPublisher = null;
            //__global_events.RemoveEventReference(blk.GlobalEventReference);
            __control_block_list.RemoveLast();
        }

        public void RemoveControlBlock(LinkedListNode<ControlBlock.ControlBlockSource> node)
        {
            if (node.List != __control_block_list)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"The specified node does not in the Linked List.");
            var blk = node.Value;
            blk.GlobalEventPublisher = null;
            //__global_events.RemoveEventReference(blk.GlobalEventReference);
            __control_block_list.Remove(node);
        }

        public ProcessShaders MakeShaders(IEnumerable<(string name, string lvalue, string rvalue)> shaders)
        {
            return new UserProcessShaders(shaders); 
        }

        /*
        public (IReadOnlyDictionary<uint, (string, Event)> locals, JsonArray[] conditions) MakeTriggers(Dictionary<uint, (string, Event)> locals, ref uint inlineEventIndex, params IEnumerable<string>[] conditions)
        {
            JsonArray[] conditionsArray = new JsonArray[conditions.Length];
            int counter = 0;
            foreach(var condition in conditions) 
            {
                JsonArray jsonTriggerCondition = new JsonArray();
                foreach(var line in condition)
                    jsonTriggerCondition.Add(line);
                Trigger trigger = new Trigger(jsonTriggerCondition, __global_events.Events, locals, ref inlineEventIndex);
                conditionsArray[counter++] = trigger.ToJson().AsArray();
            }
            return (locals, conditionsArray);
        }
        */

        public void BuildSteps()
        {
            if(__control_block_list.Count == 0)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_ARGUMENTS, "Can not find any control block in recipe document.");

            //if (__control_block_list.Any(x => x.Level >= ControlBlock.ControlBlockSource.MAX_NESTING_DEPTH))
            //throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_ARGUMENTS, $"The nesting depth of the Control Block exceeds the limit(MAX: {ControlBlock.ControlBlockSource.MAX_NESTING_DEPTH}).");
            SortedSet<uint> shaderLeftUserVariablesUsage = new SortedSet<uint>(__control_block_list.SelectMany(x => x.ShaderUserVariablesUsage));
            StepFootprint = __control_block_list.Sum(x => x.StepFootprint);
            UserVariablesFootprint = __control_block_list.Sum(x => x.UserVariableFootprint);
            //if(stepFootprint > __context.StepCapacity)
                //throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_ARGUMENTS, $"The number recipe steps required({stepFootprint}) is out of range(MAX: {__context.StepCapacity}).");
            //if (userVariableFootprint > __context.ReservedUserVariableCapacity)
            //throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_ARGUMENTS, $"The number recipe user variables required({userVariableFootprint}) is out of range(MAX: {__context.ReservedUserVariableCapacity}).");
            //if (userVariableFootprint > userVariablesUsageLowerLimit)
                //throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_ARGUMENTS, $"The number recipe user variables required({userVariableFootprint}) is out of range(MAX: {userVariablesUsageLowerLimit}).");
            ReadOnlyMemory<uint> stepalloc = Enumerable.Range(0, StepFootprint.Value).Select(e => (uint)e).ToArray();


            ReadOnlyMemory<uint> varalloc = Enumerable.Range(0, Context.UserVariableCapacity).Select(e => (uint)e).Where(x => shaderLeftUserVariablesUsage.Contains(x) == false).Take(UserVariablesFootprint.Value).ToArray();
            if(varalloc.Length != UserVariablesFootprint)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_ARGUMENTS, $"The number recipe user variables required({UserVariablesFootprint}) is out of range(The number of user variables that can be used: {varalloc.Length}).");

            __compiled_control_step_names = new Dictionary<uint, string>();
            var compiledBlocks = new LinkedList<ControlBlockObject>();
            var blk = __control_block_list.Last;
            int st0 = StepFootprint.Value - blk.Value.StepFootprint, st1 = UserVariablesFootprint.Value - blk.Value.UserVariableFootprint;
            compiledBlocks.AddFirst(blk.Value.ResolveTarget((uint)StepFootprint, __context, GlobalEvents, stepalloc.Slice(st0, blk.Value.StepFootprint), varalloc.Slice(st1, blk.Value.UserVariableFootprint), __compiled_control_step_names));
            while(blk.Previous != null)
            {
                blk = blk.Previous;
                st0 = st0 - blk.Value.StepFootprint;
                st1 = st1 - blk.Value.UserVariableFootprint;
                if (compiledBlocks.First.Value.ID == null)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, "The next Control Block ID is unresolved.");
                compiledBlocks.AddFirst(blk.Value.ResolveTarget(compiledBlocks.First.Value.ID.Value, __context, GlobalEvents, stepalloc.Slice(st0, blk.Value.StepFootprint), varalloc.Slice(st1, blk.Value.UserVariableFootprint), __compiled_control_step_names));
            }

            int cblkIdx = 0;
            __compiled_control_steps = new List<Step>();
            foreach (var cblk in compiledBlocks)
            {
                try
                {
                    __compiled_control_steps.AddRange(cblk.Build(__context, GlobalEvents));
                }
                catch (NaposhtimException ex)
                {
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_CONTROL_BLK_BUILD_ERROR,
                            $"Can not build Control Block in Recipe Document({cblk.Name} @{cblkIdx}/{compiledBlocks.Count}).", ex);
                }
                cblkIdx++;
            }
            //return __compiled_control_steps;
        }

        /*
        public void Download(RECIPE_AREA_T area, string ip, ushort port = 8367, int sendTimeout = 5000, int recvTimeout = 5000)
        {
            if(__compiled_control_steps == null)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, "Please build the recipe steps first.");
            using (TCP io = new TCP(new IPEndPoint(IPAddress.Any, 0), new IPEndPoint(IPAddress.Parse(ip), port), sendTimeout, recvTimeout))
            {
                DataPackager packager = new DataPackager(io, null);
                Master master = new Master(packager, null);
                io.Connect();

                master.Info(out var version, out var stepCapacity, out var throughput);
                if(stepCapacity < StepFootprint + 1)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"The step capacity read from controller is {stepCapacity}, but the recipe required steps capacity is {StepFootprint + 1}.");
                if ((area & RECIPE_AREA_T.CONTROL_STEPS) != 0 && __compiled_control_steps == null)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, "Can not find any compiled step. The download operation(CONTROL_STEPS) cannot be performed.");

                if ((area & RECIPE_AREA_T.GLOBAL_EVENTS) != 0)
                {
                    master.Clear(RECIPE_SEGMENT_T.GLOBAL_EVENT, 1, 0);
                    foreach (var evt in __global_events.Events)
                        master.DownloadGlobalEvent(evt.Key, evt.Value);
                }

                if ((area & RECIPE_AREA_T.CONTROL_STEPS) != 0)
                {
                    master.Clear(RECIPE_SEGMENT_T.STEP, 1, 0);
                    foreach (var step in __compiled_control_steps)
                        master.Download(RECIPE_SEGMENT_T.STEP, Encoding.ASCII.GetBytes(step.ToJson().ToJsonString() + '\0'));
                }
            }
        }
        */

        public static void Download(IEnumerable<(uint, string, Event)> globals, IEnumerable<Step> steps,
            string ip, ushort port = 8367, int sendTimeout = 5000, int recvTimeout = 5000)
        {
            using (TCP io = new TCP(new IPEndPoint(IPAddress.Any, 0), new IPEndPoint(IPAddress.Parse(ip), port), sendTimeout, recvTimeout))
            {
                DataPackager packager = new DataPackager(io, null);
                Master master = new Master(packager, null);
                io.Connect();

                master.Info(out var version, out var stepCapacity, out var throughput);

                if (steps.Count() != 0)
                {
                    uint stepCapacityRequired = steps.Select(x => Math.Max(x.ID, x.Branches.Max(b => b.Target))).Max() + 1;
                    if (stepCapacity < stepCapacityRequired)
                        throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"The step capacity read from controller is {stepCapacity}, but the recipe required steps capacity is {stepCapacityRequired}.");
                }
                master.Clear(RECIPE_SEGMENT_T.GLOBAL_EVENT, 1, 0);
                foreach (var evt in globals)
                    master.DownloadGlobalEvent(evt.Item1, evt.Item3);

                master.Clear(RECIPE_SEGMENT_T.STEP, 1, 0);
                foreach (var step in steps)
                    master.Download(RECIPE_SEGMENT_T.STEP, Encoding.ASCII.GetBytes(step.ToJson().ToJsonString() + '\0'));
            }
        }
        
        public static async Task DownloadAsync(IEnumerable<(uint, string, Event)> globals, IEnumerable<Step> steps,
            string ip, ushort port = 8367, int sendTimeout = 5000, int recvTimeout = 5000)
        {
            await Task.Run(() => Download(globals, steps, ip, port, sendTimeout, recvTimeout));
        }

        public void SaveAs(string path, IEnumerable<uint> globalIndexes)
        {
            var options = new JsonWriterOptions { Indented = false };
            using (var stream = File.Create(path))
            using (var writer = new Utf8JsonWriter(stream, options))
            {
                writer.WriteStartObject();

                writer.WritePropertyName("GLOBAL_EVENTS");
                writer.WriteStartArray();
                foreach(var idx in globalIndexes)
                    writer.WriteRawValue(__global_events.ToJson(idx).ToJsonString());
                writer.WriteEndArray();

                writer.WritePropertyName("CONTROL_BLOCKS");
                writer.WriteStartArray();
                foreach (var blk in ControlBlocks)
                    writer.WriteRawValue(blk.SaveAsJson().ToJsonString());
                writer.WriteEndArray();

                writer.WriteEndObject();
            }
        }

        public void ParseSource(string path)
        {
            Dictionary<uint, (string, Event)> globalEvents = new Dictionary<uint, (string, Event)>();
            LinkedList<ControlBlock.ControlBlockSource> controlBlocks = new LinkedList<ControlBlock.ControlBlockSource>();
            JsonObject root = JsonNode.Parse(File.OpenRead(path)).AsObject();

            if (root.TryGetPropertyValue("GLOBAL_EVENTS", out var globalEventsNode) && globalEventsNode.GetValueKind() == JsonValueKind.Array)
            {
                foreach (var globalEventNode in globalEventsNode.AsArray())
                    globalEvents.Add(globalEventNode["ID"].GetValue<uint>(), (globalEventNode["NAME"].GetValue<string>(), Event.MAKE(globalEventNode["EVENT"].AsObject())));
            }
            if (root.TryGetPropertyValue("CONTROL_BLOCKS", out var controlBlocksNode) && controlBlocksNode.GetValueKind() == JsonValueKind.Array)
            {
                foreach (var controlBlockNode in controlBlocksNode.AsArray())
                    controlBlocks.AddLast(ControlBlock.ControlBlockSource.MAKE_BLK(controlBlockNode.AsObject(), null));
            }
            ClearControlBlock();
            __global_events.RemoveAllGlobalEvents();
            foreach (var e in globalEvents)
                __global_events.AddEvent(e.Key, e.Value.Item1, e.Value.Item2);
            foreach(var blk in controlBlocks)
                AddControlBlockLast(blk);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(this.GetType().FullName);
            if(GlobalEvents.Count > 0)
            {
                sb.Append("\nGlobal Event(s):");
                foreach (var g in GlobalEvents)
                    sb.Append($"\n\t{g.Key:D10} {GlobalEventNames[g.Key]}: {g.Value.ToJson().ToJsonString()}");
            }
            int blkIdx = 0;
            if(__control_block_list.Count > 0)
            {
                sb.Append("\nControl Block(s):");
                foreach (var blk in __control_block_list)
                {
                    var lines = blk.ToString().Split('\n');
                    sb.Append($"\n{blkIdx:D10}:");
                    foreach (var line in lines)
                        sb.Append("\n\t").Append(line);
                    blkIdx++;
                    //sb.Append("\n\u2193");
                }
            }
            return sb.ToString();
        }

        public static void SaveScript(string path, IEnumerable<(uint, string, Event)> globalEvents, IEnumerable<(string, Prometheus.Napishtim.Engine.StepMechansim.Step)> steps)
        {
            var options = new JsonWriterOptions { Indented = false };
            using (var stream = File.Create(path))
            using (var writer = new Utf8JsonWriter(stream, options))
            {
                writer.WriteStartObject();

                writer.WritePropertyName("GLOBAL_EVENTS");
                writer.WriteStartArray();
                foreach (var global in globalEvents)
                {
                    writer.WriteStartObject();
                    writer.WriteNumber("ID", global.Item1);
                    writer.WriteString("NAME", global.Item2);
                    writer.WritePropertyName("EVENT");
                    writer.WriteRawValue(global.Item3.ToJson().ToJsonString());
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();

                writer.WritePropertyName("STEPS");
                writer.WriteStartArray();
                foreach (var stp in steps)
                {
                    writer.WriteStartObject();
                    writer.WriteString("NAME", stp.Item1);
                    writer.WritePropertyName("STEP");
                    writer.WriteRawValue(stp.Item2.ToJson().ToJsonString());
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();

                writer.WriteEndObject();
            }
        }

        public static (IEnumerable<(uint, string, Event)> globalEvents, IEnumerable<(string, Prometheus.Napishtim.Engine.StepMechansim.Step)> steps) ParseScript(string path)
        {
            List<(uint idx, string name, Event evt)> globals = new List<(uint idx, string name, Event evt)>();
            List<(string name, Prometheus.Napishtim.Engine.StepMechansim.Step stp)> steps = new List<(string name, Step stp)>();

            JsonObject root = JsonNode.Parse(File.OpenRead(path)).AsObject();

            if (root.TryGetPropertyValue("GLOBAL_EVENTS", out var globalNodes) && globalNodes.GetValueKind() == JsonValueKind.Array)
            {
                foreach (var globalNode in globalNodes.AsArray())
                {
                    uint idx = globalNode["ID"].GetValue<uint>();
                    string name = globalNode["NAME"].GetValue<string>();
                    Event evt = Event.MAKE(globalNode["EVENT"]);
                    globals.Add((idx, name, evt));
                }
            }

            Dictionary<uint, Event> globalEvents = new Dictionary<uint, Event>(globals.Select(x => KeyValuePair.Create(x.idx, x.evt)));
            if (root.TryGetPropertyValue("STEPS", out var stepNodes) && stepNodes.GetValueKind() == JsonValueKind.Array)
            {
                foreach (var stepNode in stepNodes.AsArray())
                {
                    string name = stepNode["NAME"].GetValue<string>();
                    uint inlineEventIndex = 10000;
                    Step stp = new Step(stepNode["STEP"], globalEvents, ref inlineEventIndex);
                    steps.Add((name, stp));
                }
            }

            return (globals, steps);
        }
    }
}
