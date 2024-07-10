using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression.AU;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.Process;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim
{
    public class ControlBlockModelCollection : ComponentCollection<ControlBlockModel>
    {
        public RecipeDocument RecipeDocument { get; }
        private ObservableCollection<ControlBlockModel> __control_blocks;
        public IEnumerable<ControlBlockModel> ControlBlocks { get; }
        public override IEnumerable<ControlBlockModel> Components { get; }
        public CONTROL_BLOCK_GROUP_T Group { get; }

        public ControlBlockModelCollection(RecipeDocument doc, CONTROL_BLOCK_GROUP_T group) 
        {
            RecipeDocument = doc;
            Group = group;
            if(group == CONTROL_BLOCK_GROUP_T.REGULAR)
                __control_blocks = new ObservableCollection<ControlBlockModel>(doc.RegularControlBlocks.Select(x => ControlBlockModel.MAKE_CONTROL_BLOCK(this, x, null)));
            else
                __control_blocks = new ObservableCollection<ControlBlockModel>(doc.ExceptionHandlingBlocks.Select(x => ControlBlockModel.MAKE_CONTROL_BLOCK(this, x, null)));
            ControlBlocks = __control_blocks;
            Components = __control_blocks;
        }

        public override string Summary
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                int i = 0;
                foreach (var blk in Group == CONTROL_BLOCK_GROUP_T.REGULAR?RecipeDocument.RegularControlBlocks: RecipeDocument.ExceptionHandlingBlocks)
                {
                    sb.Append($"Control Block {i:D10}:\n");
                    foreach(var line in blk.ToString().Split('\n'))
                        sb.Append("\t").AppendLine(line);
                    sb.Append("\n");
                    i++;
                }
                return sb.ToString();
            }
        }

        public Component AddControlBlock()
        {
            Sequential_S seq = new Sequential_S("unnamed", Enumerable.Empty<ProcessStepSource>()) { Owner = null };
            RecipeDocument.AddControlBlockLast(Group, seq);

            SequentialModel seqm = new SequentialModel(this, seq) { Owner = null };
            __control_blocks.Add(seqm);
            IsDirty = true;
            return seqm;
        }

        public Component AddControlBlock(ControlBlockModel blk)
        {
            if (blk.Owner != null)
                throw new ArgumentException($"The Control Block already has an owner.");
            if (blk.Modified)
                throw new ArgumentException($"Unapplied changes detected in the specified Control Block.");

            RecipeDocument.AddControlBlockLast(Group, blk.ControlBlock);

            __control_blocks.Add(blk);
            IsDirty = true;
            return blk;
        }

        public Component? PasteControlBlock()
        {
            var blk = Component.BUILD_CONTROL_BLOCK_FROM_CLIPBOARD(this);
            if (blk != null)
            {
                RecipeDocument.AddControlBlockLast(Group, blk.ControlBlock);
                __control_blocks.Add(blk);
                IsDirty = true;
            }
            return blk;
        }

        public Component? PasteControlBlockBefore(ControlBlockModel pos)
        {
            int idx = __control_blocks.IndexOf(pos);
            var blk = Component.BUILD_CONTROL_BLOCK_FROM_CLIPBOARD(this);
            if (blk != null)
            {
                RecipeDocument.AddControlBlockBefore(Group, RecipeDocument.ControlBlockNodeAt(Group, idx), blk.ControlBlock);
                __control_blocks.Insert(idx, blk);
                IsDirty = true;
            }
            return blk;
        }

        private (ControlBlockModel, ControlBlockSource) __default_control_block_based_on_type(Type type)
        {
            ControlBlockSource blk_s = null;
            ControlBlockModel blk_m = null;
            if (typeof(Sequential_S) == type)
            {
                blk_s = new Sequential_S("unnamed", Enumerable.Empty<ProcessStepSource>()) { Owner = null };
                blk_m = new SequentialModel(this, blk_s as Sequential_S) { Owner = null };
            }
            else if (typeof(Loop_S) == type)
            {
                blk_s = new Loop_S("unnamed", 1) { Owner = null };
                Sequential_S seq = new Sequential_S("unnamed", Enumerable.Empty<ProcessStepSource>()) { Owner = null };
                (blk_s as Loop_S).LoopBody = seq;

                blk_m = new LoopModel(this, blk_s as Loop_S) { Owner = null };
            }
            else if (typeof(Switch_S) == type)
            {
                blk_s = new Switch_S("unnamed") { Owner = null }; ;
                blk_m = new SwitchModel(this, blk_s as Switch_S) { Owner = null };
            }
            else
                throw new ArgumentException($"Unrecognized control block type: \n{type.FullName}");

            return (blk_m, blk_s);
        }

        public Component AddControlBlock(Type type)
        {
            var ret = __default_control_block_based_on_type(type);

            RecipeDocument.AddControlBlockLast(Group, ret.Item2);
            __control_blocks.Add(ret.Item1);
            IsDirty = true;
            return ret.Item1;
        }

        public Component InsertControlBlockAt(int pos)
        {
            Sequential_S seq = new Sequential_S("unnamed", Enumerable.Empty<ProcessStepSource>()) { Owner = null };
            RecipeDocument.AddControlBlockBefore(Group, RecipeDocument.ControlBlockNodeAt(Group, pos), seq);

            SequentialModel seqm = new SequentialModel(this, seq) { Owner = null };
            __control_blocks.Insert(pos, seqm);
            IsDirty = true;
            return seqm;
        }

        public Component InsertControlBlockAt(int pos, Type type)
        {
            var ret = __default_control_block_based_on_type(type);
            RecipeDocument.AddControlBlockBefore(Group, RecipeDocument.ControlBlockNodeAt(Group, pos), ret.Item2);
            __control_blocks.Insert(pos, ret.Item1);
            IsDirty = true;
            return ret.Item1;
        }

        public void RemoveControlBlockAt(int pos)
        {
            RecipeDocument.RemoveControlBlock(Group, RecipeDocument.ControlBlockNodeAt(Group, pos));
            __control_blocks.RemoveAt(pos);
            IsDirty = true;
        }

        public void RemoveControlBlock(ControlBlockModel blk)
        {
            int pos = __control_blocks.IndexOf(blk);
            RemoveControlBlockAt(pos);
        }
    }
}
