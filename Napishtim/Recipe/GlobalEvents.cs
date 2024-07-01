using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe
{
    public class GlobalEvents
    {
        private Dictionary<uint, Event> __global_event_storage = new Dictionary<uint, Event>();
        public IReadOnlyDictionary<uint, Event> Events { get { return __global_event_storage; } }

        private Dictionary<uint, string> __global_event_names = new Dictionary<uint, string>();
        public IReadOnlyDictionary<uint, string> Names { get { return __global_event_names; } }

        private SortedDictionary<uint, int> __global_event_reference = new SortedDictionary<uint, int>();

        public void AddEventReference(uint eventIdx)
        {
            if(__global_event_storage.ContainsKey(eventIdx) == false)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"GEVENT with index({eventIdx}) does not exist.");

            if (__global_event_reference.ContainsKey(eventIdx))
                __global_event_reference[eventIdx]++;
            else
                __global_event_reference[eventIdx] = 1;
        }

        public void AddEventReference(IEnumerable<uint> eventIdxes)
        {
            if(eventIdxes.Any(x => __global_event_storage.ContainsKey(x) == false))
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"At least one GEVENT does not exist.");
            foreach (var idx in eventIdxes)
                AddEventReference(idx);
        }

        public void RemoveEventReference(uint eventIdx)
        {
            if (__global_event_storage.ContainsKey(eventIdx))
            {
                if (__global_event_reference.ContainsKey(eventIdx))
                {
                    if(--__global_event_reference[eventIdx] == 0)
                        __global_event_reference.Remove(eventIdx);
                }
                else
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"GEVENT with index({eventIdx}) has not been referenced elsewhere.");
            }
            else
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"GEVENT with index({eventIdx}) does not exist.");
        }

        public void RemoveEventReference(IEnumerable<uint> eventIdxes)
        {
            eventIdxes.Any(x => __global_event_storage.ContainsKey(x) == false || __global_event_reference.ContainsKey(x) == false || __global_event_reference[x] < eventIdxes.Count(y => x == y));
            if(eventIdxes.Any(x => __global_event_storage.ContainsKey(x) == false))
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"At least one GEVENT does not exist.");
            if(eventIdxes.Any(x =>__global_event_reference.ContainsKey(x) == false))
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"At least one GEVENT has been referenced elsewhere.");
            if(eventIdxes.Any(x => __global_event_reference[x] < eventIdxes.Count(y => x == y)))
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"At least one GEVENT's reference count is not enough for removing operation.");
            foreach (var idx in eventIdxes)
                RemoveEventReference(idx);
        }

        public void AddEvent(uint idx, string name, string type, params (string pname, string pvalue)[]? parameters)
        {
            if (__global_event_storage.ContainsKey(idx))
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION,
                    $"The global event with the same index({idx}) has existed:\n{__global_event_names[idx]}\n{__global_event_storage[idx].ToJson().ToString()}");
            __global_event_storage[idx] = (Event.MAKE(type, parameters.Select(n => (n.pname, new Expression(n.pvalue, null))).ToArray()));
            __global_event_names[idx] = name;
        }

        public void AddEvent(uint idx, string name, JsonObject evt)
        {
            if (__global_event_storage.ContainsKey(idx))
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION,
                    $"The global event with the same index({idx}) has existed:\n{__global_event_names[idx]}\n{__global_event_storage[idx].ToJson().ToString()}");
            __global_event_storage[idx] = Event.MAKE(evt);
            __global_event_names[idx] = name;
        }

        public void AddEvent(uint idx, string name, Event evt)
        {
            if (__global_event_storage.ContainsKey(idx))
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION,
                    $"The global event with the same index({idx}) has existed:\n{__global_event_names[idx]}\n{__global_event_storage[idx].ToJson().ToString()}");
            __global_event_storage[idx] = evt;
            __global_event_names[idx] = name;
        }

        public void RemoveEvent(uint idx)
        {
            if (__global_event_storage.ContainsKey(idx))
            {
                if (__global_event_reference.ContainsKey(idx) == true)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"GEVENT with index({idx}) has been referenced elsewhere and cannot be deleted directly.");
                else
                {
                    __global_event_storage.Remove(idx);
                    __global_event_names.Remove(idx);
                }
            }
            else
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"GEVENT with index({idx}) does not exist.");
        }

        public void ReplaceEvent(uint idx, string name, string type, params (string pname, string pvalue)[]? parameters)
        {
            if (__global_event_storage.ContainsKey(idx) == false)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"GEVENT with index({idx}) does not exist.");
            __global_event_storage[idx] = Event.MAKE(type, parameters.Select(n => (n.pname, new Expression(n.pvalue, null))).ToArray());
            __global_event_names[idx] = name;
        }

        public void ReplaceEvent(uint idx, uint nidx, string name, string type, params (string pname, string pvalue)[]? parameters)
        {
            if (idx == nidx)
                ReplaceEvent(idx, name, type, parameters);
            else
            {
                if (__global_event_storage.ContainsKey(idx) == false)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"GEVENT with index({idx}) does not exist.");
                if (__global_event_reference.ContainsKey(idx) == true)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"GEVENT with index({idx}) has been referenced elsewhere and cannot be deleted directly.");
                if (__global_event_storage.ContainsKey(nidx) == true)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"GEVENT with index({nidx}) already existed.");
                __global_event_storage.Remove(idx);
                __global_event_names.Remove(idx);
                __global_event_storage[nidx] = Event.MAKE(type, parameters.Select(n => (n.pname, new Expression(n.pvalue, null))).ToArray());
                __global_event_names[nidx] = name;
            }
        }

        public void ReplaceEvent(uint idx, string name, JsonObject evt)
        {
            if (__global_event_storage.ContainsKey(idx) == false)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"GEVENT with index({idx}) does not exist.");
            __global_event_storage[idx] = Event.MAKE(evt);
            __global_event_names[idx] = name;
        }

        public void ReplaceEvent(uint idx, string name, Event evt)
        {
            if (__global_event_storage.ContainsKey(idx) == false)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"GEVENT with index({idx}) does not exist.");
            __global_event_storage[idx] = evt;
            __global_event_names[idx] = name;
        }

        public void ReplaceEvent(uint idx, uint nidx, string name, Event evt)
        {
            if (idx == nidx)
                ReplaceEvent(idx, name, evt);
            else
            {
                if (__global_event_storage.ContainsKey(idx) == false)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"GEVENT with index({idx}) does not exist.");
                if (__global_event_reference.ContainsKey(idx) == true)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"GEVENT with index({idx}) has been referenced elsewhere and cannot be deleted directly.");
                if (__global_event_storage.ContainsKey(nidx) == true)
                    throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"GEVENT with index({nidx}) already existed.");
                __global_event_storage.Remove(idx);
                __global_event_names.Remove(idx);
                __global_event_storage[nidx] = evt;
                __global_event_names[nidx] = name;
            }
        }

        public void RemoveAllGlobalEvents()
        {
            bool referened = __global_event_storage.Keys.Any(e => __global_event_reference.ContainsKey(e) == true);
            if (referened)
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"At least one GEVENT has been referenced elsewhere and cannot be deleted directly.");
            else
            {
                __global_event_storage.Clear();
            }
        }

        public bool Contains(uint idx) => __global_event_storage.ContainsKey(idx);

        public JsonArray ToJson()
        {
            JsonArray globalEvents = new JsonArray();
            foreach(var evt in Events)
            {
                JsonObject eventNode = new JsonObject();
                eventNode["ID"] = evt.Key;
                eventNode["NAME"] = __global_event_names[evt.Key];
                eventNode["EVENT"] = evt.Value.ToJson();
                globalEvents.Add(eventNode);
            }
            return globalEvents;
        }

        public JsonObject ToJson(uint idx)
        {
            if (__global_event_storage.ContainsKey(idx))
            {
                JsonObject eventNode = new JsonObject();
                eventNode["ID"] = idx;
                eventNode["NAME"] = __global_event_names[idx];
                eventNode["EVENT"] = __global_event_storage[idx].ToJson();
                return eventNode;
            }
            else
                throw new NaposhtimDocumentException(NaposhtimExceptionCode.DOCUMENT_INVALID_OPERATION, $"GEVENT with index({idx}) does not exist.");
        }
    }
}
