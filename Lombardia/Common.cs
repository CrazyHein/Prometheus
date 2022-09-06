using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia
{
    public interface IComparable<T>
    {
        bool IsEquivalent(T? other);
    }
    public interface ISubscriber<T> where T : notnull
    {
        public ISubscriber<T>? DependencyChanged(T origin, T newcome);
    }

    public enum ProcessDataImageLayout
    {
        Bit,
        Block,
        System
    }

    public enum ProcessDataImageAccess
    {
        RX,
        TX
    }

    public enum ReplaceMode
    {
        Full,
        //Half,
        DeviceConfiguration,
        Variable,
        ProcessObject,
        ProcessData
    }

    public enum LogicOperator
    {
        AND,
        OR,
        NOT,
        XOR,
        NAND,
        NOR
    }

    public enum LogicElementType
    {
        OPERAND,
        EXPRESSION
    }

    public enum LogicOperandArea
    {
        RX, 
        TX
    }

    public enum CONSTANTS : int
    {
        MAX_LAYER_OF_NESTED_LOGIC = 5
    }

    public abstract class Publisher<T> where T : notnull
    {
        protected Dictionary<T, List<ISubscriber<T>>> _subscribers = new Dictionary<T, List<ISubscriber<T>>>();
        public virtual bool AddSubscriber(T key, ISubscriber<T> subscriber)
        {
            if (_subscribers.TryGetValue(key, out var subs) == false)
                subs = new List<ISubscriber<T>>();
            subs.Add(subscriber);
            _subscribers[key] = subs;
            return true;
        }
        public virtual bool RemoveSubscriber(T key, ISubscriber<T> subscriber)
        {
            if(_subscribers.TryGetValue(key, out var subs))
            {
                if (subs.Remove(subscriber))
                {
                    if(subs.Count == 0)
                        _subscribers.Remove(key);
                    return true;
                }
            }
            return false;
        }
        public virtual IReadOnlyList<ISubscriber<T>>? CurrentSubscribers(T key)
        {
            _subscribers.TryGetValue(key, out var res);
            return res;
        }

        public virtual IEnumerable<T> KeyCollection
        {
            get
            {
                return _subscribers.Keys;
            }
        }
    }

    class Helper
    {
        public static Regex VALID_IPV4_ADDRESS { get; private set; } = new Regex(@"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$", RegexOptions.Compiled);
        public static Regex VALID_MODULE_LOCAL_ADDRESS { get; private set; } = new Regex(@"^0x[0-9A-F]{3}0$", RegexOptions.Compiled);
        public static bool OVERLAP_DETECTOR(List<ValueTuple<uint, uint>> ranges)
        {
            for (int i = 0; i < ranges.Count - 1; ++i)
            {
                for (int j = 0; j < ranges.Count - 1 - i; ++j)
                {
                    if (ranges[j].Item1 > ranges[j + 1].Item1)
                    {
                        ValueTuple<uint, uint> temp = ranges[j];
                        ranges[j] = ranges[j + 1];
                        ranges[j + 1] = temp;
                    }
                }
            }

            for (int i = 0; i < ranges.Count - 1; ++i)
            {
                if (ranges[i].Item1 + ranges[i].Item2 > ranges[i + 1].Item1)
                    return true;
            }

            return false;
        }
    }
}
