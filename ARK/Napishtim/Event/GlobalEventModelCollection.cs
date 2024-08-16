using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim
{
    public class GlobalEventModelCollection : ComponentCollection<GlobalEventModel>
    {
        public RecipeDocument RecipeDocument { get; }
        private ObservableCollection<GlobalEventModel> __events;
        public IEnumerable<GlobalEventModel> Events { get; }
        public override IEnumerable<GlobalEventModel> Components { get; }

        public GlobalEventModelCollection(RecipeDocument doc)
        {
            RecipeDocument = doc;
            __events = new ObservableCollection<GlobalEventModel>(doc.GlobalEvents.Select(x => new GlobalEventModel(this, x.Key, doc.GlobalEventNames[x.Key], x.Value) { Owner = null}));
            Events = __events;
            Components = __events;
        }

        public override string Summary
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach(var evt in __events)
                {
                    sb.Append(evt.Summary);
                    sb.Append("\n");
                }
                return sb.ToString();
            }
        }

        public GlobalEventModel Add()
        {
            uint idx = 0;
            if(RecipeDocument.GlobalEvents.Count != 0)
                idx = RecipeDocument.GlobalEvents.Keys.Max() + 1;
            GlobalEventModel evt = new GlobalEventModel(this, idx, "unnamed_global_event") { Owner = null };
            RecipeDocument.AddGlobalEvent(idx, evt.Name, evt.Event.Type, evt.Event.EventParameters.Where(x => x.Value != string.Empty).Select(x => (x.Name, x.Value)).ToArray());
            __events.Add(evt);
            IsDirty = true;
            return evt;
        }

        public GlobalEventModel InsertBefore(int pos)
        {
            uint idx = 0;
            if (RecipeDocument.GlobalEvents.Count != 0)
                idx = RecipeDocument.GlobalEvents.Keys.Max() + 1;
            GlobalEventModel evt = new GlobalEventModel(this, idx, "unnamed_global_event") { Owner = null };
            RecipeDocument.AddGlobalEvent(idx, evt.Name, evt.Event.Type, evt.Event.EventParameters.Where(x => x.Value != string.Empty).Select(x => (x.Name, x.Value)).ToArray());
            __events.Insert(pos, evt);
            IsDirty = true;
            return evt;
        }

        public void Remove(GlobalEventModel evt)
        {
            RecipeDocument.RemoveGlobalEvent(evt.Index);
            __events.Remove(evt);
            IsDirty = true;
        }

        public void MoveBefore(GlobalEventModel source, GlobalEventModel target)
        {
            __events.Remove(source);
            int pos = __events.IndexOf(target);
            __events.Insert(pos, source);
            IsDirty = true;
        }

        public void MoveAfter(GlobalEventModel source, GlobalEventModel target)
        {
            __events.Remove(source);
            int pos = __events.IndexOf(target);
            if(pos == __events.Count - 1)
                __events.Add(source);
            else
                __events.Insert(pos + 1, source);
            IsDirty = true;
        }

        public bool ContainsKey(uint idx)
        {
            return RecipeDocument.GlobalEvents.ContainsKey(idx);
        }

        public GlobalEventModel Add(JsonNode node)
        {
            if (node["VERSION"].GetValue<string>() != Settings.ArkVersion || node["ASSEMBLY"].GetValue<string>() != typeof(GlobalEventModel).FullName)
                throw new ArgumentException("The input json node is not a valid Json object that encapsulated a Global Event.");
            uint idx = 0;
            if (RecipeDocument.GlobalEvents.Count != 0)
                idx = RecipeDocument.GlobalEvents.Keys.Max() + 1;
            GlobalEventModel evt = new GlobalEventModel(this, idx, node["NAME"].GetValue<string>(), node["EVENT"]) { Owner = null };
            RecipeDocument.AddGlobalEvent(idx, evt.Name, evt.Event.Type, evt.Event.EventParameters.Where(x => x.Value != string.Empty).Select(x => (x.Name, x.Value)).ToArray());
            __events.Add(evt);
            IsDirty = true;
            return evt;
        }

        public GlobalEventModel InsertBefore(int pos, JsonNode node)
        {
            if (node["VERSION"].GetValue<string>() != Settings.ArkVersion || node["ASSEMBLY"].GetValue<string>() != typeof(GlobalEventModel).FullName)
                throw new ArgumentException("The input json node is not a valid Json object that encapsulated a Global Event.");
            uint idx = 0;
            if (RecipeDocument.GlobalEvents.Count != 0)
                idx = RecipeDocument.GlobalEvents.Keys.Max() + 1;
            GlobalEventModel evt = new GlobalEventModel(this, idx, node["NAME"].GetValue<string>(), node["EVENT"]) { Owner = null };
            RecipeDocument.AddGlobalEvent(idx, evt.Name, evt.Event.Type, evt.Event.EventParameters.Where(x => x.Value != string.Empty).Select(x => (x.Name, x.Value)).ToArray());
            __events.Insert(pos, evt);
            IsDirty = true;
            return evt;
        }
    }
}
