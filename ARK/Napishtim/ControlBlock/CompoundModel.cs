using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression.AU;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.StepMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.Process;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim
{
    public class CompoundModel : ControlBlockModel
    {
        private ObservableCollection<ControlBlockModel> __sub_control_blocks;
        public IEnumerable<ControlBlockModel> SubControlBlocks { get; }

        public override string Type => typeof(Compound_S).Name;

        public override string Summary
        {
            get
            {
                string summary;
                try
                {
                    if (Modified)
                        summary = ExportToControlBlockSource().ToString();
                    else
                        summary = ControlBlock.ToString();
                }
                catch (Exception ex)
                {
                    summary = ex.ToString();
                }
                return summary;
            }
        }

        public override BitmapImage ImageIcon => new BitmapImage(new Uri("pack://application:,,,/imgs/compound.png"));

        public CompoundModel(ControlBlockModelCollection collection, Compound_S blk) : base(collection, blk)
        {
            __sub_control_blocks = new ObservableCollection<ControlBlockModel>();
            SubControlBlocks = __sub_control_blocks;
            foreach (var sub in blk.OriginalControlBlocks)
                __sub_control_blocks.Add(ControlBlockModel.MAKE_CONTROL_BLOCK(collection, sub, this));
        }

        public ControlBlockModel this[int i]
        {
            get { return __sub_control_blocks[i]; }
        }

        public int SubIndex(ControlBlockModel blk) => __sub_control_blocks.IndexOf(blk);

        public override ControlBlockSource ExportToControlBlockSource()
        {
            Compound_S cp = new Compound_S(Name, Enumerable.Empty<ControlBlockSource>());
            foreach (var b in SubControlBlocks)
                cp.AddControlBlockLast(b.ExportToControlBlockSource());
            return cp;
        }

        public override void SubComponentChangesApplied(Component sub)
        {
            _notify_property_changed("Summary");
            ApplyChanges();
        }

        public override void SubComponentChangesHappened(Component sub)
        {
            
        }

        public override JsonNode ToJson()
        {
            if (Modified)
                throw new InvalidOperationException("Unapplied changes detected.");
            var json = new JsonObject();
            json["VERSION"] = Settings.ArkVersion;
            json["ASSEMBLY"] = this.GetType().FullName;
            json["SOURCE"] = ControlBlock.SaveAsJson();
            return json;
        }

        public void Move(int oldIndex, int newIndex)
        {
            if(Modified == false)
            {
                int length = (ControlBlock as Compound_S).OriginalControlBlocks.Count();
                if (oldIndex >= length || oldIndex < 0)
                    throw new IndexOutOfRangeException($"The old index {oldIndex} is invalid to a linked-list of length {length}.");
                if (newIndex >= length || newIndex < 0)
                    throw new IndexOutOfRangeException($"The new index {newIndex} is invalid to a linked-list of length {length}.");

                var sourceNode = (ControlBlock as Compound_S).NodeAt(oldIndex);
                var targetNode = (ControlBlock as Compound_S).NodeAt(newIndex);
                if (oldIndex > newIndex)
                    (ControlBlock as Compound_S).MoveBefore(sourceNode, targetNode);
                else
                    (ControlBlock as Compound_S).MoveAfter(sourceNode, targetNode);

                var temp = __sub_control_blocks[oldIndex];
                __sub_control_blocks.RemoveAt(oldIndex);
                __sub_control_blocks.Insert(newIndex, temp);

                _notify_property_changed("Summary");
                ApplyChanges();
            }
            else
                throw new InvalidOperationException("Apply or discard the changes first and then adjust the order of sub-control-blocks.");
        }

        public void ResetControlBlock(int pos, Type type)
        {
            if (Modified == false)
            {
                var blk_s = (ControlBlock as Compound_S)[pos];
                var ret = __default_control_block_model(blk_s.Name, type);
                (ControlBlock as Compound_S).ReplaceControlBlockWith((ControlBlock as Compound_S).NodeAt(pos), ret.Item2);
                __sub_control_blocks[pos] = ret.Item1;
            }
            else
                throw new InvalidOperationException("Apply or discard the changes first and reset add sub-control-blocks.");
        }

        public void ResetControlBlock(int pos, JsonNode blkNode)
        {
            if (Modified == false)
            {
                ControlBlockSource blk_s = ControlBlockSource.MAKE_BLK(blkNode["SOURCE"].AsObject(), null);
                ControlBlockModel blk_m = ControlBlockModel.MAKE_CONTROL_BLOCK(Manager, blk_s, this);

                (ControlBlock as Compound_S).ReplaceControlBlockWith((ControlBlock as Compound_S).NodeAt(pos), blk_s);
                __sub_control_blocks[pos] = blk_m;
            }
            else
                throw new InvalidOperationException("Apply or discard the changes first and reset add sub-control-blocks.");
        }

        private (ControlBlockModel, ControlBlockSource) __default_control_block_model(string name, Type type)
        {
            ControlBlockSource blk_s;
            ControlBlockModel blk_m;

            if (typeof(Sequential_S) == type)
            {
                blk_s = new Sequential_S(name, Enumerable.Empty<ProcessStepSource>());
                blk_m = new SequentialModel(Manager, blk_s as Sequential_S) { Owner = this };
            }
            else if (typeof(Loop_S) == type)
            {
                blk_s = new Loop_S(name, 1);
                Sequential_S seq = new Sequential_S("sequential", Enumerable.Empty<ProcessStepSource>());
                (blk_s as Loop_S).LoopBody = seq;
                blk_m = new LoopModel(Manager, blk_s as Loop_S) { Owner = this };
            }
            else if (typeof(Switch_S) == type)
            {
                blk_s = new Switch_S(name);
                blk_m = new SwitchModel(Manager, blk_s as Switch_S) { Owner = this };
            }
            else if (typeof(Compound_S) == type)
            {
                blk_s = new Compound_S(name, Enumerable.Empty<ControlBlockSource>());
                blk_m = new CompoundModel(Manager, blk_s as Compound_S) { Owner = this };
            }
            else
                throw new ArgumentException($"Unrecognized control block type: \n{type.FullName}");

            return (blk_m, blk_s);
        }

        public ControlBlockModel Add(string name, Type type)
        {
            if (Modified == false)
            {
                var ret = __default_control_block_model(name, type);

                (ControlBlock as Compound_S).AddControlBlockLast(ret.Item2);
                __sub_control_blocks.Add(ret.Item1);

                _notify_property_changed("Summary");
                ApplyChanges();
                return ret.Item1;
            }
            else
                throw new InvalidOperationException("Apply or discard the changes first and then add sub-control-blocks.");
        }

        public ControlBlockModel Add(JsonNode blkNode)
        {
            if (Modified == false)
            {
                ControlBlockSource blk_s = ControlBlockSource.MAKE_BLK(blkNode["SOURCE"].AsObject(), null);
                ControlBlockModel blk_m = ControlBlockModel.MAKE_CONTROL_BLOCK(Manager, blk_s, this);

                (ControlBlock as Compound_S).AddControlBlockLast(blk_s);
                __sub_control_blocks.Add(blk_m);

                _notify_property_changed("Summary");
                ApplyChanges();
                return blk_m;
            }
            else
                throw new InvalidOperationException("Apply or discard the changes first and then add sub-control-blocks.");
        }

        public void Remove(ControlBlockModel blk)
        {
            if (Modified == false)
            {
                int pos = __sub_control_blocks.IndexOf(blk);
                (ControlBlock as Compound_S).RemoveControlBlock((ControlBlock as Compound_S).NodeAt(pos));
                __sub_control_blocks.RemoveAt(pos);

                _notify_property_changed("Summary");
                ApplyChanges();
            }
            else
                throw new InvalidOperationException("Apply or discard the changes first and then remove sub-control-blocks.");
        }

        public void RemoveAt(int pos)
        {
            if (Modified == false)
            {
                (ControlBlock as Compound_S).RemoveControlBlock((ControlBlock as Compound_S).NodeAt(pos));
                __sub_control_blocks.RemoveAt(pos);

                _notify_property_changed("Summary");
                ApplyChanges();
            }
            else
                throw new InvalidOperationException("Apply or discard the changes first and then remove sub-control-blocks.");
        }

        public ControlBlockModel InsertBefore(int pos, string name, Type type)
        {
            if (Modified == false)
            {
                var ret = __default_control_block_model(name, type);
                (ControlBlock as Compound_S).AddControlBlockBefore((ControlBlock as Compound_S).NodeAt(pos), ret.Item2);
                __sub_control_blocks.Insert(pos, ret.Item1);

                _notify_property_changed("Summary");
                ApplyChanges();
                return ret.Item1;
            }
            else
                throw new InvalidOperationException("Apply or discard the changes first and then insert sub-control-blocks.");
        }

        public ControlBlockModel InsertBefore(int pos, JsonNode blkNode)
        {
            if (Modified == false)
            {
                ControlBlockSource blk_s = ControlBlockSource.MAKE_BLK(blkNode["SOURCE"].AsObject(), null);
                ControlBlockModel blk_m = ControlBlockModel.MAKE_CONTROL_BLOCK(Manager, blk_s, this);

                (ControlBlock as Compound_S).AddControlBlockBefore((ControlBlock as Compound_S).NodeAt(pos), blk_s);
                __sub_control_blocks.Insert(pos, blk_m);

                _notify_property_changed("Summary");
                ApplyChanges();
                return blk_m;
            }
            else
                throw new InvalidOperationException("Apply or discard the changes first and then insert sub-control-blocks.");
        }

        public override void ApplyChanges()
        {
            ControlBlock.Name = Name;
            base.ApplyChanges();
        }

        public override void DiscardChanges()
        {
            Name = ControlBlock.Name;
            base.DiscardChanges();
        }
    }
}
