using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.ExceptionMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.StepMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ExceptionHandling;
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
    public class SimpleExceptionResponseBranchModel: Component
    {
        public override void SubComponentChangesApplied(Component sub) => throw new NotSupportedException();
        public override void SubComponentChangesHappened(Component sub) => throw new NotSupportedException();
        protected override void RollbackChanges() => throw new NotSupportedException();
        public override BitmapImage ImageIcon => throw new NotSupportedException();
        public override string Summary => throw new NotSupportedException();
        public override string Header { get { return Name; } }
        public override JsonNode ToJson() => throw new NotSupportedException();

        private string __name = "unnamed";
        public string Name
        {
            get { return __name; }
            set
            {
                value = value.Trim();
                if (value != __name)
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
                if(__condition.Length == 0)
                    return new JsonArray();
                return new JsonArray(__condition.Split('\n').Select(x => JsonValue.Create<string>(x.TrimEnd())).ToArray());
            }
        }

        private int __return_code = -1;
        public int ReturnCode
        {
            get { return __return_code; }
            set
            {
                if (value != __return_code)
                {
                    __return_code = value;
                    _notify_property_changed();
                }
            }
        }

        public SimpleExceptionResponseBranchModel(string name, JsonArray condition, ProcessShaders postShaders)
        {
            __name = name.Trim();
            __condition = string.Join('\n', condition.AsArray().Select(x => x.GetValue<string>()));
            __return_code = (int)(postShaders[0].Shader.Expr.Value(true, 0.0));
        }

        public SimpleExceptionResponseBranchModel()
        {
            __name = "unnamed";
            __condition = string.Empty;
            __return_code= -1;
        }
    }

    public class SimpleExceptionResponseModel: ExceptionResponseModel
    {
        private ObservableCollection<SimpleExceptionResponseBranchModel> __branches;
        public IEnumerable<SimpleExceptionResponseBranchModel> Branches { get { return __branches; } }

        public LocalEventModelCollection LocalEvents { get; }
        public override string Summary
        {
            get
            {
                string summary;
                try
                {
                    if (Modified)
                        summary = ExportToExceptionResponseSource()?.ToString();
                    else
                        summary = ExceptionResponse?.ToString();
                }
                catch (Exception ex)
                {
                    summary = ex.ToString();
                }
                if(summary == null)
                    return "The Exception Response feature is disabled.";
                return summary;
            }
        }

        public override JsonNode ToJson()
        {
            if (Modified)
                throw new InvalidOperationException("Unapplied changes detected.");
            if (Enabled)
            {
                var json = new JsonObject();
                json["VERSION"] = Settings.ArkVersion;
                json["ASSEMBLY"] = this.GetType().FullName;
                json["SOURCE"] = ExceptionResponse.SaveAsJson();
                return json;
            }
            else
                return new JsonObject();
        }

        public SimpleExceptionResponseModel(SimpleExceptionResponse_S? exception, ContextModel context): base(exception, context) 
        {
            if (exception != null)
            {
                LocalEvents = new LocalEventModelCollection(this, exception.LocalEvents.Select(x => new LocalEventModel(x.Key, x.Value.name, x.Value.evt) { Owner = this }));
                __branches = new ObservableCollection<SimpleExceptionResponseBranchModel>(exception.Branches.Select(x => new SimpleExceptionResponseBranchModel(x.name, x.condition, x.postShaders) { Owner = this}));
            }
            else
            {
                LocalEvents = new LocalEventModelCollection(this, Enumerable.Empty<LocalEventModel>());
                __branches = new ObservableCollection<SimpleExceptionResponseBranchModel>();
            }
        }

        public override void ApplyChanges()
        {
            if (Modified)
            {
                //var step = ExportToProcessStepSource();
                //var node = ((Owner as SequentialModel).ControlBlock as Sequential_S).NodeAt(SerialNumber);
                //((Owner as SequentialModel).ControlBlock as Sequential_S).ReplaceProcessStepWith(node, step);
                _exception_response_for_rollback = ExceptionResponse;
                ExceptionResponse = ExportToExceptionResponseSource();
                base.ApplyChanges();
            }
        }

        protected override void RollbackChanges()
        {
            if (Validated)
            {
                ExceptionResponse = _exception_response_for_rollback;
            }
        }

        public override void DiscardChanges()
        {
            if (Modified)
            {
                var response = ExceptionResponse as SimpleExceptionResponse_S;
                if(response == null)
                {
                    Enabled = false;
                    LocalEvents.Clear(false);
                    __branches.Clear();
                }
                else
                {
                    Enabled = true;

                    LocalEvents.Clear(false);
                    foreach (var s in response.LocalEvents.Select(x => new LocalEventModel(x.Key, x.Value.name, x.Value.evt) { Owner = this }))
                        LocalEvents.Add(s, false);

                    __branches.Clear();
                    foreach (var s in response.Branches.Select(x => new SimpleExceptionResponseBranchModel(x.name, x.condition, x.postShaders) { Owner = this }))
                        __branches.Add(s);
                }
                _notify_property_changed("Summary");
                base.DiscardChanges();
            }
        }

        public override ExceptionResponseSource? ExportToExceptionResponseSource()
        {
            if (Enabled == false)
                return null;

            Dictionary<uint, (string name, Event evt)>? locals = null;

            if (LocalEvents.Events.Count() != 0)
                locals = new Dictionary<uint, (string name, Event evt)>(LocalEvents.Events.Select(x => KeyValuePair.Create(x.Index, (x.Name, x.Event.ToEvent()))));

            return new SimpleExceptionResponse_S(Name, locals, __branches.Select(x => (x.Name, x.ConditionArray, x.ReturnCode)));
        }

        public override void SubComponentChangesApplied(Component sub)
        {
            _notify_property_changed("Summary");
        }

        public override void SubComponentChangesHappened(Component sub)
        {
            _notify_property_changed("Summary");
        }

        public void MoveBranch(int oldIndex, int newIndex)
        {
            var temp = __branches[oldIndex];
            __branches.RemoveAt(oldIndex);
            __branches.Insert(newIndex, temp);

            _notify_property_changed("Summary");
        }

        public void AddBranch()
        {
            __branches.Add(new SimpleExceptionResponseBranchModel() { Owner = this });
            _notify_property_changed("Summary");
        }

        public void InsertBranch(int pos)
        {
            __branches.Insert(pos, new SimpleExceptionResponseBranchModel() { Owner = this });
            _notify_property_changed("Summary");
        }

        public void RemoveBranchAt(int pos)
        {
            __branches.RemoveAt(pos);
            _notify_property_changed("Summary");
        }
    }
}
