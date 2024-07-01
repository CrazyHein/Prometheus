using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.StepMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim
{
    public class EventModel : Component
    {
        public static Dictionary<string, string> EventIndex;
        public static IEnumerable<string> EventTypes { get; }

        public override string Header => "EVENT";

        public override BitmapImage ImageIcon => new BitmapImage(new Uri("pack://application:,,,/imgs/tag.png"));

        public Event Event { get; protected set; }
        protected Event? _event_for_rollback;

        private string __type;
        public string Type 
        { 
            get { return __type; } 
            set 
            {
                if(__type != value)
                {
                    __type = value;

                    __event_parameters.Clear();
                    foreach (var p in Event.EventDefaultParameters[__type])
                        __event_parameters.Add(new EventParameter(p.pname, p.pdefault, p.required, p.comment, this));

                    _notify_property_changed();
                    _reload_property("Info");
                    _reload_property("Summary");
                }
            } 
        }
        public string Info { get { return $"{Type}: {EventIndex[Type]}"; } }

        protected ObservableCollection<EventParameter> __event_parameters = new ObservableCollection<EventParameter>();
        public IEnumerable<EventParameter> EventParameters => __event_parameters;

        public override string Summary
        {
            get
            {
                string summary;
                try
                {
                    if (Modified)
                        summary = ToEvent().ToString();
                    else
                        summary = Event.ToString();
                }
                catch (Exception ex)
                {
                    summary = ex.ToString();
                }
                return summary;
            }
        }

        static EventModel()
        {
            EventIndex = new Dictionary<string, string>();
            foreach (var x in Event.CompatibleEventTypes)
                EventIndex[x.Key] = x.Value;
            EventTypes = EventIndex.Keys;
        }

        public EventModel()
        {
            __type = EventTypes.First();
            __event_parameters = new ObservableCollection<EventParameter>();
            foreach (var p in Event.EventDefaultParameters[__type])
                __event_parameters.Add(new EventParameter(p.pname, p.pdefault, p.required, p.comment, this));
            Event = Event.MAKE(__type, __event_parameters.Where(x => x.Required).Select(x => (x.Name, new Expression(x.Value, null))).ToArray());
        }

        public EventModel(Event evt)
        {
            Event = evt;
            __type = evt.Tag;
            __event_parameters = new System.Collections.ObjectModel.ObservableCollection<EventParameter>(
                evt.ParameterSettings.Join(evt.ParameterDescriptions, x => x.pname, y => y.pname, (x, y) => (x, y)).Select(x => new EventParameter(x.x.pname, x.x.pvalue, x.y.required, x.y.comment, this)));
        }

        public EventModel(string name, params (string pname, Expression pvalue)[] parameters)
        {
            Event = Event.MAKE(name, parameters);
            __type = Event.Tag;
            __event_parameters = new System.Collections.ObjectModel.ObservableCollection<EventParameter>(
                Event.ParameterSettings.Join(Event.ParameterDescriptions, x => x.pname, y => y.pname, (x, y) => (x, y)).Select(x => new EventParameter(x.x.pname, x.x.pvalue, x.y.required, x.y.comment, this)));
        }

        public override JsonNode ToJson()
        {
            //if (Modified)
            //throw new InvalidOperationException("Unapplied changes detected.");
            var json = new JsonObject();
            json["VERSION"] = Settings.ArkVersion;
            json["ASSEMBLY"] = this.GetType().FullName;
            json["SOURCE"] = ToEvent().ToJson();
            return json;
        }


        public Event ToEvent()
        {
            return Event.MAKE(__type, __event_parameters.Where(x => String.IsNullOrEmpty(x.Value) == false).Select(x => (x.Name, new Expression(x.Value, null))).ToArray());
        }

        public override void ApplyChanges()
        {
            if (Modified)
            {
                _event_for_rollback = Event;
                Event = Event.MAKE(__type, EventParameters.Where(x => x.Value != string.Empty).Select(x => (x.Name, new Expression(x.Value, null))).ToArray());
                base.ApplyChanges();
            }
        }

        public override void DiscardChanges()
        {
            if (Modified)
            {
                __type = Event.Tag;
                __event_parameters.Clear();
                foreach (var p in Event.ParameterSettings.Join(Event.ParameterDescriptions, x => x.pname, y => y.pname, (x, y) => (x, y)).Select(x => new EventParameter(x.x.pname, x.x.pvalue, x.y.required, x.y.comment, this)))
                    __event_parameters.Add(p);
                _reload_property("Type");
                _reload_property("Summary");
                base.DiscardChanges();
            }
        }

        public void PropertyValueChanged(string propertyName, string propertyValue)
        {
            _notify_property_changed("Summary");
        }

        public override void SubComponentChangesApplied(Component sub)
        {
            throw new NotSupportedException();
        }

        public override void SubComponentChangesHappened(Component sub)
        {
            throw new NotSupportedException();
        }

        protected override void RollbackChanges()
        {
            if (Validated)
            {
                Event = _event_for_rollback;
            }
        }
    }

    public class EventParameter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void _notify_property_changed([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public EventModel Event { get; init; }

        public string Name { get; }

        private string __value;
        public string Value
        {
            get { return __value; }
            set
            {
                if(value != __value)
                {
                    __value = value;
                    _notify_property_changed();
                    _notify_property_changed("ValueWithTags");
                    Event.PropertyValueChanged("Value", value);
                }
            }
        }

        public bool Required { get; }
        public string Comment { get; }

        public string ValueWithTags 
        { 
            get 
            {
                return ContextModel.PROCESS_DATA_INDEX_TO_TAG(Value);
            }
        }

        public EventParameter(string name, string value, bool required, string comment, EventModel owner)
        {
            Name = name;
            __value = value;
            Required = required;
            Comment = comment;
            Event = owner;
        }
    }
}
