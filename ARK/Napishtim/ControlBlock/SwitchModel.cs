using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.BranchMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.StepMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.Globals;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.Process;
using Microsoft.VisualBasic;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim
{
    public class SwitchBranchModel: Component
    {
        public override void SubComponentChangesApplied(Component sub)
        {
            throw new NotSupportedException();
        }

        public override void SubComponentChangesHappened(Component sub)
        {
            throw new NotSupportedException();
        }

        public override JsonNode ToJson()
        {
            //if (Modified)
            //throw new InvalidOperationException("Unapplied changes detected.");
            int pos = (Owner as SwitchModel).Branches.IndexOf(this);
            var act = (Owner as SwitchModel).BranchActions.ElementAt(pos);
            JsonObject o = new JsonObject();
            o["VERSION"] = Settings.ArkVersion;
            o["ASSEMBLY"] = this.GetType().FullName;
            o["NAME"] = Name;
            o["TRIGGER"] = ConditionArray;
            o["ACTION"] = act.ToJson();
            return o;
        }

        protected override void RollbackChanges()
        {
            throw new NotSupportedException();
        }

        private string __name = "unnamed";
        public string Name 
        { 
            get{ return __name; }
            set
            {
                value = value.Trim();
                if(value != __name)
                {
                    __name = value;
                    _notify_property_changed();
                    _reload_property("Header");
                }
            } 
        }

        private string __condition = "";
        public string Condition
        {
            get { return __condition; }
            set
            {
                value = value.Trim();
                if (value != __condition)
                {
                    __condition = value;
                    _notify_property_changed();
                    _reload_property("ConditionArray");
                }
            }
        }

        public JsonArray ConditionArray
        {
            get 
            {
                if (__condition.Length == 0)
                    return new JsonArray();
                return new JsonArray(__condition.Split('\n').Select(x => JsonValue.Create<string>(x.TrimEnd())).ToArray());
            }
        }


        public override string Header { get { return Name; } }

        public override string Summary
        {
            get
            {
                string summary;
                try
                {
                    summary = $"{Name}: ({Type}){ControlBlock}";
                }
                catch (Exception ex)
                {
                    summary = ex.ToString();
                }
                return summary;
            }
        }

        public override BitmapImage ImageIcon => throw new NotSupportedException();

        public SwitchBranchModel(string name, JsonArray condition)
        {
            Name = name;
            __condition = string.Join('\n', condition.AsArray().Select(x => x.GetValue<string>()));
        }

        public string ControlBlock
        {
            get 
            {
                int pos = (Owner as SwitchModel).Branches.IndexOf(this);
                var act = (Owner as SwitchModel).BranchActions.ElementAt(pos);
                return act.Name;
            }
        }

        public string Type
        {
            get
            {
                int pos = (Owner as SwitchModel).Branches.IndexOf(this);
                var act = (Owner as SwitchModel).BranchActions.ElementAt(pos);
                return act.Type;
            }
        }

        public void ReloadControlBlockInfo()
        {
            _reload_property("Type");
            _reload_property("ControlBlock");
        }
    }
    public class SwitchModel : ControlBlockModel
    {
        private ObservableCollection<ControlBlockModel> __branch_actions;
        //private ObservableCollection<(string, JsonArray)> __branch_conditions;
        private ObservableCollection<SwitchBranchModel> __branches;
        public IEnumerable<ControlBlockModel> BranchActions { get; }
        //public IEnumerable<(string, JsonArray)> BranchConditions { get; }
        public IEnumerable<SwitchBranchModel> Branches { get; }

        //private bool __local_events_modified = false;
        private bool __branch_modified = false;
        public SwitchModel(ControlBlockModelCollection collection, Switch_S blk) : base(collection, blk)
        {
            //__branch_actions = new ObservableCollection<ControlBlockModel>(blk.Branches.Select(x => ControlBlockModel.MAKE_CONTROL_BLOCK(x.action, this)));
            //__branch_conditions = new ObservableCollection<(string, JsonArray)>(blk.Branches.Select<(string, JsonArray, ControlBlockSource), (string, JsonArray)>(x => ValueTuple.Create(x.Item1, x.Item2.DeepClone().AsArray())));

            __branch_actions = new ObservableCollection<ControlBlockModel>();
            foreach (var a in blk.Branches.Select(x => x.action))
                __branch_actions.Add(ControlBlockModel.MAKE_CONTROL_BLOCK(collection, a, this));

            __branches = new ObservableCollection<SwitchBranchModel>(blk.Branches.Select(x => new SwitchBranchModel(x.name, x.condition.DeepClone().AsArray()) { Owner = this}));
            //__local_events = new ObservableCollection<LocalEventModel>(blk.LocalEvents.Select(x => new LocalEventModel(x.Key, x.Value.name, x.Value.evt){ Owner = this}));
            LocalEvents = new LocalEventModelCollection(this, blk.LocalEvents.Select(x => new LocalEventModel(x.Key, x.Value.name, x.Value.evt) { Owner = this }));

            BranchActions = __branch_actions;
            //BranchConditions = __branch_conditions;
            Branches = __branches;
        }

        //private ObservableCollection<LocalEventModel> __local_events;
        //public IEnumerable<LocalEventModel> LocalEvents { get { return __local_events; } }
        public LocalEventModelCollection LocalEvents { get; }

        public override string Header => $"{Name} ({__branches.Count})";
        public override BitmapImage ImageIcon => new BitmapImage(new Uri("pack://application:,,,/imgs/switch.png"));

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

        public override string Type => typeof(Switch_S).Name;

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

        public override void ApplyChanges()
        {
            if (Modified)
            {
                HashSet<uint> localEvents = new HashSet<uint>(LocalEvents.Events.Select(x => x.Index));
                if (localEvents.Count != LocalEvents.Events.Count())
                    throw new ArgumentException("Local Events with the same index exist.");

                (ControlBlock as Switch_S).ClearLocalEvents();
                (ControlBlock as Switch_S).MergeLocalEvents(LocalEvents.Events.Select(e => (e.Index, e.Name, e.Event.ToEvent())));


                if (__branch_modified)
                {
                    (ControlBlock as Switch_S).ClearBranches();
                    (ControlBlock as Switch_S).AddBranches(__branches.Zip(__branch_actions).Select(x => (x.First.Name, x.First.ConditionArray, x.Second.ControlBlock)));
                    __branch_modified = false;
                }

                ControlBlock.Name = Name;
                base.ApplyChanges();
            }
        }

        public override void DiscardChanges()
        {
            if (Modified)
            {
                Name = ControlBlock.Name;

                LocalEvents.Clear(false);
                foreach(var x in (ControlBlock as Switch_S).LocalEvents)
                    LocalEvents.Add(new LocalEventModel(x.Key, x.Value.name, x.Value.evt) { Owner = this }, false);


                if (__branch_modified)
                {
                    __branch_actions.Clear();
                    foreach (var a in (ControlBlock as Switch_S).Branches.Select(x => x.action))
                        __branch_actions.Add(ControlBlockModel.MAKE_CONTROL_BLOCK(Manager, a, this));

                    __branches.Clear();
                    foreach (var x in (ControlBlock as Switch_S).Branches)
                        __branches.Add(new SwitchBranchModel(x.name, x.condition.DeepClone().AsArray()) { Owner = this });

                    __branch_modified = false;
                }
                _notify_property_changed("Summary");
                base.DiscardChanges();
            }
        }

        public override ControlBlockSource ExportToControlBlockSource()
        {
            return new Switch_S(Name, new Dictionary<uint, (string name, Event evt)>(LocalEvents.Events.Select(x => KeyValuePair.Create(x.Index, (x.Name, x.Event.ToEvent())))),
                __branches.Zip(__branch_actions).Select(x => (x.First.Name, x.First.ConditionArray, x.Second.ExportToControlBlockSource())));
        }

        public override void SubComponentChangesApplied(Component sub)
        {
            int pos = __branch_actions.IndexOf(sub);
            __branches[pos].ReloadControlBlockInfo();
            _notify_property_changed("Summary");
            ApplyChanges();
        }

        public override void SubComponentChangesHappened(Component sub)
        {
            if (sub is LocalEventModel)
            {
                _notify_property_changed("Summary");
            }
            else if(sub is SwitchBranchModel)
            {
                _notify_property_changed("Summary");
                __branch_modified = true;
            }
        }

        public void MoveBranch(int oldIndex, int newIndex)
        {
            //(ControlBlock as Switch_S).MoveBranch(oldIndex, newIndex);
            var temp = __branches[oldIndex];
            __branches.RemoveAt(oldIndex);
            __branches.Insert(newIndex, temp);
            
            var act = __branch_actions[oldIndex];
            __branch_actions.RemoveAt(oldIndex);
            __branch_actions.Insert(newIndex, act);

            __branch_modified = true;
            _notify_property_changed("Summary");
            //ApplyChanges();
        }

        public int IndexOf(SwitchBranchModel branch)
        {
            return __branches.IndexOf(branch);
        }

        public void AddBranch()
        {
            var sm = new SwitchBranchModel("unnamed", new JsonArray()) { Owner = this };
            var blk = new SequentialModel(Manager, new Sequential_S("unnamed", Enumerable.Empty<ProcessStepSource>())) { Owner = this };
            if (this.ControlBlock.Nesting + blk.ControlBlock.Height > ControlBlockSource.MAX_NESTING_DEPTH)
                throw new InvalidOperationException($"The nesting depth of the Control Block exceeds the limit(MAX: {ControlBlockSource.MAX_NESTING_DEPTH}).");
            __branches.Add(sm);
            __branch_actions.Add(blk);
            __branch_modified = true;
            _notify_property_changed("Summary");
        }

        public void InsertBranch(int pos)
        {
            var sm = new SwitchBranchModel("unnamed", new JsonArray()) { Owner = this };
            var blk = new SequentialModel(Manager, new Sequential_S("unnamed", Enumerable.Empty<ProcessStepSource>())) { Owner = this };
            if (this.ControlBlock.Nesting + blk.ControlBlock.Height > ControlBlockSource.MAX_NESTING_DEPTH)
                throw new InvalidOperationException($"The nesting depth of the Control Block exceeds the limit(MAX: {ControlBlockSource.MAX_NESTING_DEPTH}).");
            __branches.Insert(pos, sm);
            __branch_actions.Insert(pos, blk);
            __branch_modified = true;
            _notify_property_changed("Summary");
        }

        public void AddBranch(JsonNode blkNode)
        {
            var sm = new SwitchBranchModel("unnamed", new JsonArray()) { Owner = this };
            var blk_s = ControlBlockSource.MAKE_BLK(blkNode["SOURCE"].AsObject(), null);
            var blk = ControlBlockModel.MAKE_CONTROL_BLOCK(Manager, blk_s, this);
            if (this.ControlBlock.Nesting + blk.ControlBlock.Height > ControlBlockSource.MAX_NESTING_DEPTH)
                throw new InvalidOperationException($"The nesting depth of the Control Block exceeds the limit(MAX: {ControlBlockSource.MAX_NESTING_DEPTH}).");

            __branches.Add(sm);
            __branch_actions.Add(blk);
            __branch_modified = true;
            _notify_property_changed("Summary");
        }

        public void InsertBranchBefore(ControlBlockModel pos, JsonNode blkNode)
        {
            var sm = new SwitchBranchModel("unnamed", new JsonArray()) { Owner = this };
            var blk_s = ControlBlockSource.MAKE_BLK(blkNode["SOURCE"].AsObject(), null);
            var blk = ControlBlockModel.MAKE_CONTROL_BLOCK(Manager, blk_s, this);
            if (this.ControlBlock.Nesting + blk.ControlBlock.Height > ControlBlockSource.MAX_NESTING_DEPTH)
                throw new InvalidOperationException($"The nesting depth of the Control Block exceeds the limit(MAX: {ControlBlockSource.MAX_NESTING_DEPTH}).");

            int idx = __branch_actions.IndexOf(pos);
            __branches.Insert(idx, sm);
            __branch_actions.Insert(idx, blk);
            __branch_modified = true;
            _notify_property_changed("Summary");
        }

        public void RemoveBranchAt(int pos)
        {
            __branches.RemoveAt(pos);
            __branch_actions.RemoveAt(pos);
            __branch_modified = true;
            _notify_property_changed("Summary");
        }

        public void ResetBranch(int pos, Type type)
        {
            if (Modified)
                throw new InvalidOperationException("Unapplied changes detected.");

            ControlBlockSource blk_s;
            ControlBlockModel blk_m;

            if (typeof(Sequential_S) == type)
            {
                blk_s = new Sequential_S(__branch_actions[pos].Name, Enumerable.Empty<ProcessStepSource>());
                blk_m = new SequentialModel(Manager, blk_s as Sequential_S) { Owner = this };
            }
            else if (typeof(Loop_S) == type)
            {
                blk_s = new Loop_S(__branch_actions[pos].Name, 1);
                Sequential_S seq = new Sequential_S("unnamed", Enumerable.Empty<ProcessStepSource>());
                (blk_s as Loop_S).LoopBody = seq;
                blk_m = new LoopModel(Manager, blk_s as Loop_S) { Owner = this };
            }
            else if (typeof(Switch_S) == type)
            {
                blk_s = new Switch_S(__branch_actions[pos].Name);
                blk_m = new SwitchModel(Manager, blk_s as Switch_S) { Owner = this };
            }
            else
                throw new ArgumentException($"Unrecognized control block type: \n{type.FullName}");

            (ControlBlock as Switch_S).RemoveBranchAt(pos);
            if (pos == (ControlBlock as Switch_S).Branches.Count)
                (ControlBlock as Switch_S).AddBranch(__branches[pos].Name, __branches[pos].ConditionArray, blk_s);
            else
                (ControlBlock as Switch_S).InsertBranch(pos, __branches[pos].Name, __branches[pos].ConditionArray, blk_s);

            __branch_actions.RemoveAt(pos);
            if(pos == __branch_actions.Count)
                __branch_actions.Add(blk_m);
            else
                __branch_actions.Insert(pos, blk_m);
            __branches[pos].ReloadControlBlockInfo();

            _notify_property_changed("Summary");
            ApplyChanges();
        }

    }
}
