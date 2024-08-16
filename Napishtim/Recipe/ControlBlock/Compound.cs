using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.BranchMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.StepMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock
{
    public class Compound_S : ControlBlockSource
    {
        private LinkedList<ControlBlockSource> __original_control_blocks;
        public IEnumerable<ControlBlockSource> OriginalControlBlocks { get { return __original_control_blocks; } }

        public Compound_S(string name, IEnumerable<ControlBlockSource> blks) : base(name)
        {
            __original_control_blocks = new LinkedList<ControlBlockSource>(blks);
        }

        private Compound_S(JsonObject node) : base(node["NAME"].GetValue<string>())
        {
            __original_control_blocks = new LinkedList<ControlBlockSource>();
            foreach (var blk in node["BLOCKS"].AsArray())
            {
                ControlBlockSource st = ControlBlockSource.MAKE_BLK(blk.AsObject(), null);
                AddControlBlockLast(st);
            }
        }

        static public ControlBlockSource MAKE(JsonObject node, ControlBlockSource? owner)
        {
            try
            {
                if (node["ASSEMBLY"].GetValue<string>() != typeof(Compound_S).FullName)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, $"Assmebly name mismatch: {node["ASSEMBLY"].GetValue<string>()} vs {typeof(Compound_S).FullName}.");

                return new Compound_S(node) { Owner = owner };
            }
            catch (Exception ex)
            {
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, $"Can not restore Compound_S object from node:\n{node.ToString()}", ex);
            }
        }


        public override int Height
        {
            get
            {
                if (__original_control_blocks.Count == 0)
                    return 1;
                else
                    return __original_control_blocks.Max(x => x.Height) + 1;
            }
        }

        public override IEnumerable<uint> ShaderUserVariablesUsage => __original_control_blocks.SelectMany(x => x.ShaderUserVariablesUsage).Distinct();

        public override IEnumerable<uint> GlobalEventReference => __original_control_blocks.SelectMany(s => s.GlobalEventReference).Distinct();

        public override IEnumerable<(Sequential_S container, ProcessStepSource step)> ProcessSteps => __original_control_blocks.SelectMany(x => x.ProcessSteps);

        public override int StepFootprint => __original_control_blocks.Sum(x => x.StepFootprint);

        public override int UserVariableFootprint => __original_control_blocks.Sum(x => x.UserVariableFootprint);

        public override bool ContainsGlobalEventReference(uint index)
        {
            return __original_control_blocks.Any(x => x.ContainsGlobalEventReference(index));
        }

        public int IndexOf(ControlBlockSource? blk)
        {
            var node = __original_control_blocks.FindLast(blk);
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

        public LinkedListNode<ControlBlockSource>? NodeAt(int pos)
        {
            if (pos >= __original_control_blocks.Count || pos < 0)
                return null;
            int i = 0;
            var node = __original_control_blocks.First;
            while (i++ != pos)
                node = node.Next;
            return node;
        }

        public ControlBlockSource? ControlBlockAt(int pos)
        {
            if (pos >= __original_control_blocks.Count || pos < 0)
                return null;
            int i = 0;
            var node = __original_control_blocks.First;
            while (i++ != pos)
                node = node.Next;
            return node.Value;
        }

        public void AddControlBlockFirst(ControlBlockSource blk)
        {
            if (this.Nesting + blk.Height > MAX_NESTING_DEPTH)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"The nesting depth of a Control Block exceeds the limit(MAX: {ControlBlockSource.MAX_NESTING_DEPTH}).");
            if (blk.Owner != null)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"The Control Block already has an owner.");

            __original_control_blocks.AddFirst(blk);
            blk.Owner = this;
        }

        public void AddControlBlockLast(ControlBlockSource blk)
        {
            if (this.Nesting + blk.Height > MAX_NESTING_DEPTH)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"The nesting depth of a Control Block exceeds the limit(MAX: {ControlBlockSource.MAX_NESTING_DEPTH}).");
            if (blk.Owner != null)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"The Control Block already has an owner.");

            __original_control_blocks.AddLast(blk);
            blk.Owner = this;
        }

        public void AddControlBlockAfter(LinkedListNode<ControlBlockSource> node, ControlBlockSource blk)
        {
            if (node.List != __original_control_blocks)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The specified node does not in the Linked List.");
            if (this.Nesting + blk.Height > MAX_NESTING_DEPTH)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"The nesting depth of a Control Block exceeds the limit(MAX: {ControlBlockSource.MAX_NESTING_DEPTH}).");
            if (blk.Owner != null)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"The Control Block already has an owner.");

            __original_control_blocks.AddAfter(node, blk);
            blk.Owner = this;
        }
        public void AddControlBlockBefore(LinkedListNode<ControlBlockSource> node, ControlBlockSource blk)
        {
            if (node.List != __original_control_blocks)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The specified node does not in the Linked List.");
            if (this.Nesting + blk.Height > MAX_NESTING_DEPTH)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"The nesting depth of a Control Block exceeds the limit(MAX: {ControlBlockSource.MAX_NESTING_DEPTH}).");
            if (blk.Owner != null)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"The Control Block already has an owner.");

            __original_control_blocks.AddBefore(node, blk);
            blk.Owner = this;
        }

        public ControlBlockSource RemoveControlBlockFirst()
        {
            if (__original_control_blocks.Count == 0)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "Can not find any control block in 'Compound' Control Block.");

            var blk = __original_control_blocks.First.Value;
            __original_control_blocks.RemoveFirst();
            blk.Owner = null;
            return blk;
        }

        public ControlBlockSource RemoveControlBlockLast()
        {
            if (__original_control_blocks.Count == 0)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "Can not find any control block in 'Compound' Control Block.");

            var blk = __original_control_blocks.Last.Value;
            __original_control_blocks.RemoveLast();
            blk.Owner = null;
            return blk;
        }

        public ControlBlockSource RemoveControlBlock(LinkedListNode<ControlBlockSource> node)
        {
            if (node.List != __original_control_blocks)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The specified node does not in the Linked List.");

            __original_control_blocks.Remove(node);
            node.Value.Owner = null;
            return node.Value;
        }

        public ControlBlockSource this[int idx]
        {
            set
            {
                var originalBlk = ControlBlockAt(idx);
                if (originalBlk == null)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"The index {idx} is invalid to a linked-list of length {__original_control_blocks.Count}.");
                if (value.Owner != null)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"The Control Block already has an owner.");
                var node = NodeAt(idx);
                __original_control_blocks.AddBefore(node, value);
                __original_control_blocks.Remove(node);
                value.Owner = this;
                node.Value.Owner = null;
            }
            get
            {
                var blk = ControlBlockAt(idx);
                if (blk == null)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"The index {idx} is invalid to a linked-list of length {__original_control_blocks.Count}.");
                return blk;
            }
        }

        public ControlBlockSource ReplaceControlBlockWith(LinkedListNode<ControlBlockSource> node, ControlBlockSource blk)
        {
            if (node.List != __original_control_blocks)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The specified node does not in the Linked List.");
            if (blk.Owner != null)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, $"The Control Block already has an owner.");
            __original_control_blocks.AddBefore(node, blk);
            __original_control_blocks.Remove(node);
            blk.Owner = this;
            node.Value.Owner = null;
            return node.Value;
        }

        public void MoveAfter(LinkedListNode<ControlBlockSource> source, LinkedListNode<ControlBlockSource> target)
        {
            if (source.List != __original_control_blocks || target.List != __original_control_blocks)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The specified node does not in the Linked List.");
            if (source == target)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The source node and target node cannot be the same node.");

            __original_control_blocks.Remove(source);
            __original_control_blocks.AddAfter(target, source);
        }

        public void MoveBefore(LinkedListNode<ControlBlockSource> source, LinkedListNode<ControlBlockSource> target)
        {
            if (source.List != __original_control_blocks || target.List != __original_control_blocks)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The specified node does not in the Linked List.");
            if (source == target)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_INVALID_OPERATION, "The source node and target node cannot be the same node.");

            __original_control_blocks.Remove(source);
            __original_control_blocks.AddBefore(target, source);
        }

        public void ClearControlBlock()
        {
            foreach(var c in __original_control_blocks)
                c.Owner = null;
            __original_control_blocks.Clear();
        }

        public override ControlBlockObject ResolveTarget(uint next, uint abort, Context context, IReadOnlyDictionary<uint, Event> globals, ReadOnlyMemory<uint> stepLinkMapping, ReadOnlyMemory<uint> userVariableMapping, Dictionary<uint, string> stepNameMapping)
        {
            if (__original_control_blocks.Count == 0)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.CONTROL_BLOCK_ARGUMENTS_ERROR, $"Can not find any control block in Compound({FullName}) Control Block.");

            var compiledControlBlocks = new LinkedList<ControlBlockObject>();
            var blk = __original_control_blocks.Last;
            int st0 = StepFootprint - blk.Value.StepFootprint, st1 = UserVariableFootprint - blk.Value.UserVariableFootprint;

            compiledControlBlocks.AddFirst(blk.Value.ResolveTarget(next, abort, context, globals, stepLinkMapping.Slice(st0, blk.Value.StepFootprint), userVariableMapping.Slice(st1, blk.Value.UserVariableFootprint), stepNameMapping));

            while (blk.Previous != null)
            {
                blk = blk.Previous;
                st0 = st0 - blk.Value.StepFootprint;
                st1 = st1 - blk.Value.UserVariableFootprint;
                compiledControlBlocks.AddFirst(blk.Value.ResolveTarget(compiledControlBlocks.First.Value.ID!.Value, abort,//step.Next.Value.ID.Value,
                                                context, globals,
                                                stepLinkMapping.Slice(st0, blk.Value.StepFootprint),
                                                userVariableMapping.Slice(st1, blk.Value.UserVariableFootprint), stepNameMapping));
            }

            return new Compound_O(Name, compiledControlBlocks);
        }

        public override JsonObject SaveAsJson()
        {
            JsonObject node = new JsonObject();
            node["ASSEMBLY"] = this.GetType().FullName;
            node["NAME"] = Name;
            JsonArray blocksNode = new JsonArray();
            node["BLOCKS"] = blocksNode;
            foreach (var blk in __original_control_blocks)
                blocksNode.Add(blk.SaveAsJson());

            return node;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder($"{Name}({typeof(Compound_S).FullName})");
            var blk = __original_control_blocks.First;
            int i = 0;
            while (blk != null)
            {
                sb.Append($"\n<Sub {i:D10}>");
                var lines = blk.Value.ToString().Split('\n');
                foreach (var line in lines)
                    sb.Append($"\n\t{line}");
                sb.Append($"\n</Sub {i:D10}>");
                i++;
                blk = blk.Next;
            }
            return sb.ToString();
        }
    }

    public class Compound_O: ControlBlockObject
    {
        private LinkedList<ControlBlockObject> __compiled_control_blocks;

        internal Compound_O(string name, IEnumerable<ControlBlockObject> blks) : base(name)
        {
            __compiled_control_blocks = new LinkedList<ControlBlockObject>(blks);
            ID = __compiled_control_blocks.First.Value.ID;
        }

        public override int StepFootprint => __compiled_control_blocks.Sum(x => x.StepFootprint);

        public override int UserVariableFootprint => __compiled_control_blocks.Sum(x => x.UserVariableFootprint);

        public override IEnumerable<Step> Build(Context context, IReadOnlyDictionary<uint, Event> globals)
        {
            return __compiled_control_blocks.SelectMany(x => x.Build(context, globals));
        }

        public override IEnumerable<ProcessStepObject> ProcessSteps => __compiled_control_blocks.SelectMany(b => b.ProcessSteps);
    }
}
