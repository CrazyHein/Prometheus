using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim
{
    public class LocalEventModelCollection
    {
        private Component __owner;
        private ObservableCollection<LocalEventModel> __events;
        private string __summary;

        public IEnumerable<LocalEventModel> Events => __events;
        public LocalEventModelCollection(Component owner, IEnumerable<LocalEventModel> locals, string summaryName = "Summary")
        {
            if (locals.Any(x => x.Owner != owner))
                throw new ArgumentException($"Can not add local event with different owner.");
            __owner = owner;
            __events = new ObservableCollection<LocalEventModel>(locals);
            __summary = summaryName;
        }
        public void Add(LocalEventModel evt, bool updateSummay)
        {
            if (__events.Any(x => x.Index == evt.Index))
                throw new ArgumentException($"The local event with the same index({evt.Index}) has existed already.");
            if(evt.Owner != __owner)
                throw new ArgumentException($"Can not add a local event with different owner.");
            __events.Add(evt);
            if (updateSummay)
                __owner._notify_property_changed(__summary);
        }

        public void Add(bool updateSummay)
        {
            uint idx = 0;
            if (__events.Count != 0)
                idx = __events.Max(x => x.Index) + 1;
            LocalEventModel evt = new LocalEventModel(idx, "unnamed") { Owner = __owner };
            __events.Add(evt);
            if (updateSummay)
                __owner._notify_property_changed(__summary);
        }

        public void Add(JsonArray array, bool updateSummay)
        {
            foreach (var evt in array.Select(x => new LocalEventModel(x.AsObject()) { Owner = __owner }))
                __events.Add(evt);
            if (updateSummay)
                __owner._notify_property_changed(__summary);
        }

        public void Insert(int pos, LocalEventModel evt, bool updateSummay)
        {
            if (__events.Any(x => x.Index == evt.Index))
                throw new ArgumentException($"The local event with the same index({evt.Index}) has existed already.");
            if (evt.Owner != __owner)
                throw new ArgumentException($"Can not insert a local event with different owner.");
            __events.Insert(pos, evt);
            if (updateSummay)
                __owner._notify_property_changed(__summary);
        }

        public void Insert(int pos, bool updateSummay)
        {
            uint idx = 0;
            if (__events.Count != 0)
                idx = __events.Max(x => x.Index) + 1;
            LocalEventModel evt = new LocalEventModel(idx, "unnamed") { Owner = __owner };
            __events.Insert(pos, evt);
            if (updateSummay)
                __owner._notify_property_changed(__summary);
        }

        public void Insert(int pos, JsonArray array, bool updateSummay)
        {
            foreach (var evt in array.Select(x => new LocalEventModel(x.AsObject()) { Owner = __owner }).Reverse())
                __events.Insert(pos, evt);
            if (updateSummay)
                __owner._notify_property_changed(__summary);
        }

        public void Remove(uint index, bool updateSummay)
        {
            if (__events.Any(x => x.Index == index) == false)
                throw new ArgumentException($"The local event with the specified index({index}) does not exist.");
            var e = __events.First(x => x.Index == index);
            e.Owner = null;
            __events.Remove(e);
            if (updateSummay)
                __owner._notify_property_changed(__summary);
        }

        public void RemoveAt(int pos, bool updateSummay)
        {
            __events[pos].Owner = null;
            __events.RemoveAt(pos);
            if (updateSummay)
                __owner._notify_property_changed(__summary);
        }

        public void Remove(IEnumerable<LocalEventModel> locals, bool updateSummay)
        {
            List<LocalEventModel> temp = new List<LocalEventModel>(locals);
            foreach (var local in temp)
                __events.Remove(local);
            if (updateSummay)
                __owner._notify_property_changed(__summary);
        }

        public void Clear(bool updateSummay)
        {
            __events.Clear();
            if(updateSummay)
                __owner._notify_property_changed(__summary);
        }
    }
}
