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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim
{
    public class LoopModel : ControlBlockModel
    {
        private ObservableCollection<ControlBlockModel> __loop_body;
        public IEnumerable<ControlBlockModel> LoopBody { get; }

        private int __loop_count;
        public int LoopCount
        {
            get { return __loop_count;}
            set {
                if (__loop_count != value)
                {
                    __loop_count = value;
                    _notify_property_changed();
                    _reload_property("Header");
                    _reload_property("Summary");
                }
            }
        }


        public LoopModel(ControlBlockModelCollection collection, Loop_S blk) : base(collection, blk)
        {
            __loop_count = blk.LoopCount;
            if (blk.LoopBody != null)
                __loop_body = new ObservableCollection<ControlBlockModel>() { ControlBlockModel.MAKE_CONTROL_BLOCK(collection, blk.LoopBody, this) };
            else
                __loop_body = new ObservableCollection<ControlBlockModel>();
            LoopBody = __loop_body;
        }

        public override string Header => $"{Name} ({LoopCount})";
        public override BitmapImage ImageIcon => new BitmapImage(new Uri("pack://application:,,,/imgs/loop.png"));

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

        public override string Type => typeof(Loop_S).Name;

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

        public void ResetLoopBody(Type type)
        {
            if(Modified)
                throw new InvalidOperationException("Apply or discard the changes first and reset loop body.");

            ControlBlockModel blk;
            string name = __loop_body.Count() == 0? "unnamed": __loop_body[0].Name;

            if (typeof(Sequential_S) == type)
            {
                Sequential_S seq = new Sequential_S(name, Enumerable.Empty<ProcessStepSource>()) { Owner = null };
                (ControlBlock as Loop_S).LoopBody = seq;
                blk = new SequentialModel(Manager, seq) { Owner = this };
            }
            else if (typeof(Loop_S) == type)
            {
                Loop_S loop = new Loop_S(name, 1) { Owner = null };
                Sequential_S seq = new Sequential_S("unnamed", Enumerable.Empty<ProcessStepSource>()) { Owner = null };
                loop.LoopBody = seq;

                (ControlBlock as Loop_S).LoopBody = loop;
                blk = new LoopModel(Manager, loop) { Owner = this };
            }
            else if (typeof(Switch_S) == type)
            {
                Switch_S sw = new Switch_S(name) { Owner = null }; ;
                (ControlBlock as Loop_S).LoopBody = sw;
                blk = new SwitchModel(Manager, sw) { Owner = this };
            }
            else
                throw new ArgumentException($"Unrecognized control block type: \n{type.FullName}");

            __loop_body.Clear();
            __loop_body.Add(blk);

            _notify_property_changed("Summary");
            base.ApplyChanges();
        }

        public ControlBlockModel ResetLoopBody(JsonNode blkNode)
        {
            var blk_s = ControlBlockSource.MAKE_BLK(blkNode["SOURCE"].AsObject(), null);
            var blk = ControlBlockModel.MAKE_CONTROL_BLOCK(Manager, blk_s, this);

            (ControlBlock as Loop_S).LoopBody = blk_s;

            __loop_body.Clear();
            __loop_body.Add(blk);

            _notify_property_changed("Summary");
            base.ApplyChanges();

            return blk;
        }

        public void ClearLoopBody()
        {
            if (Modified)
                throw new InvalidOperationException("Apply or discard the changes first and clear loop body.");

            (ControlBlock as Loop_S).LoopBody = null;
            __loop_body.Clear();

            _notify_property_changed("Summary");
            base.ApplyChanges();
        }

        public override void ApplyChanges()
        {
            ControlBlock.Name = Name;
            (ControlBlock as Loop_S).LoopCount = LoopCount;
            base.ApplyChanges();
        }

        public override void DiscardChanges()
        {
            Name = ControlBlock.Name;
            LoopCount = (ControlBlock as Loop_S).LoopCount;
            base.DiscardChanges();
        }

        public override ControlBlockSource ExportToControlBlockSource()
        {
            if(__loop_body.Count() == 0)
                return new Loop_S(Name, LoopCount) { Owner = null };
            else
                return new Loop_S(Name, __loop_body[0].ExportToControlBlockSource(), LoopCount) { Owner = null };
        }

        public override void SubComponentChangesApplied(Component sub)
        {
            _notify_property_changed("Summary");
            ApplyChanges();
        }

        public override void SubComponentChangesHappened(Component sub)
        {
            
        }
    }
}
