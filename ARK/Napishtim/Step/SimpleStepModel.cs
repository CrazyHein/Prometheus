﻿using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim.TriggerMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression.AU;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.ShaderMechansim;
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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim
{
    public class SimpleStepModel : StepModel
    {
        public SimpleStepModel(SimpleStep_S step, Sequential_S seq, SequentialModel seqm) : base(step, seq, seqm)
        {
            //IsReadOnly = false;
            //__serial_number = seq.IndexOf(step);
            __with_timeout = false;
            __specify_timeout_directly = true;
            __step_time_to_timeout_collection = new ObservableCollection<SimpleStepModel>(
                seqm.SubSteps.Take(seq.IndexOf(step)).Where(x => (x as SimpleStepModel)?.WithTimeout == true).Select(x => (SimpleStepModel)x));
            //__local_events = new ObservableCollection<LocalEventModel>(step.LocalEvents.Select(x => new LocalEventModel(x.Key, x.Value.name, x.Value.evt) { Owner  = this}));
            LocalEvents = new LocalEventModelCollection(this, step.LocalEvents.Select(x => new LocalEventModel(x.Key, x.Value.name, x.Value.evt) { Owner = this }));
            __shaders = new ObservableCollection<ShaderModel>(step.Shaders.Select(x => new ShaderModel(x) { Owner = this}));
            __post_shaders = new ObservableCollection<ShaderModel>(step.PostShaders.Select(x => new ShaderModel(x) { Owner = this}));
            __abort_shaders = new ObservableCollection<ShaderModel>(step.AbortShaders.Select(x => new ShaderModel(x) { Owner = this }));
            __termination_condition = step.TerminationCondition;
            __aborot_condition = step.AbortCondition;
        }

        public SimpleStepModel(SimpleStepWithTimeout_S step, Sequential_S seq, SequentialModel seqm) : base(step, seq, seqm)
        {
            //IsReadOnly = false;
            //__serial_number = seq.IndexOf(step);
            __with_timeout = true;
            if(step.EmployPreceding == null)
            {
                __specify_timeout_directly = true;
                __timeout_value = step.Timeout;
            }
            else
            {
                __specify_timeout_directly = false;
                __step_time_to_timeout = seqm[seq.IndexOf(step.EmployPreceding)] as SimpleStepModel;
            }
            __step_time_to_timeout_collection = new ObservableCollection<SimpleStepModel>(
                seqm.SubSteps.Take(seq.IndexOf(step)).Where(x => (x as SimpleStepModel)?.WithTimeout == true).Select(x => (SimpleStepModel)x));

            //__local_events = new ObservableCollection<LocalEventModel>(step.LocalEvents.Select(x => new LocalEventModel(x.Key, x.Value.name, x.Value.evt) { Owner = this }));
            LocalEvents = new LocalEventModelCollection(this, step.LocalEvents.Select(x => new LocalEventModel(x.Key, x.Value.name, x.Value.evt) { Owner = this }));
            __shaders = new ObservableCollection<ShaderModel>(step.Shaders.Select(x => new ShaderModel(x) { Owner = this }));
            __post_shaders = new ObservableCollection<ShaderModel>(step.PostShaders.Select(x => new ShaderModel(x) { Owner = this }));
            __abort_shaders = new ObservableCollection<ShaderModel>(step.AbortShaders.Select(x => new ShaderModel(x) { Owner = this }));
            __termination_condition = step.TerminationCondition;
            __aborot_condition = step.AbortCondition;
        }


        public override void ApplyChanges()
        {
            if (Modified)
            {
                //var step = ExportToProcessStepSource();
                //var node = ((Owner as SequentialModel).ControlBlock as Sequential_S).NodeAt(SerialNumber);
                //((Owner as SequentialModel).ControlBlock as Sequential_S).ReplaceProcessStepWith(node, step);
                _step_for_rollback = Step;
                Step = ExportToProcessStepSource();
                base.ApplyChanges();
            }
        }

        protected override void RollbackChanges()
        {
            if(Validated)
            {
                Step = _step_for_rollback;
            }
        }
        public override void DiscardChanges()
        {
            if (Modified)
            {
                __enable_summary_update_notification = false;

                Name = Step.Name;
                if (Step is SimpleStep_S)
                {
                    var simpleStep = (SimpleStep_S)Step;
                    //SerialNumber = Sequential.IndexOf(Step);
                    WithTimeout = false;
                    SpecifyTimeoutDirectly = true;

                    __step_time_to_timeout_collection.Clear();
                    foreach(var s in (Owner as SequentialModel).SubSteps.Take(Sequential.IndexOf(Step)).Where(x => (x as SimpleStepModel)?.WithTimeout == true).Select(x => (SimpleStepModel)x))
                        __step_time_to_timeout_collection.Add(s);

                    LocalEvents.Clear(false);
                    foreach(var s in simpleStep.LocalEvents.Select(x => new LocalEventModel(x.Key, x.Value.name, x.Value.evt) { Owner = this }))
                        LocalEvents.Add(s, false);

                    __shaders.Clear();
                    foreach (var s in simpleStep.Shaders.Select(x => new ShaderModel(x) { Owner = this }))
                        __shaders.Add(s);

                    __post_shaders.Clear();
                    foreach (var s in simpleStep.PostShaders.Select(x => new ShaderModel(x) { Owner = this }))
                        __post_shaders.Add(s);

                    __abort_shaders.Clear();
                    foreach (var s in simpleStep.AbortShaders.Select(x => new ShaderModel(x) { Owner = this }))
                        __abort_shaders.Add(s);

                    TerminationCondition = simpleStep.TerminationCondition;
                    AbortCondition = simpleStep.AbortCondition;
                }
                else
                {
                    var simpleStep = (SimpleStepWithTimeout_S)Step;
                    //SerialNumber = Sequential.IndexOf(Step);
                    WithTimeout = true;

                    __step_time_to_timeout_collection.Clear();
                    foreach (var s in (Owner as SequentialModel).SubSteps.Take(Sequential.IndexOf(Step)).Where(x => (x as SimpleStepModel)?.WithTimeout == true).Select(x => (SimpleStepModel)x))
                        __step_time_to_timeout_collection.Add(s);

                    if (simpleStep.EmployPreceding == null)
                    {
                        SpecifyTimeoutDirectly = true;
                        TimeoutValue = simpleStep.Timeout;
                    }
                    else
                    {
                        SpecifyTimeoutDirectly = false;
                        StepTimeToTimeout = (Owner as SequentialModel)[Sequential.IndexOf(simpleStep.EmployPreceding)] as SimpleStepModel;
                    }

                    LocalEvents.Clear(false);
                    foreach (var s in simpleStep.LocalEvents.Select(x => new LocalEventModel(x.Key, x.Value.name, x.Value.evt) { Owner = this }))
                        LocalEvents.Add(s, false);

                    __shaders.Clear();
                    foreach (var s in simpleStep.Shaders.Select(x => new ShaderModel(x) { Owner = this }))
                        __shaders.Add(s);

                    __post_shaders.Clear();
                    foreach (var s in simpleStep.PostShaders.Select(x => new ShaderModel(x) { Owner = this }))
                        __post_shaders.Add(s);

                    __abort_shaders.Clear();
                    foreach (var s in simpleStep.AbortShaders.Select(x => new ShaderModel(x) { Owner = this }))
                        __abort_shaders.Add(s);

                    TerminationCondition = simpleStep.TerminationCondition;
                    AbortCondition = simpleStep.AbortCondition;
                }
                __enable_summary_update_notification = true;
                _notify_property_changed("Summary");
                base.DiscardChanges();
            }
        }

        private bool __enable_summary_update_notification = true;
        private bool __enable_time_to_timeout_update_notification = true;

        public override int SerialNumber
        {
            get { return Sequential.IndexOf(Step); }
        }

        private bool __with_timeout;
        public bool WithTimeout { 
            get { return __with_timeout; }
            set 
            { 
                __with_timeout = value; 
                _notify_property_changed(); 
                if(__enable_summary_update_notification)
                    _reload_property("Summary"); 
            } 
        }

        private bool __specify_timeout_directly;
        public bool SpecifyTimeoutDirectly
        {
            get { return __specify_timeout_directly; }
            set 
            { 
                __specify_timeout_directly = value; 
                _notify_property_changed(); 
                if (__enable_summary_update_notification)
                    _reload_property("Summary"); 
            }
        }

        private int __timeout_value;
        public int TimeoutValue
        {
            get { return __timeout_value; }
            set 
            { 
                if (value != __timeout_value) 
                { 
                    __timeout_value = value; 
                    _notify_property_changed(); 
                    if (__enable_summary_update_notification)
                        _reload_property("Summary"); 
                } 
            }
        }

        private SimpleStepModel? __step_time_to_timeout;
        public SimpleStepModel? StepTimeToTimeout 
        {
            get { return __step_time_to_timeout; } 
            set
            {
                if (value != __step_time_to_timeout)
                {
                    __step_time_to_timeout = value;
                    if(__enable_time_to_timeout_update_notification)
                        _notify_property_changed();
                    if (__enable_summary_update_notification)
                        _reload_property("Summary");
                }
            }
        }

        private ObservableCollection<SimpleStepModel> __step_time_to_timeout_collection;
        public IEnumerable<SimpleStepModel> StepTimeToTimeoutCollection { get{ return __step_time_to_timeout_collection; }}

        //private ObservableCollection<LocalEventModel> __local_events;
        //public IEnumerable<LocalEventModel> LocalEvents { get { return __local_events; } }
        public LocalEventModelCollection LocalEvents { get; }

        private ObservableCollection<ShaderModel> __shaders;
        public IEnumerable<ShaderModel> Shaders { get { return __shaders; } }

        private string __termination_condition = "";
        public string TerminationCondition
        {
            get { return __termination_condition; }
            set 
            {
                value = value.Trim();
                if (value != __termination_condition) 
                { 
                    __termination_condition = value; 
                    _notify_property_changed();
                    if (__enable_summary_update_notification)
                        _reload_property("Summary"); 
                }
            }
        }

        private string __aborot_condition = "";
        public string AbortCondition
        {
            get { return __aborot_condition; }
            set
            {
                value = value.Trim();
                if (value != __aborot_condition)
                {
                    __aborot_condition = value;
                    _notify_property_changed();
                    if (__enable_summary_update_notification)
                        _reload_property("Summary");
                }
            }
        }

        private ObservableCollection<ShaderModel> __post_shaders;
        public IEnumerable<ShaderModel> PostShaders { get { return __post_shaders; } }

        private ObservableCollection<ShaderModel> __abort_shaders;
        public IEnumerable<ShaderModel> AbortShaders { get { return __abort_shaders; } }

        public override string Summary
        {
            get
            {
                string summary;
                try
                {
                    if (Modified)
                        summary = ExportToProcessStepSource().ToString();
                    else
                        summary = Step.ToString();
                }
                catch(Exception ex)
                {
                    summary = ex.ToString();
                }
                return summary;
            }
        }

        public override JsonNode ToJson()
        {
            if (Modified)
                throw new InvalidOperationException("Unapplied changes detected.");
            var json = new JsonObject();
            json["VERSION"] = Settings.ArkVersion;
            json["ASSEMBLY"] = this.GetType().FullName;
            json["OWNER"] = Owner.GetHashCode();
            json["SOURCE"] = Step.SaveAsJson((Owner as SequentialModel).ControlBlock as Sequential_S);
            return json; ;
        }

        public void ClearStepAction()
        {
            __shaders.Clear();
            foreach (var s in __post_shaders)
                s.EvaluateOmissible();
            foreach (var s in __abort_shaders)
                s.EvaluateOmissible();
            _notify_property_changed("Summary");
        }

        public void AddStepAction()
        {
            __shaders.Add(new ShaderModel() { Owner = this});
            foreach (var s in __shaders.SkipLast(1))
                s.EvaluateOmissible();
            foreach (var s in __post_shaders)
                s.EvaluateOmissible();
            foreach (var s in __abort_shaders)
                s.EvaluateOmissible();
            _notify_property_changed("Summary");
        }

        public void AddStepActions(JsonArray array)
        {
            foreach(var shader in array.Select(x => new ShaderModel(x.AsObject()) { Owner = this}))
                __shaders.Add(shader);
            foreach (var s in __shaders.SkipLast(array.Count))
                s.EvaluateOmissible();
            foreach (var s in __post_shaders)
                s.EvaluateOmissible();
            foreach (var s in __abort_shaders)
                s.EvaluateOmissible();
            _notify_property_changed("Summary");
        }

        public void InsertStepAction(int pos)
        {
            __shaders.Insert(pos, new ShaderModel() { Owner = this });
            foreach (var s in __shaders.SkipLast(__shaders.Count - pos))
                s.EvaluateOmissible();
            foreach (var s in __post_shaders)
                s.EvaluateOmissible();
            foreach (var s in __abort_shaders)
                s.EvaluateOmissible();
            _notify_property_changed("Summary");
        }

        public void InsertStepActions(int pos, JsonArray array)
        {
            foreach (var shader in array.Select(x => new ShaderModel(x.AsObject()) { Owner = this }).Reverse())
                __shaders.Insert(pos, shader);
            foreach (var s in __shaders.SkipLast(__shaders.Count - pos))
                s.EvaluateOmissible();
            foreach (var s in __post_shaders)
                s.EvaluateOmissible();
            foreach (var s in __abort_shaders)
                s.EvaluateOmissible();
            _notify_property_changed("Summary");
        }

        public void RemoveStepActionAt(int pos)
        {
            var removed = __shaders[pos];
            __shaders.RemoveAt(pos);
            if (removed.CanBeOmitted == false)
            {
                foreach (var s in __shaders.SkipLast(__shaders.Count - pos))
                    s.EvaluateOmissible();
            }
            foreach (var s in __post_shaders)
                s.EvaluateOmissible();
            foreach (var s in __abort_shaders)
                s.EvaluateOmissible();
            _notify_property_changed("Summary");
        }

        public void RemoveStepActions(IEnumerable<ShaderModel> shaders)
        {
            List<ShaderModel> temp = new List<ShaderModel>(shaders);
            int max = shaders.Select(x => __shaders.IndexOf(x)).Max();

            foreach (var sh in temp)
                __shaders.Remove(sh);
            foreach (var s in __shaders.SkipLast(temp.Count - max - 1))
                s.EvaluateOmissible();

            foreach (var s in __post_shaders)
                s.EvaluateOmissible();
            foreach (var s in __abort_shaders)
                s.EvaluateOmissible();

            _notify_property_changed("Summary");
        }

        public void ClearPostStepAction()
        {
            __post_shaders.Clear();
            _notify_property_changed("Summary");
        }

        public void AddPostStepAction()
        {
            __post_shaders.Add(new ShaderModel() { Owner = this });
            foreach (var s in __post_shaders.SkipLast(1))
                s.EvaluateOmissible();
            _notify_property_changed("Summary");
        }

        public void AddPostStepActions(JsonArray array)
        {
            foreach (var shader in array.Select(x => new ShaderModel(x.AsObject()) { Owner = this }))
                __post_shaders.Add(shader);
            foreach (var s in __post_shaders.SkipLast(array.Count))
                s.EvaluateOmissible();
            _notify_property_changed("Summary");
        }

        public void InsertPostStepAction(int pos)
        {
            __post_shaders.Insert(pos, new ShaderModel() { Owner = this });
            foreach (var s in __post_shaders.SkipLast(__post_shaders.Count - pos))
                s.EvaluateOmissible();
            _notify_property_changed("Summary");
        }

        public void InsertPostStepActions(int pos, JsonArray array)
        {
            foreach (var shader in array.Select(x => new ShaderModel(x.AsObject()) { Owner = this }).Reverse())
                __post_shaders.Insert(pos, shader);
            foreach (var s in __post_shaders.SkipLast(__post_shaders.Count - pos))
                s.EvaluateOmissible();
            _notify_property_changed("Summary");
        }

        public void RemovePostStepActionAt(int pos)
        {
            var removed = __post_shaders[pos];
            __post_shaders.RemoveAt(pos);
            if (removed.CanBeOmitted == false)
            {
                foreach (var s in __post_shaders.SkipLast(__post_shaders.Count - pos))
                    s.EvaluateOmissible();
            }
            _notify_property_changed("Summary");
        }

        public void RemovePostStepActions(IEnumerable<ShaderModel> shaders)
        {
            List<ShaderModel> temp = new List<ShaderModel>(shaders);
            int max = shaders.Select(x => __post_shaders.IndexOf(x)).Max();
            
            foreach (var sh in temp)
                __post_shaders.Remove(sh);

            foreach (var s in __post_shaders.SkipLast(temp.Count - max - 1))
                s.EvaluateOmissible();

            _notify_property_changed("Summary");
        }

        public void ClearAbortStepAction()
        {
            __abort_shaders.Clear();
            _notify_property_changed("Summary");
        }

        public void AddAbortStepAction()
        {
            __abort_shaders.Add(new ShaderModel() { Owner = this });
            foreach (var s in __abort_shaders.SkipLast(1))
                s.EvaluateOmissible();
            _notify_property_changed("Summary");
        }

        public void AddAbortStepActions(JsonArray array)
        {
            foreach (var shader in array.Select(x => new ShaderModel(x.AsObject()) { Owner = this }))
                __abort_shaders.Add(shader);
            foreach (var s in __abort_shaders.SkipLast(array.Count))
                s.EvaluateOmissible();
            _notify_property_changed("Summary");
        }

        public void InsertAbortStepAction(int pos)
        {
            __abort_shaders.Insert(pos, new ShaderModel() { Owner = this });
            foreach (var s in __abort_shaders.SkipLast(__abort_shaders.Count - pos))
                s.EvaluateOmissible();
            _notify_property_changed("Summary");
        }

        public void InsertAbortStepActions(int pos, JsonArray array)
        {
            foreach (var shader in array.Select(x => new ShaderModel(x.AsObject()) { Owner = this }).Reverse())
                __abort_shaders.Insert(pos, shader);
            foreach (var s in __abort_shaders.SkipLast(__abort_shaders.Count - pos))
                s.EvaluateOmissible();
            _notify_property_changed("Summary");
        }

        public void RemoveAbortStepActionAt(int pos)
        {
            var removed = __abort_shaders[pos];
            __abort_shaders.RemoveAt(pos);
            if (removed.CanBeOmitted == false)
            {
                foreach (var s in __abort_shaders.SkipLast(__abort_shaders.Count - pos))
                    s.EvaluateOmissible();
            }
            _notify_property_changed("Summary");
        }

        public void RemoveAbortStepActions(IEnumerable<ShaderModel> shaders)
        {
            List<ShaderModel> temp = new List<ShaderModel>(shaders);
            int max = shaders.Select(x => __abort_shaders.IndexOf(x)).Max();

            foreach (var sh in temp)
                __abort_shaders.Remove(sh);

            foreach (var s in __abort_shaders.SkipLast(temp.Count - max - 1))
                s.EvaluateOmissible();

            _notify_property_changed("Summary");
        }

        public override ProcessStepSource ExportToProcessStepSource(Sequential_S? seq = null)
        {
            Dictionary<uint, (string name, Event evt)>? locals = null;
            ProcessShaders? shaders = null;
            ProcessShaders? postShaders = null;
            JsonArray? completionCondition = null;
            ProcessShaders? abortShaders = null;
            JsonArray? abortCondition = null;

            if (LocalEvents.Events.Count() != 0)
                locals = new Dictionary<uint, (string name, Event evt)>(LocalEvents.Events.Select(x => KeyValuePair.Create(x.Index, (x.Name, x.Event.ToEvent()))));
            if(__shaders.Count != 0)
                shaders = new UserProcessShaders(__shaders.Select(x => (x.Name, x.LeftValue, x.RightValue)));
            if (__post_shaders.Count != 0)
                postShaders = new UserProcessShaders(__post_shaders.Select(x => (x.Name, x.LeftValue, x.RightValue)));
            if (__abort_shaders.Count != 0)
                abortShaders = new UserProcessShaders(__abort_shaders.Select(x => (x.Name, x.LeftValue, x.RightValue)));
            if (TerminationCondition.Length != 0)
            {
                completionCondition = new JsonArray();
                foreach (var line in TerminationCondition.Split('\n').Select(x => x.TrimEnd()))
                    completionCondition.Add(line);
            }
            if(AbortCondition.Length != 0)
            {
                abortCondition = new JsonArray();
                foreach (var line in AbortCondition.Split('\n').Select(x => x.TrimEnd()))
                    abortCondition.Add(line);
            }

            if (WithTimeout == false)
                return new SimpleStep_S(Name, locals, shaders, completionCondition, postShaders, abortCondition, abortShaders);
            else if (SpecifyTimeoutDirectly)
                return new SimpleStepWithTimeout_S(Name, locals, shaders, TimeoutValue, completionCondition, postShaders, abortCondition, abortShaders);
            else
            {
                if(seq != null)
                    return new SimpleStepWithTimeout_S(Name, locals, shaders, seq[Sequential.IndexOf(StepTimeToTimeout.Step)] as SimpleStepWithTimeout_S, completionCondition, postShaders, abortCondition, abortShaders);
                else
                    return new SimpleStepWithTimeout_S(Name, locals, shaders, StepTimeToTimeout.Step as SimpleStepWithTimeout_S, completionCondition, postShaders, abortCondition, abortShaders);
            }
        }

        public override void SequenceChanged()
        {
            __enable_time_to_timeout_update_notification = false;
            __step_time_to_timeout_collection.Clear();
            foreach (var s in (Owner as SequentialModel).SubSteps.Take(Sequential.IndexOf(Step)).Where(x => (x as SimpleStepModel)?.WithTimeout == true).Select(x => (SimpleStepModel)x))
                __step_time_to_timeout_collection.Add(s);

            var simpleStep = Step as SimpleStepWithTimeout_S;
            if (simpleStep?.EmployPreceding != null)
                __step_time_to_timeout = (Owner as SequentialModel)[Sequential.IndexOf(simpleStep.EmployPreceding)] as SimpleStepModel;

            _reload_property("Header");
            _reload_property("SerialNumber");
            _reload_property("StepTimeToTimeout");
            _reload_property("Summary");
            __enable_time_to_timeout_update_notification = true;
        }

        public override void EvaluateShaderValidity()
        {
            foreach (var s in Shaders.Where(x => x.IsObjectDirectAssignment))
                s.EvaluateOmissible();
            foreach (var s in PostShaders.Where(x => x.IsObjectDirectAssignment))
                s.EvaluateOmissible();
            foreach (var s in AbortShaders.Where(x => x.IsObjectDirectAssignment))
                s.EvaluateOmissible();
        }

        public override void SubComponentChangesApplied(Component sub)
        {
            _notify_property_changed("Summary");
        }

        public override void SubComponentChangesHappened(Component sub)
        {
            if (sub is ShaderModel)
            {
                int shaderPos = __shaders.IndexOf(sub as ShaderModel);
                int postShaderPos = __post_shaders.IndexOf(sub as ShaderModel);
                int abortShaderPos = __abort_shaders.IndexOf(sub as ShaderModel);
                if (shaderPos != -1)
                {
                    foreach (var s in __shaders.SkipLast(__shaders.Count - shaderPos).SkipWhile(s => s == sub).Where(x => x.IsObjectDirectAssignment))
                        s.EvaluateOmissible();
                    foreach (var s in __post_shaders.SkipWhile(s => s == sub).Where(x => x.IsObjectDirectAssignment))
                        s.EvaluateOmissible();
                    foreach (var s in __abort_shaders.SkipWhile(s => s == sub).Where(x => x.IsObjectDirectAssignment))
                        s.EvaluateOmissible();
                }
                else if(postShaderPos != -1)
                {
                    foreach (var s in __post_shaders.SkipLast(__post_shaders.Count - postShaderPos).SkipWhile(s => s == sub).Where(x => x.IsObjectDirectAssignment))
                        s.EvaluateOmissible();
                }
                else if(abortShaderPos != -1)
                {
                    foreach (var s in __abort_shaders.SkipLast(__abort_shaders.Count - abortShaderPos).SkipWhile(s => s == sub).Where(x => x.IsObjectDirectAssignment))
                        s.EvaluateOmissible();
                }
            }

            _notify_property_changed("Summary");
        }
    }
}
