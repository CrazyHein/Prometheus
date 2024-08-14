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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.Globals
{
    public class GlobalEvents
    {
        private Dictionary<uint, Event> __global_event_storage = new Dictionary<uint, Event>();
        public IReadOnlyDictionary<uint, Event> Events { get { return __global_event_storage; } }

        private Dictionary<uint, string> __global_event_names = new Dictionary<uint, string>();
        public IReadOnlyDictionary<uint, string> Names { get { return __global_event_names; } }

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
