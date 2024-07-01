using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.StepMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim
{
    public abstract class ControlBlockModel: Component
    {
        public ControlBlockSource ControlBlock { get; }
        public ControlBlockModelCollection Manager { get; }

        public ControlBlockModel(ControlBlockModelCollection collection, ControlBlockSource blk) {
            ControlBlock = blk;
            __name = blk.Name;
            Manager = collection;
        }

        public override string Header => $"{Name}";

        private string __name;
        public string Name
        {
            get { return __name; }
            set
            {
                value = value.Trim();
                if (__name != value)
                {
                    __name = value;
                    _notify_property_changed();
                    _reload_property("Header");
                    _reload_property("Summary");
                }
            }
        }

        public override BitmapImage ImageIcon => throw new NotSupportedException();

        public abstract string Type { get; }


        public abstract ControlBlockSource ExportToControlBlockSource();

        protected override void RollbackChanges()
        {
            throw new NotSupportedException();
        }

        public override void ApplyChanges()
        {
            Manager.IsDirty = true;
            base.ApplyChanges();
        }

        public static ControlBlockModel MAKE_CONTROL_BLOCK(ControlBlockModelCollection collection, ControlBlockSource blk, ControlBlockModel? owner) 
        {
            if (blk is Sequential_S)
                return new SequentialModel(collection, blk as Sequential_S) { Owner = owner };
            else if (blk is Loop_S)
                return new LoopModel(collection, blk as Loop_S) { Owner = owner };
            else if (blk is Switch_S)
                return new SwitchModel(collection, blk as Switch_S) { Owner = owner };
            else
                throw new ArgumentException($"Unsupported control block object was given:\n{blk.ToString()}");
        }
    }
}
