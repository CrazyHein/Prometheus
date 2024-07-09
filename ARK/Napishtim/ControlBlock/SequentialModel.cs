using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression.AU;
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
    public class SequentialModel : ControlBlockModel
    {
        private ObservableCollection<StepModel> __sub_steps;
        public SequentialModel(ControlBlockModelCollection collection, Sequential_S blk) : base(collection, blk)
        {
            //__sub_steps = new ObservableCollection<StepModel>(blk.OriginalProcessSteps.Select(s => StepModel.MAKE_STEP(s, blk, this)));
            __sub_steps = new ObservableCollection<StepModel>();
            SubSteps = __sub_steps;
            foreach (var step in blk.OriginalProcessSteps)
                __sub_steps.Add(StepModel.MAKE_STEP(step, blk, this));
        }

        public static SequentialModel MAKE(ControlBlockModelCollection collection, JsonNode seqNode)
        {
            Sequential_S blk = new Sequential_S(seqNode["NAME"].GetValue<string>(), Enumerable.Empty<ProcessStepSource>()) { Owner = null};
            foreach (var stp in seqNode["SUBSTEPS"].AsArray())
            {
                blk.AddProcessStepLast(ProcessStepSource.MAKE_STEP(stp["SOURCE"].AsObject(), blk));
            }
            SequentialModel blk_m = new SequentialModel(collection, blk) { Owner = null };

            return blk_m;
        }

        public StepModel this[int i]
        {
            get { return __sub_steps[i]; }
            /*
            set 
            {
                var node = (ControlBlock as Sequential_S).NodeAt(i);
                (ControlBlock as Sequential_S).ReplaceProcessStepWith(node, value.Step);

                __sub_steps[i] = value; 
                _notify_property_changed("Summary");
                ApplyChanges();
            }*/
        }

        public void Move(int oldIndex, int newIndex)
        {
            if (Modified == false)
            {
                int length = (ControlBlock as Sequential_S).OriginalProcessSteps.Count();
                if (oldIndex >= length || oldIndex < 0)
                    throw new IndexOutOfRangeException($"The old index {oldIndex} is invalid to a linked-list of length {length}.");
                if (newIndex >= length || newIndex < 0)
                    throw new IndexOutOfRangeException($"The new index {newIndex} is invalid to a linked-list of length {length}.");

                var sourceNode = (ControlBlock as Sequential_S).NodeAt(oldIndex);
                var targetNode = (ControlBlock as Sequential_S).NodeAt(newIndex);
                if (oldIndex > newIndex)
                    (ControlBlock as Sequential_S).MoveBefore(sourceNode, targetNode);
                else
                    (ControlBlock as Sequential_S).MoveAfter(sourceNode, targetNode);

                var temp = __sub_steps[oldIndex];
                __sub_steps.RemoveAt(oldIndex);
                __sub_steps.Insert(newIndex, temp);

                for (int i = Math.Min(oldIndex, newIndex); i < __sub_steps.Count; i++)
                {
                    __sub_steps[i].SequenceChanged();
                    __sub_steps[i].EvaluateShaderValidity();
                }

                _notify_property_changed("Summary");
                ApplyChanges();
            }
            else
                throw new InvalidOperationException("Apply or discard the changes first and then adjust the order of sub-steps.");
        }

        public SimpleStepModel Add(string name)
        {
            if (Modified == false)
            {
                SimpleStep_S step_s = new SimpleStep_S(name, null, null, null, null);
                (ControlBlock as Sequential_S).AddProcessStepLast(step_s);
                SimpleStepModel step = new SimpleStepModel(step_s, ControlBlock as Sequential_S, this) { Owner = this };
                __sub_steps.Add(step);

                _notify_property_changed("Summary");
                ApplyChanges();
                return step;
            }
            else
                throw new InvalidOperationException("Apply or discard the changes first and then add sub-steps.");
        }

        public SimpleStepModel Add(JsonNode stepNode)
        {
            if (Modified == false)
            {
                ProcessStepSource step_s = ProcessStepSource.MAKE_STEP(stepNode["SOURCE"].AsObject(), ControlBlock as Sequential_S);
                SimpleStepModel step = null;
                
                if(step_s is SimpleStep_S)
                    step = new SimpleStepModel(step_s as SimpleStep_S, ControlBlock as Sequential_S, this) { Owner = this };
                else
                    step = new SimpleStepModel(step_s as SimpleStepWithTimeout_S, ControlBlock as Sequential_S, this) { Owner = this };

                (ControlBlock as Sequential_S).AddProcessStepLast(step_s);
                __sub_steps.Add(step);

                _notify_property_changed("Summary");
                ApplyChanges();
                return step;
            }
            else
                throw new InvalidOperationException("Apply or discard the changes first and then add sub-steps.");
        }

        public void Remove(StepModel stp)
        {
            if (Modified == false)
            {
                int pos = __sub_steps.IndexOf(stp);
                (ControlBlock as Sequential_S).RemoveProcessStep((ControlBlock as Sequential_S).NodeAt(pos));
                __sub_steps.RemoveAt(pos);
                for (int i = pos; i < __sub_steps.Count; i++)
                {
                    __sub_steps[i].SequenceChanged();
                    __sub_steps[i].EvaluateShaderValidity();
                }

                _notify_property_changed("Summary");
                ApplyChanges();
            }
            else
                throw new InvalidOperationException("Apply or discard the changes first and then remove sub-steps.");
        }

        public void RemoveAt(int pos)
        {
            if (Modified == false)
            {
                (ControlBlock as Sequential_S).RemoveProcessStep((ControlBlock as Sequential_S).NodeAt(pos));
                __sub_steps.RemoveAt(pos);
                for (int i = pos; i < __sub_steps.Count; i++)
                {
                    __sub_steps[i].SequenceChanged();
                    __sub_steps[i].EvaluateShaderValidity();
                }

                _notify_property_changed("Summary");
                ApplyChanges();
            }
            else
                throw new InvalidOperationException("Apply or discard the changes first and then remove sub-steps.");
        }

        public SimpleStepModel InsertBefore(int pos, string name)
        {
            if (Modified == false)
            {
                SimpleStep_S step_s = new SimpleStep_S(name, null, null, null, null);
                (ControlBlock as Sequential_S).AddProcessStepBefore((ControlBlock as Sequential_S).NodeAt(pos), step_s);
                SimpleStepModel step = new SimpleStepModel(step_s, ControlBlock as Sequential_S, this) { Owner = this };
                __sub_steps.Insert(pos, step);
                for (int i = pos; i < __sub_steps.Count; i++)
                {
                    __sub_steps[i].SequenceChanged();
                    __sub_steps[i].EvaluateShaderValidity();
                }

                _notify_property_changed("Summary");
                ApplyChanges();
                return step;
            }
            else
                throw new InvalidOperationException("Apply or discard the changes first and then insert sub-steps.");
        }

        public SimpleStepModel InsertBefore(int pos, JsonNode stepNode)
        {
            if (Modified == false)
            {
                ProcessStepSource step_s = ProcessStepSource.MAKE_STEP(stepNode["SOURCE"].AsObject(), ControlBlock as Sequential_S);
                SimpleStepModel step = null;

                if (step_s is SimpleStep_S)
                    step = new SimpleStepModel(step_s as SimpleStep_S, ControlBlock as Sequential_S, this) { Owner = this };
                else
                    step = new SimpleStepModel(step_s as SimpleStepWithTimeout_S, ControlBlock as Sequential_S, this) { Owner = this };

                (ControlBlock as Sequential_S).AddProcessStepBefore((ControlBlock as Sequential_S).NodeAt(pos), step_s);
                __sub_steps.Insert(pos, step);
                for (int i = pos; i < __sub_steps.Count; i++)
                {
                    __sub_steps[i].SequenceChanged();
                    __sub_steps[i].EvaluateShaderValidity();
                }

                _notify_property_changed("Summary");
                ApplyChanges();
                return step;
            }
            else
                throw new InvalidOperationException("Apply or discard the changes first and then insert sub-steps.");
        }

        public override BitmapImage ImageIcon => new BitmapImage(new Uri("pack://application:,,,/imgs/seq.png"));
        public IEnumerable<StepModel> SubSteps { get; }

        public override string Summary
        {
            get
            {
                string summary;
                try
                {
                    if(Modified)
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

        public override string Type => typeof(Sequential_S).Name;

        public override ControlBlockSource ExportToControlBlockSource()
        {
            Sequential_S seq = new Sequential_S(Name, Enumerable.Empty<ProcessStepSource>()) { Owner = null };
            foreach(var s in SubSteps)
            {
                seq.AddProcessStepLast(s.ExportToProcessStepSource(seq));
            }
            return seq;
            //return new Sequential_S(Name, SubSteps.Select(x => x.Step())) { Owner = null };
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

        public override void SubComponentChangesApplied(Component sub)
        {
            int idx = __sub_steps.IndexOf(sub as StepModel);
            var node = (ControlBlock as Sequential_S).NodeAt(idx);
            (ControlBlock as Sequential_S).ReplaceProcessStepWith(node, (sub as StepModel).Step);

            for (int i = idx; i < __sub_steps.Count; i++)
            {
                __sub_steps[i].SequenceChanged();
                if(i != idx)
                    __sub_steps[i].EvaluateShaderValidity();
            }

            _notify_property_changed("Summary");
            ApplyChanges();
        }

        public override void SubComponentChangesHappened(Component sub)
        {
            
        }
    }
}
