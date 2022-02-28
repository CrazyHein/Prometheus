using Spire.Xls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia
{
    public class InterlockCollection : IEquatable<InterlockCollection>
    {
        private ProcessDataImage __tx_process_data_image;
        private ProcessDataImage __rx_process_data_image;
        private ObjectDictionary __object_dictionary;
        private List<InterlockLogic> __logics;
        public IReadOnlyList<InterlockLogic> Logics;
        public InterlockCollection(ObjectDictionary od, ProcessDataImage tx, ProcessDataImage rx)
        {
            __tx_process_data_image = tx;
            __rx_process_data_image = rx;
            __object_dictionary = od;
            __logics = new List<InterlockLogic>();
            Logics = __logics;
            InterlockLogic.Publisher = this;
        }

        public InterlockCollection(ObjectDictionary od, ProcessDataImage tx, ProcessDataImage rx, XmlNode intlksNode) : this(od, tx, rx)
        {
            try
            {
                if (intlksNode?.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode interlock in intlksNode.ChildNodes)
                    {
                        if (interlock.NodeType != XmlNodeType.Element || interlock.Name != "Interlock")
                            continue;

                        List<ProcessData> subs = new List<ProcessData>();
                        string name = interlock.SelectSingleNode("Name").FirstChild.Value;
                        var targets = __load_interlock_logic_target(interlock.SelectSingleNode("Target"), subs);
                        var statement = __load_interlock_logic_statement(interlock.SelectSingleNode("Statement").FirstChild, null, subs) as LogicExpression;

                        if (statement == null)
                            throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_INTERLOCK_LOGIC_EXPRESSION_FORMAT);

                        InterlockLogic loc = new InterlockLogic(name, targets, statement, od, tx, rx, subs);
                        __logics.Add(loc);
                    }
                }
            }
            catch (LombardiaException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new LombardiaException(e);
            }
        }

        public InterlockLogic Add(string name, string target, string statement)
        {
            InterlockLogic logic = new InterlockLogic(name, target, statement, __object_dictionary, __tx_process_data_image, __rx_process_data_image);
            logic.AddSubscription();
            __logics.Add(logic);
            return logic;
        }

        public InterlockLogic Insert(int index,string name, string target, string statement)
        {
            InterlockLogic logic = new InterlockLogic(name, target, statement, __object_dictionary, __tx_process_data_image, __rx_process_data_image);
            logic.AddSubscription();
            __logics.Insert(index, logic);
            return logic;
        }

        public InterlockLogic Replace(int index, string name, string target, string statement, ReplaceMode mode = ReplaceMode.Full)
        {
            InterlockLogic logic = new InterlockLogic(name, target, statement, __object_dictionary, __tx_process_data_image, __rx_process_data_image);
            var origin = __logics[index];
            __logics[index] = logic;

            if (mode == ReplaceMode.Full)
            {
                origin.RemoveSubscription();
                logic.AddSubscription();
            }
            return logic;
        }

        public InterlockLogic Remove(int index)
        {
            var i = __logics[index];
            i.RemoveSubscription();
            __logics.RemoveAt(index);
            return i;
        }

        public int Indexof(InterlockLogic logic)
        {
            return __logics.IndexOf(logic);
        }

        public void Save(XmlDocument doc, XmlElement intlkCollectionNode, uint version = 1)
        {
            try
            {
                foreach (var i in __logics)
                {
                    XmlElement interlock = doc.CreateElement("Interlock");

                    XmlElement e = doc.CreateElement("Name");
                    e.AppendChild(doc.CreateTextNode(i.Name));
                    interlock.AppendChild(e);

                    e = doc.CreateElement("Target");
                    foreach (var o in i.Targets)
                    {
                        XmlElement index = doc.CreateElement("Index");
                        index.AppendChild(doc.CreateTextNode($"0x{o.ProcessObject.Index:X8}"));
                        e.AppendChild(index);
                    }
                    interlock.AppendChild(e);

                    e = doc.CreateElement("Statement");
                    e.AppendChild(__save_interlock_statement(doc, i.Statement, version));
                    interlock.AppendChild(e);

                    intlkCollectionNode.AppendChild(interlock);
                }
            }
            catch(Exception ex)
            {
                throw new LombardiaException(ex);
            }
        }

        public void Save(Worksheet sheet, CellStyle title, CellStyle content, uint version = 1)
        {
            try
            {
                int counter = 0;
                foreach (var i in __logics)
                {
                    sheet.Range[1 + counter * 4, 1].Text = "Name";
                    sheet.Range[2 + counter * 4, 1].Text = "Target";
                    sheet.Range[3 + counter * 4, 1].Text = "Statement";

                    sheet.Range[1 + counter * 4, 2].Text = i.Name;
                    StringBuilder target = new StringBuilder();
                    foreach (var t in i.Targets)
                    {
                        target.Append(t.ToString());
                        target.Append("\r\n");
                    }
                    sheet.Range[2 + counter * 4, 2].Text = target.ToString();
                    sheet.Range[3 + counter * 4, 2].Text = i.Statement.ToString();

                    counter++;
                }

                if (counter > 0)
                {
                    sheet.Range[1, 1, counter * 4, 1].Style = title;
                    sheet.Range[1, 2, counter * 4, 2].Style = content;
                }
                sheet.AllocatedRange.AutoFitColumns();
                sheet.AllocatedRange.AutoFitRows();
            }
            catch (Exception ex)
            {
                throw new LombardiaException(ex);
            }
        }

        private XmlElement __save_interlock_statement(XmlDocument doc, LogicElement logic, uint version = 1)
        {
            XmlElement e;
            if (logic.Type == LogicElementType.OPERAND)
            {
                e = doc.CreateElement("Index");
                e.AppendChild(doc.CreateTextNode($"0x{(logic as LogicOperand).Operand.ProcessObject.Index:X8}"));
            }
            else
            {
                e = doc.CreateElement((logic as LogicExpression).Operator.ToString());
                foreach (var el in (logic as LogicExpression).Elements)
                    e.AppendChild(__save_interlock_statement(doc, el, version));
            }
            return e;
        }

        public static ProcessData LOGIC_TARGET_PROCESS_DATA(uint id, ObjectDictionary od, ProcessDataImage rx)
        {
            if (od.ProcessObjects.TryGetValue(id, out var ob) == true && rx.ProcessObjectHash.TryGetValue(ob, out var d) == true)
                return d;
            else
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_PROCESS_DATA_REFERENCE_IN_INTERLOCK);
        }

        public static (ProcessData, ProcessDataImage) LOGIC_STATEMENT_PROCESS_DATA(uint id, ObjectDictionary od, ProcessDataImage tx, ProcessDataImage rx)
        {
            if (od.ProcessObjects.TryGetValue(id, out var ob) == true)
            {
                if (tx.ProcessObjectHash.TryGetValue(ob, out var d) == true)
                    return (d, tx);
                else if (rx.ProcessObjectHash.TryGetValue(ob, out d) == true)
                {
                    return (d, rx);
                }
            }
            throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_PROCESS_DATA_REFERENCE_IN_INTERLOCK);
        }

        private List<ProcessData> __load_interlock_logic_target(XmlNode targetsNode, List<ProcessData> subs)
        {
            try
            {
                var targets = new List<ProcessData>();
                if (targetsNode?.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode target in targetsNode.ChildNodes)
                    {
                        if (target.Name == "Index" && target.NodeType == XmlNodeType.Element)
                        {
                            uint id = Convert.ToUInt32(target.FirstChild.Value, 16);
                            var d = InterlockCollection.LOGIC_TARGET_PROCESS_DATA(id, __object_dictionary, __rx_process_data_image);
                            targets.Add(d);
                            subs.Add(d);
                        }
                    }
                }
                return targets;
            }
            catch (LombardiaException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new LombardiaException(e);
            }
        }

        private LogicElement __load_interlock_logic_statement(XmlNode rootNode, LogicExpression? rootExpression, List<ProcessData> subs)
        {
            try
            {
                if (rootNode?.NodeType == XmlNodeType.Element)
                {
                    LogicExpression expression;
                    if (rootExpression != null)
                    {
                        if (rootExpression.Layer == (int)CONSTANTS.MAX_LAYER_OF_NESTED_LOGIC)
                            throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INTERLOCK_LOGIC_STATEMENT_LAYER_OUT_OF_RANGE);
                    }
                    switch (rootNode.Name)
                    {
                        case "Index":
                            uint id = Convert.ToUInt32(rootNode.FirstChild.Value, 16);
                            var (d, _) = InterlockCollection.LOGIC_STATEMENT_PROCESS_DATA(id, __object_dictionary, __tx_process_data_image, __rx_process_data_image);
                            subs.Add(d);
                            return new LogicOperand(d, rootExpression);
                        case "NOT":
                            expression = new LogicExpression(LogicOperator.NOT, rootExpression);
                            foreach (XmlNode node in rootNode.ChildNodes)
                            {
                                LogicElement element = __load_interlock_logic_statement(node, expression, subs);
                                if (expression.Elements.Count == 0)
                                    expression.Elements.Add(element);
                                else
                                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_INTERLOCK_LOGIC_NOT_EXPRESSION);
                            }
                            return expression;
                        case "AND":
                            expression = new LogicExpression(LogicOperator.AND, rootExpression);
                            foreach (XmlNode node in rootNode.ChildNodes)
                                expression.Elements.Add(__load_interlock_logic_statement(node, expression, subs));
                            return expression;
                        case "OR":
                            expression = new LogicExpression(LogicOperator.OR, rootExpression);
                            foreach (XmlNode node in rootNode.ChildNodes)
                                expression.Elements.Add(__load_interlock_logic_statement(node, expression, subs));
                            return expression;
                        case "NAND":
                            expression = new LogicExpression(LogicOperator.NAND, rootExpression);
                            foreach (XmlNode node in rootNode.ChildNodes)
                                expression.Elements.Add(__load_interlock_logic_statement(node, expression, subs));
                            return expression;
                        case "NOR":
                            expression = new LogicExpression(LogicOperator.NOR, rootExpression);
                            foreach (XmlNode node in rootNode.ChildNodes)
                                expression.Elements.Add(__load_interlock_logic_statement(node, expression, subs));
                            return expression;
                        case "XOR":
                            expression = new LogicExpression(LogicOperator.XOR, rootExpression);
                            foreach (XmlNode node in rootNode.ChildNodes)
                                expression.Elements.Add(__load_interlock_logic_statement(node, expression, subs));
                            return expression;
                        default:
                            throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_INTERLOCK_LOGIC_NOT_EXPRESSION);
                    }
                }
                else
                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_INTERLOCK_LOGIC_EXPRESSION_FORMAT);
            }
            catch (LombardiaException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new LombardiaException(e);
            }
        }

        public bool Equals(InterlockCollection? other)
        {
            return other != null && other.Logics.Count == Logics.Count && other.Logics.Select((l, i) => l.Equals(Logics[i])).All(r => r == true);
        }
    }

    public abstract class LogicElement : IEquatable<LogicElement>
    {
        public LogicElementType Type { get; private set; }
        public LogicExpression? Root { get; private set; }
        public int Layer { get; private set; }

        public abstract string Serialize(ProcessData? origin = null, ProcessData? replace = null);

        public bool Equals(LogicElement? other)
        {
            if(other == null || other.Type != Type)
                return false;
            else if (Type == LogicElementType.OPERAND)
                return (other as LogicOperand).Operand.Equals((this as LogicOperand).Operand);
            else
            {
                LogicExpression ori  = this as LogicExpression;
                LogicExpression oth = other as LogicExpression;
                if(ori.Layer == oth.Layer && ori.Operator == oth.Operator && ori.Elements.Count == oth.Elements.Count)
                    return oth.Elements.Select((t, i) => ori.Elements[i].Equals(t)).All(r => r == true);
                else
                    return false;
            }
        }

        public LogicElement(LogicElementType type, LogicExpression root)
        {
            Type = type;
            Root = root;
            if (root == null)
                Layer = 0;
            else
                Layer = root.Layer + 1;
        }
    }

    public class LogicOperand : LogicElement
    {
        public ProcessData Operand { get; private set; }
        public LogicOperand(ProcessData data, LogicExpression root) : base(LogicElementType.OPERAND, root)
        {
            Operand = data;
        }

        public override string Serialize(ProcessData? origin = null, ProcessData? replace = null)
        {
            StringBuilder tabs = new StringBuilder();
            for (int i = 0; i < Layer; i++)
                tabs.Append("\t");
            if(origin != null && replace != null && Operand == origin)
                return tabs.Append("0x").Append(replace.ProcessObject.Index.ToString("X08")).Append("\r\n").ToString();
            else
                return tabs.Append("0x").Append(Operand.ProcessObject.Index.ToString("X08")).Append("\r\n").ToString();
        }

        public override string ToString()
        {
            StringBuilder tabs = new StringBuilder();
            for (int i = 0; i < Layer; i++)
                tabs.Append("    ");
            return tabs.Append("0x").Append(Operand.ToString()).Append("\r\n").ToString();
        }
    }

    public class LogicExpression : LogicElement
    {
        public List<LogicElement> Elements { get; private set; }
        public LogicOperator Operator { get; private set; }
        public LogicExpression(LogicOperator op, LogicExpression root) : base(LogicElementType.EXPRESSION, root)
        {
            Operator = op;
            Elements = new List<LogicElement>();
        }

        public override string Serialize(ProcessData? origin = null, ProcessData? replace = null)
        {
            StringBuilder tabs = new StringBuilder();
            for (int i = 0; i < Layer; i++)
                tabs.Append("\t");
            StringBuilder str = new StringBuilder();
            str.Append(tabs).Append(Operator).Append("\r\n");
            foreach (var e in Elements)
                str.Append(e.Serialize(origin, replace));
            return str.ToString();
        }

        public override string ToString()
        {
            StringBuilder tabs = new StringBuilder();
            for (int i = 0; i < Layer; i++)
                tabs.Append("    ");
            StringBuilder str = new StringBuilder();
            str.Append(tabs).Append(Operator).Append("\r\n");
            foreach (var e in Elements)
                str.Append(e.ToString());
            return str.ToString();
        }
    }

    public class InterlockLogic : ISubscriber<ProcessData>, IEquatable<InterlockLogic>
    {
        public string Name { get; private set; }
        public List<ProcessData> Targets { get; private set; }
        public LogicExpression Statement { get; private set; }

        private List<ProcessData> __subscriptions = new List<ProcessData> ();
        public IReadOnlyList<ProcessData> Subscriptions { get; private set; }
        private ProcessDataImage __tx_process_data_image;
        private ProcessDataImage __rx_process_data_image;

        public InterlockLogic(string name, List<ProcessData> targets, LogicExpression statement, ObjectDictionary od, ProcessDataImage tx, ProcessDataImage rx, IReadOnlyList<ProcessData> subs)
        {
            Name = name;
            Targets = targets; 
            Statement = statement;

            if (subs != null)
            {
                foreach (var d in subs)
                {
                    __subscriptions.Add(d);
                    switch (d.Access)
                    {
                        case ProcessDataImageAccess.TX:
                            tx.AddSubscriber(d, this);
                            break;
                        case ProcessDataImageAccess.RX:
                            rx.AddSubscriber(d, this);
                            break;
                    }
                }
            }
            __tx_process_data_image = tx;
            __rx_process_data_image = rx;
            Subscriptions = __subscriptions;
        }

        public InterlockLogic(string name, string target, string statement,
            ObjectDictionary od, ProcessDataImage tx, ProcessDataImage rx)
        {
            Name = name;
            try
            {
                Targets = new List<ProcessData>();
                System.IO.StringReader sr = new System.IO.StringReader(target);
                while (true)
                {
                    string line = sr.ReadLine();
                    if (line == null)
                        break;
                    else
                        line = line.Trim();
                    if (line == string.Empty)
                        continue;
                    uint id = Convert.ToUInt32(line, 16);
                    var d = InterlockCollection.LOGIC_TARGET_PROCESS_DATA(id, od, rx);
                    //rx.AddSubscriber(d, this);
                    __subscriptions.Add(d);
                    Targets.Add(d);
                }

                sr = new System.IO.StringReader(statement);
                List<(string op, int indent)> statements = new List<(string op, int indent)>();
                while (true)
                {
                    string line = sr.ReadLine();
                    if (line == null)
                        break;
                    else if (line.Trim() == string.Empty)
                        continue;
                    else
                    {
                        int i = 0;
                        for (; i < line.Count(); ++i)
                            if (line[i] != '\t')
                                break;
                        statements.Add(new ValueTuple<string, int>(line.Substring(i).TrimEnd(), i));
                    }
                }
                int start = 0;
                Statement = __search_logic_element(statements, ref start, null, od, tx, rx) as LogicExpression;

                if (Statement == null)
                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_INTERLOCK_LOGIC_EXPRESSION_FORMAT);

                __tx_process_data_image = tx;
                __rx_process_data_image = rx;
                Subscriptions = __subscriptions;
            }
            catch (LombardiaException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new LombardiaException(e);
            }
        }

        public void RemoveSubscription()
        {
            foreach (var d in __subscriptions)
            {
                switch (d.Access)
                {
                    case ProcessDataImageAccess.TX:
                        __tx_process_data_image.RemoveSubscriber(d, this);
                        break;
                    case ProcessDataImageAccess.RX:
                        __rx_process_data_image.RemoveSubscriber(d, this);
                        break;
                }
            }
        }

        public void AddSubscription()
        {
            foreach (var d in __subscriptions)
            {
                switch (d.Access)
                {
                    case ProcessDataImageAccess.TX:
                        __tx_process_data_image.AddSubscriber(d, this);
                        break;
                    case ProcessDataImageAccess.RX:
                        __rx_process_data_image.AddSubscriber(d, this);
                        break;
                }
            }
        }

        private LogicElement __search_logic_element(List<(string op, int indent)> statements, ref int start, LogicExpression? root,
            ObjectDictionary od, ProcessDataImage tx, ProcessDataImage rx)
        {
            if (root != null && root.Layer == (int)CONSTANTS.MAX_LAYER_OF_NESTED_LOGIC)
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INTERLOCK_LOGIC_STATEMENT_LAYER_OUT_OF_RANGE);

            LogicExpression expression = null;
            var lineInfo = statements[start];
            switch(lineInfo.op)
            {
                case "NOT":
                    expression = new LogicExpression(LogicOperator.NOT, root);
                    /*
                    while (start + 1 < statements.Count)
                    {
                        if (statements[start + 1].indent == lineInfo.indent + 1)
                        {
                            start++;
                            LogicElement element = __search_logic_element(statements, ref start, expression, od, tx, rx);
                            if (expression.Elements.Count == 0)
                                expression.Elements.Add(element);
                            else
                                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_INTERLOCK_LOGIC_NOT_EXPRESSION);
                        }
                        else if (statements[start + 1].indent > lineInfo.indent + 1 || statements[start + 1].indent == 0)
                            throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_INTERLOCK_LOGIC_EXPRESSION_FORMAT);
                        else
                            break;
                    }
                    return expression;
                    */
                    break;
                case "AND":
                    expression = new LogicExpression(LogicOperator.AND, root);
                    break;
                case "OR":
                    expression = new LogicExpression(LogicOperator.OR, root);
                    break;
                case "NAND":
                    expression = new LogicExpression(LogicOperator.NAND, root);
                    break;
                case "NOR":
                    expression = new LogicExpression(LogicOperator.NOR, root);
                    break;
                case "XOR":
                    expression = new LogicExpression(LogicOperator.XOR, root);
                    break;
                default:
                    uint id = Convert.ToUInt32(lineInfo.op, 16);
                    if (root == null)
                        throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_INTERLOCK_LOGIC_EXPRESSION_FORMAT);
                    var (d, r) = InterlockCollection.LOGIC_STATEMENT_PROCESS_DATA(id, od, tx, rx);
                    //r.AddSubscriber(d, this);
                    __subscriptions.Add(d);
                    return new LogicOperand(d, root);
            }

            while (start + 1 < statements.Count)
            {
                if (statements[start + 1].indent == lineInfo.indent + 1)
                {
                    start++;
                    expression.Elements.Add(__search_logic_element(statements, ref start, expression, od, tx, rx));
                }
                else if (statements[start + 1].indent > lineInfo.indent + 1 || statements[start + 1].indent == 0)
                    throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_INTERLOCK_LOGIC_EXPRESSION_FORMAT);
                else
                    break;
            }
            if(expression.Elements.Count == 0 || (expression.Operator == LogicOperator.NOT && expression.Elements.Count > 1))
                throw new LombardiaException(LOMBARDIA_ERROR_CODE_T.INVALID_INTERLOCK_LOGIC_EXPRESSION_FORMAT);
            return expression;
        }

        public ISubscriber<ProcessData>? DependencyChanged(ProcessData origin, ProcessData newcome)
        {
            Debug.Assert(__subscriptions.Contains(origin));
            if (Publisher != null)
            {
                int pos = Publisher.Indexof(this);
                //if (pos == -1)
                    //;
                StringBuilder targets = new StringBuilder();
                foreach (var t in Targets)
                {
                    if(t == origin)
                        targets.Append("0x").Append(newcome.ProcessObject.Index.ToString("X08"));
                    else
                        targets.Append("0x").Append(t.ProcessObject.Index.ToString("X08"));
                    targets.Append("\r\n");
                }
                return Publisher.Replace(pos, this.Name, targets.ToString().TrimEnd(), Statement.Serialize(origin, newcome), ReplaceMode.Half);
            }
            else
                return null;
        }

        public bool Equals(InterlockLogic? other)
        {
            if(other == null || other.Name != Name || other.Targets.Count != Targets.Count || other.Statement.Equals(Statement) == false)
                return false;
            else
                return other.Targets.Select((t, i) => Targets[i].Equals(t)).All(r => r == true);
        }

        public static InterlockCollection? Publisher { get; set; }
    }
}
