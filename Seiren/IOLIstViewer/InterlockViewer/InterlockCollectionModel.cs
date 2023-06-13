using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    public class InterlockCollectionModel : RecordContainerModel
    {
        private InterlockCollection __interlock_collection;
        private ObjectsModel __objects_source;
        //private ObservableCollection<InterlockLogic> __interlock_logic_models;
        private ObservableCollection<InterlockLogicModel> __interlock_logic_models;
        private ProcessDataImage __tx_bit_data_image;
        private ProcessDataImage __rx_bit_data_image;
        //public IReadOnlyList<InterlockLogic> InterlockLogicModels { get; private set; }
        public IReadOnlyList<InterlockLogicModel> InterlockLogicModels { get; private set; }

        public bool IgnoreHardwareIntlks
        {
            set
            {
                if (value && (__interlock_collection.IgnoredAttribute & (uint)InterlockAttribute.Hardware) == 0)
                {
                    __interlock_collection.IgnoredAttribute |= (uint)InterlockAttribute.Hardware;
                    OnPropertyChanged("IgnoreHardwareIntlks");
                    Modified = true;
                    __objects_source.SubsModified = true;
                }
                else if (!value && (__interlock_collection.IgnoredAttribute & (uint)InterlockAttribute.Hardware) != 0)
                {
                    __interlock_collection.IgnoredAttribute &= (uint)~InterlockAttribute.Hardware;
                    OnPropertyChanged("IgnoreHardwareIntlks");
                    Modified = true;
                    __objects_source.SubsModified = true;
                }
            }
            get
            {
                return (__interlock_collection.IgnoredAttribute & (uint)InterlockAttribute.Hardware) != 0;
            }
        }

        public bool IgnoreExclusiveIntlks
        {
            set
            {
                if (value && (__interlock_collection.IgnoredAttribute & (uint)InterlockAttribute.Exclusive) == 0)
                {
                    __interlock_collection.IgnoredAttribute |= (uint)InterlockAttribute.Exclusive;
                    OnPropertyChanged("IgnoreExclusiveIntlks");
                    Modified = true;
                    __objects_source.SubsModified = true;
                }
                else if (!value && (__interlock_collection.IgnoredAttribute & (uint)InterlockAttribute.Exclusive) != 0)
                {
                    __interlock_collection.IgnoredAttribute &= (uint)~InterlockAttribute.Exclusive;
                    OnPropertyChanged("IgnoreExclusiveIntlks");
                    Modified = true;
                    __objects_source.SubsModified = true;
                }
            }
            get
            {
                return (__interlock_collection.IgnoredAttribute & (uint)InterlockAttribute.Exclusive) != 0;
            }
        }
        private bool __is_offline = true;
        public bool IsOffline
        {
            get { return __is_offline; }
            set
            {
                if (value != __is_offline)
                {
                    __is_offline = value;
                    OnPropertyChanged("IsOffline");
                }
            }
        }
        public InterlockCollectionModel(InterlockCollection ic, ObjectDictionary od, ProcessDataImage txbit, ProcessDataImage rxbit, ObjectsModel objectsSource, OperatingHistory history)
        {
            __interlock_collection = ic;
            //__interlock_logic_models = new ObservableCollection<InterlockLogic>(ic.Logics);
            __interlock_logic_models = new ObservableCollection<InterlockLogicModel>(ic.Logics.Select(l => new InterlockLogicModel(l)));
            __tx_bit_data_image = txbit;
            __rx_bit_data_image= rxbit;
            InterlockLogicModels = __interlock_logic_models;
            __objects_source = objectsSource;
            Modified = false;
            OperatingHistory = history;
            Name = "Interlock Area";
        }

        public void UpdateInterlockLogic()
        {
            for (int i = 0; i < __interlock_logic_models.Count; ++i)
            {
                //if (__interlock_logic_models[i] != __interlock_collection.Logics[i])
                if (__interlock_logic_models[i].Logic != __interlock_collection.Logics[i])
                {
                    //__interlock_logic_models[i] = __interlock_collection.Logics[i];
                    __interlock_logic_models[i] = new InterlockLogicModel(__interlock_collection.Logics[i]);
                    Modified = true;
                    __objects_source.SubsModified = true;
                }
            }
        }

        public void Add(uint attr, string name, string target, string statement, bool log = true)
        {
            //__interlock_logic_models.Add(__interlock_collection.Add(name, target, statement));
            __interlock_logic_models.Add(new InterlockLogicModel(__interlock_collection.Add(attr, name, target, statement)));
            Modified = true;
            __objects_source.SubsModified = true;
            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(new OperatingRecord() { Host = this, Operation = Operation.Add, OriginaPos = -1, NewPos = __interlock_logic_models.Count - 1, OriginalValue = null, 
                    NewValue = new Tuple<string, string, string>(name, target, statement) });
        }

        public void Insert(int index, uint attr, string name, string target, string statement, bool log = true)
        {
            //__interlock_logic_models.Insert(index, __interlock_collection.Insert(index, name, target, statement));
            __interlock_logic_models.Insert(index, new InterlockLogicModel(__interlock_collection.Insert(index, attr, name, target, statement)));
            Modified = true;
            __objects_source.SubsModified = true;
            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(new OperatingRecord() { Host = this, Operation = Operation.Insert, OriginaPos = -1, NewPos = index, OriginalValue = null, 
                    NewValue = new Tuple<string, string, string>(name, target, statement) });
        }

        public void Remove(int index, bool log = true)
        {
            var logic = __interlock_collection.Remove(index);
            __interlock_logic_models.RemoveAt(index);
            Modified = true;
            __objects_source.SubsModified = true;
            if (log && OperatingHistory != null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var t in logic.Targets)
                {
                    sb.Append("0x").Append(t.ProcessObject.Index.ToString("X08"));
                    sb.Append("\r\n");
                }
                var v = new Tuple<string, string, string>(logic.Name, sb.ToString(), logic.Statement.Serialize());
                OperatingHistory.PushOperatingRecord(new OperatingRecord() { Host = this, Operation = Operation.Remove, OriginaPos = index, NewPos = -1, OriginalValue = v, NewValue = null });
            }
        }

        public void Replace(int index, uint attr, string name, string target, string statement, bool log = true)
        {
            //__interlock_logic_models[index] = __interlock_collection.Replace(index, name, target, statement);
            var logic = __interlock_collection.Logics[index];
            __interlock_logic_models[index] = new InterlockLogicModel(__interlock_collection.Replace(index, attr, name, target, statement));
            Modified = true;
            __objects_source.SubsModified = true;
            if (log && OperatingHistory != null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var t in logic.Targets)
                {
                    sb.Append("0x").Append(t.ProcessObject.Index.ToString("X08"));
                    sb.Append("\r\n");
                }
                var v = new Tuple<string, string, string>(logic.Name, sb.ToString(), logic.Statement.Serialize());
                OperatingHistory?.PushOperatingRecord(new OperatingRecord() { Host = this, Operation = Operation.Replace, OriginaPos = index, NewPos = index, 
                    OriginalValue = v, 
                    NewValue = new Tuple<string, string, string>(name, target, statement) });
            }
        }

        public void Move(int srcIndex, int dstIndex, bool log = true)
        {
            if (srcIndex >= __interlock_logic_models.Count || dstIndex >= __interlock_logic_models.Count || srcIndex < 0 || dstIndex < 0)
                throw new ArgumentOutOfRangeException();
            __interlock_collection.Move(srcIndex, dstIndex);
            var temp = __interlock_logic_models[srcIndex];
            __interlock_logic_models.RemoveAt(srcIndex);
            __interlock_logic_models.Insert(dstIndex, temp);
            Modified = true;
            __objects_source.SubsModified = true;
            if (log && OperatingHistory != null)
                OperatingHistory.PushOperatingRecord(new OperatingRecord() { Host = this, Operation = Operation.Move, OriginaPos = srcIndex, NewPos = dstIndex, OriginalValue = temp, NewValue = temp });
        }

        public void Save(XmlDocument doc, XmlElement root)
        {
            __interlock_collection.Save(doc, root);
            Modified = false;
        }

        public void ProcessDataValueChanged(ushort[] tx, ushort[] rx)
        {
            foreach (var logic in __interlock_logic_models)
                logic.Eval(tx, rx, __interlock_collection.IgnoredAttribute);
        }

        public void ResetProcessDataValue()
        {
            //reset bit offset and so on
            foreach (var logic in __interlock_logic_models)
                logic.Rebuild();
        }

        public override void Undo(OperatingRecord r)
        {
            switch (r.Operation)
            {
                case Operation.Add:
                    Remove(r.NewPos, false);
                    break;
                case Operation.Remove:
                    var v = r.OriginalValue as Tuple<uint, string, string, string>;
                    if (r.OriginaPos == __interlock_logic_models.Count)
                        Add(v.Item1, v.Item2, v.Item3, v.Item4, false);
                    else
                        Insert(r.OriginaPos, v.Item1, v.Item2, v.Item3, v.Item4, false);
                    break;
                case Operation.Insert:
                    Remove(r.NewPos, false);
                    break;
                case Operation.Replace:
                    v = r.OriginalValue as Tuple<uint, string, string, string>;
                    Replace(r.NewPos, v.Item1, v.Item2, v.Item3, v.Item4, false);
                    break;
                case Operation.Move:
                    Move(r.NewPos, r.OriginaPos, false);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public override void Redo(OperatingRecord r)
        {
            switch (r.Operation)
            {
                case Operation.Add:
                    var v = r.NewValue as Tuple<uint, string, string, string>;
                    Add(v.Item1, v.Item2, v.Item3, v.Item4, false);
                    break;
                case Operation.Remove:
                    Remove(r.OriginaPos, false);
                    break;
                case Operation.Insert:
                    v = r.NewValue as Tuple<uint, string, string, string>;
                    Insert(r.NewPos, v.Item1, v.Item2, v.Item3, v.Item4, false);
                    break;
                case Operation.Replace:
                    v = r.NewValue as Tuple<uint, string, string, string>;
                    Replace(r.OriginaPos, v.Item1, v.Item2, v.Item3, v.Item4, false);
                    break;
                case Operation.Move:
                    Move(r.OriginaPos, r.NewPos, false);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public int FindNext(uint index)
        {
            return __interlock_collection.FindNext(index);
        }
    }

    public abstract class LogicElementModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void _notify_property_changed([CallerMemberName] String propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Display { get; protected set; }
        public LogicElementType Type { get; private set; }
        public LogicExpressionModel? Root { get; private set; }
        //public int Layer { get; private set; }

        private bool __value = false;
        public bool Value
        {
            get { return __value; }
            set 
            { 
                if (value != __value)
                {
                    __value = value;
                    _notify_property_changed();
                }
            }
        }

        public abstract bool Eval(ushort[] tx, ushort[] rx);

        public LogicElementModel(LogicElementType type, LogicExpressionModel root)
        {
            Type = type;
            Root = root;
            /*
            if (root == null)
                Layer = 0;
            else
                Layer = root.Layer + 1;
            */
        }
    }

    public class LogicOperandModel : LogicElementModel
    {
        public uint BitPos { get; set; }
        public ProcessDataImageAccess Acess { get; set; }
        public LogicOperandModel(ProcessData data, LogicExpressionModel root) : base(LogicElementType.OPERAND, root)
        {
            BitPos = data.BitPos;
            Acess = data.Access;
            Display = data.ToString();
        }

        public override bool Eval(ushort[] tx, ushort[] rx)
        {
            ushort bitpos;
            if (Acess == ProcessDataImageAccess.TX)
            {
                bitpos = (ushort)(1 << (int)(BitPos % 16));
                if ((tx[BitPos / 16] & bitpos) == 0)
                    Value = false;
                else
                    Value = true;
            }
            else
            {
                bitpos = (ushort)(1 << (int)(BitPos % 16));
                if ((rx[BitPos / 16] & bitpos) == 0)
                    Value = false;
                else
                    Value = true;
            }
            return Value;
        }
    }

    public class LogicTargetModel : LogicOperandModel
    {
        private bool __warning = false;
        public bool Warning
        {
            get { return __warning; }
            set 
            {
                if (value != __warning)
                {
                    __warning = value;
                    _notify_property_changed();
                }
            }
        }
        private bool __ignored = false;
        public bool Ignored
        {
            get { return __ignored; }
            set
            {
                if (value != __ignored)
                {
                    __ignored = value;
                    _notify_property_changed();
                }
            }
        }

        public LogicTargetModel(ProcessData data, LogicExpressionModel root) : base(data, root)
        {
            Display = data.ToString();
        }
    }

    public class LogicExpressionModel : LogicElementModel
    {
        public List<LogicElementModel> Elements { get; private set; }
        public LogicOperator Operator { get; private set; }
        public LogicExpressionModel(LogicOperator op, LogicExpressionModel root) : base(LogicElementType.EXPRESSION, root)
        {
            Elements = new List<LogicElementModel>();
            Operator = op;
            Display = op.ToString();
        }

        public override bool Eval(ushort[] tx, ushort[] rx)
        {
            bool res = false;
            if (Elements.Count == 0)
            {
                Value = false;
                return false;
            }
            else
                res = Elements[0].Eval(tx, rx);

            for (int i = 1; i < Elements.Count; ++i)
            {
                switch (Operator)
                {
                    case LogicOperator.AND:
                    case LogicOperator.NAND:
                        res &= Elements[i].Eval(tx, rx);
                        break;
                    case LogicOperator.OR:
                    case LogicOperator.NOR:
                        res |= Elements[i].Eval(tx, rx);
                        break;
                    case LogicOperator.XOR:
                        res ^= Elements[i].Eval(tx, rx);
                        break;
                    case LogicOperator.NOT:
                        break;
                }
            }
            if (Operator == LogicOperator.NOR || Operator == LogicOperator.NAND || Operator == LogicOperator.NOT)
                res = !res;

            Value = res;
            return res;
        } 
    }

    public class InterlockLogicModel
    {
        public List<LogicTargetModel> Targets { get; private set; }
        public LogicExpressionModel Statement { get; private set; }

        public InterlockLogic Logic { get; private set; }

        public bool IsHardware { get { return (Logic.Attr & (uint)InterlockAttribute.Hardware) != 0; } }
        public bool IsExclusive { get { return (Logic.Attr & (uint)InterlockAttribute.Exclusive) != 0; } }

        public string Title { get { return Logic.Attr == 0 ? Logic.Name : AttributeValue(Logic.Attr) + " " + Logic.Name; } }

        public static string? AttributeValue(uint value)
        {
            if (value == 0)
                return null;
            StringBuilder sb = new StringBuilder("[");
            if ((value & (uint)InterlockAttribute.Hardware) != 0)
            {
                sb.Append("Hardware");
            }
            if ((value & (uint)InterlockAttribute.Exclusive) != 0)
            {
                if(sb.Length > 1)
                    sb.Append(", Exclusive");
                else
                    sb.Append("Exclusive");
            }
            return sb.Append("]").ToString();
        }

        public InterlockLogicModel(InterlockLogic logic)
        {
            Logic = logic;

            Targets = new List<LogicTargetModel>(logic.Targets.Select(t => new LogicTargetModel(t, null)));
            Statement = __traverse(logic.Statement, null) as LogicExpressionModel;
        }

        private LogicElementModel __traverse(LogicElement logic, LogicExpressionModel? root)
        {
            if (logic.Type == LogicElementType.OPERAND)
                return new LogicOperandModel((logic as LogicOperand).Operand, null);

            var express = new LogicExpressionModel((logic as LogicExpression).Operator, root);
            foreach (var sub in (logic as LogicExpression).Elements)
                express.Elements.Add(__traverse(sub, root));
            return express;
        }

        public void Rebuild()
        {
            for (int i = 0; i < Targets.Count; ++i)
            {
                Targets[i].Value = false;
                Targets[i].Warning = false;
                Targets[i].BitPos = Logic.Targets[i].BitPos;
            }

            __traverse_rebuild(Statement, Logic.Statement);
        }

        private void __traverse_rebuild(LogicElementModel exp, LogicElement logic)
        {
            if (exp.Type == LogicElementType.OPERAND)
            {
                (exp as LogicOperandModel).Value = false;
                (exp as LogicOperandModel).BitPos = (logic as LogicOperand).Operand.BitPos;
            }
            else
            {
                var model = exp as LogicExpressionModel;
                var data = logic as LogicExpression;
                for (int i = 0; i < model.Elements.Count; ++i)
                    __traverse_rebuild(model.Elements[i], data.Elements[i]);
            }
        }

        public void Eval(ushort[] tx, ushort[] rx, uint attr)
        {
            bool res = Statement.Eval(tx, rx);
            foreach (var t in Targets)
            {
                t.Warning = (t.Eval(tx, rx) == true && res == false && (attr & Logic.Attr) == 0);
                t.Ignored = (attr & Logic.Attr) != 0;
            }
        }

        public override string ToString()
        {
            return Title;
        }
    }

    [System.Flags]
    public enum InterlockAttribute : uint
    {
        None = 0x00000000,
        Hardware = 0x00000001,
        Exclusive = 0x00000002
    }
}
