using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.Catalogue;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.IOListDataControl
{
    public class PDOCollectionDataModel : IOListDataModel
    {
        public IReadOnlyList<ObjectItemDataModel> TxDiagnosticArea { get { return __tx_diagnostic_area; } }
        public IReadOnlyList<ObjectItemDataModel> TxBitArea { get { return __tx_bit_area; } }
        public IReadOnlyList<ObjectItemDataModel> TxBlockArea { get { return __tx_block_area; } }
        public IReadOnlyList<ObjectItemDataModel> RxControlArea { get { return __rx_control_area; } }
        public IReadOnlyList<ObjectItemDataModel> RxBitArea { get { return __rx_bit_area; } }
        public IReadOnlyList<ObjectItemDataModel> RxBlockArea { get { return __rx_block_area; } }
        public IReadOnlyList<ObjectItemDataModel> AvailableObjects { get { return __object_collection_data_model.Objects; }}
        public ObjectItemFilter AvailableObjectItemFilter { get; private set; }
        public string FilterDataTypeName { get; set; }
        public string FilterVariableName { get; set; }
        public string FilterModuleName { get; set; }

        private ObservableCollection<ObjectItemDataModel> __tx_diagnostic_area;
        private ObservableCollection<ObjectItemDataModel> __tx_bit_area;
        private ObservableCollection<ObjectItemDataModel> __tx_block_area;
        private ObservableCollection<ObjectItemDataModel> __rx_control_area;
        private ObservableCollection<ObjectItemDataModel> __rx_bit_area;
        private ObservableCollection<ObjectItemDataModel> __rx_block_area;
        private uint __tx_diag_offset_in_word, __tx_diag_size_in_word, __tx_diag_actual_size_in_byte;
        private uint __tx_bit_offset_in_word, __tx_bit_size_in_word, __tx_bit_actual_size_in_byte;
        private uint __tx_block_offset_in_word, __tx_block_size_in_word, __tx_block_actual_size_in_byte;
        private uint __rx_control_offset_in_word, __rx_control_size_in_word, __rx_control_actual_size_in_byte;
        private uint __rx_bit_offset_in_word, __rx_bit_size_in_word, __rx_bit_actual_size_in_byte;
        private uint __rx_block_offset_in_word, __rx_block_size_in_word, __rx_block_actual_size_in_byte;

        private Dictionary<IO_LIST_PDO_AREA_T, ObservableCollection<ObjectItemDataModel>> __collection_areas;

        private ObservableCollection<IntlklogicDefinition> __intlk_logic_area;
        public IReadOnlyList<IntlklogicDefinition> IntlkLogicArea { get { return __intlk_logic_area; } }



        private ObjectCollectionDataModel __object_collection_data_model;

        public PDOCollectionDataModel(IOListDataHelper helper, ObjectCollectionDataModel objectCollectionDataModel) : base(helper)
        {
            __tx_diagnostic_area = new ObservableCollection<ObjectItemDataModel>();
            __tx_bit_area = new ObservableCollection<ObjectItemDataModel>();
            __tx_block_area = new ObservableCollection<ObjectItemDataModel>();
            __rx_control_area = new ObservableCollection<ObjectItemDataModel>();
            __rx_bit_area = new ObservableCollection<ObjectItemDataModel>();
            __rx_block_area = new ObservableCollection<ObjectItemDataModel>();

            __collection_areas = new Dictionary<IO_LIST_PDO_AREA_T, ObservableCollection<ObjectItemDataModel>>(6);
            __collection_areas.Add(IO_LIST_PDO_AREA_T.TX_DIAGNOSTIC, __tx_diagnostic_area);
            __collection_areas.Add(IO_LIST_PDO_AREA_T.TX_BIT, __tx_bit_area);
            __collection_areas.Add(IO_LIST_PDO_AREA_T.TX_BLOCK, __tx_block_area);
            __collection_areas.Add(IO_LIST_PDO_AREA_T.RX_CONTROL, __rx_control_area);
            __collection_areas.Add(IO_LIST_PDO_AREA_T.RX_BIT, __rx_bit_area);
            __collection_areas.Add(IO_LIST_PDO_AREA_T.RX_BLOCK, __rx_block_area);

            __intlk_logic_area = new ObservableCollection<IntlklogicDefinition>();

            __object_collection_data_model = objectCollectionDataModel;

            FilterVariableName = "";
            FilterDataTypeName = "";
            FilterModuleName = "";
            AvailableObjectItemFilter = new ObjectItemFilter(null, FilterModuleName, FilterVariableName);
        }

        public uint TxDiagnosticAreaOffsetInWord
        {
            get { return __tx_diag_offset_in_word; }
            set { SetProperty(ref __tx_diag_offset_in_word, value); }
        }
        public uint TxDiagnosticAreaSizeInWord
        {
            get { return __tx_diag_size_in_word; }
            set { SetProperty(ref __tx_diag_size_in_word, value); }
        }
        public uint TxDiagnosticAreaActualSizeInByte
        {
            get { return __tx_diag_actual_size_in_byte; }
            private set { SetProperty(ref __tx_diag_actual_size_in_byte, value); }
        }

        public uint TxBitAreaOffsetInWord
        {
            get { return __tx_bit_offset_in_word; }
            set { SetProperty(ref __tx_bit_offset_in_word, value); }
        }
        public uint TxBitAreaSizeInWord
        {
            get { return __tx_bit_size_in_word; }
            set { SetProperty(ref __tx_bit_size_in_word, value); }
        }
        public uint TxBitAreaActualSizeInByte
        {
            get { return __tx_bit_actual_size_in_byte; }
            private set { SetProperty(ref __tx_bit_actual_size_in_byte, value); }
        }

        public uint TxBlockAreaOffsetInWord
        {
            get { return __tx_block_offset_in_word; }
            set { SetProperty(ref __tx_block_offset_in_word, value); }
        }
        public uint TxBlockAreaSizeInWord
        {
            get { return __tx_block_size_in_word; }
            set { SetProperty(ref __tx_block_size_in_word, value); }
        }
        public uint TxBlockAreaActualSizeInByte
        {
            get { return __tx_block_actual_size_in_byte; }
            private set { SetProperty(ref __tx_block_actual_size_in_byte, value); }
        }

        public uint RxControlAreaOffsetInWord
        {
            get { return __rx_control_offset_in_word; }
            set { SetProperty(ref __rx_control_offset_in_word, value); }
        }
        public uint RxControlAreaSizeInWord
        {
            get { return __rx_control_size_in_word; }
            set { SetProperty(ref __rx_control_size_in_word, value); }
        }
        public uint RxControlAreaActualSizeInByte
        {
            get { return __rx_control_actual_size_in_byte; }
            private set { SetProperty(ref __rx_control_actual_size_in_byte, value); }
        }

        public uint RxBitAreaOffsetInWord
        {
            get { return __rx_bit_offset_in_word; }
            set { SetProperty(ref __rx_bit_offset_in_word, value); }
        }
        public uint RxBitAreaSizeInWord
        {
            get { return __rx_bit_size_in_word; }
            set { SetProperty(ref __rx_bit_size_in_word, value); }
        }
        public uint RxBitAreaActualSizeInByte
        {
            get { return __rx_bit_actual_size_in_byte; }
            private set { SetProperty(ref __rx_bit_actual_size_in_byte, value); }
        }

        public uint RxBlockAreaOffsetInWord
        {
            get { return __rx_block_offset_in_word; }
            set { SetProperty(ref __rx_block_offset_in_word, value); }
        }
        public uint RxBlockAreaSizeInWord
        {
            get { return __rx_block_size_in_word; }
            set { SetProperty(ref __rx_block_size_in_word, value); }
        }
        public uint RxBlockAreaActualSizeInByte
        {
            get { return __rx_block_actual_size_in_byte; }
            private set { SetProperty(ref __rx_block_actual_size_in_byte, value); }
        }

        public void __updata_area_actual_size(IO_LIST_PDO_AREA_T area)
        {
            switch (area)
            {
                case IO_LIST_PDO_AREA_T.TX_DIAGNOSTIC:
                    TxDiagnosticAreaActualSizeInByte = _data_helper.TxDiagnosticAreaActualSize;
                    break;
                case IO_LIST_PDO_AREA_T.TX_BIT:
                    TxBitAreaActualSizeInByte = _data_helper.TxBitAreaActualSize;
                    break;
                case IO_LIST_PDO_AREA_T.TX_BLOCK:
                    TxBlockAreaActualSizeInByte = _data_helper.TxBlockAreaActualSize;
                    break;
                case IO_LIST_PDO_AREA_T.RX_CONTROL:
                    RxControlAreaActualSizeInByte = _data_helper.RxControlAreaActualSize;
                    break;
                case IO_LIST_PDO_AREA_T.RX_BIT:
                    RxBitAreaActualSizeInByte = _data_helper.RxBitAreaActualSize;
                    break;
                case IO_LIST_PDO_AREA_T.RX_BLOCK:
                    RxBlockAreaActualSizeInByte = _data_helper.RxBlockAreaActualSize;
                    break;
            }
        }

        private void __updata_area_size(IO_LIST_PDO_AREA_T area)
        {
            switch (area)
            {
                case IO_LIST_PDO_AREA_T.TX_DIAGNOSTIC:            
                    _data_helper.TxDiagnosticAreaSize = TxDiagnosticAreaSizeInWord;
                    _data_helper.TxDiagnosticAreaOffset = TxDiagnosticAreaOffsetInWord;
                    break;
                case IO_LIST_PDO_AREA_T.TX_BIT:
                    _data_helper.TxBitAreaSize = TxBitAreaSizeInWord;
                    _data_helper.TxBitAreaOffset = TxBitAreaOffsetInWord;
                    break;
                case IO_LIST_PDO_AREA_T.TX_BLOCK:
                    _data_helper.TxBlockAreaSize = TxBlockAreaSizeInWord;
                    _data_helper.TxBlockAreaOffset = TxBlockAreaOffsetInWord;
                    break;
                case IO_LIST_PDO_AREA_T.RX_CONTROL:
                    _data_helper.RxControlAreaSize = RxControlAreaSizeInWord;
                    _data_helper.RxControlAreaOffset = RxControlAreaOffsetInWord;
                    break;
                case IO_LIST_PDO_AREA_T.RX_BIT:
                    _data_helper.RxBitAreaSize = RxBitAreaSizeInWord;
                    _data_helper.RxBitAreaOffset = RxBitAreaOffsetInWord;
                    break;
                case IO_LIST_PDO_AREA_T.RX_BLOCK:
                    _data_helper.RxBlockAreaSize = RxBlockAreaSizeInWord;
                    _data_helper.RxBlockAreaOffset = RxBlockAreaOffsetInWord;
                    break;
            }
        }

        private IntlkLogicElement __load_interlock_logic_statement(IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_ELEMEMT_T element,
            IntlkLogicExpression root)
        {
            if (element.type == IO_LIST_INTERLOCK_LOGIC_ELEMENT_TYPE.OPERAND)
            {
                IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_OPERAND_T operand = element as IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_OPERAND_T;
                return new IntlkLogicOperand(__object_collection_data_model.ObjectDictionary[operand.Operand.index], root);
            }
            else
            {
                IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_EXPRESSION_T expression = element as IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_EXPRESSION_T;
                IntlkLogicExpression expressionDataModel = new IntlkLogicExpression(expression.logic_operator, root);
                foreach (var e in expression.elements)
                    expressionDataModel.Elements.Add(__load_interlock_logic_statement(e, expressionDataModel));
                return expressionDataModel;
            }
        }

        private IntlklogicDefinition __load_interlock_logic_definition(IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_DEFINITION_T definition)
        {
            List<ObjectItemDataModel> objectDataModels = new List<ObjectItemDataModel>();
            foreach (var o in definition.target_objects)
                objectDataModels.Add(__object_collection_data_model.ObjectDictionary[o.index]);

            IntlkLogicExpression expressionDataModel =
                __load_interlock_logic_statement(definition.statement, null) as IntlkLogicExpression;

            return new IntlklogicDefinition(definition.name, objectDataModels, expressionDataModel);
        }

        public override void UpdateDataHelper()
        {
            foreach (var area in Enum.GetValues(typeof(IO_LIST_PDO_AREA_T)))
                __updata_area_size((IO_LIST_PDO_AREA_T)area);
        }

        public override void UpdateDataModel()
        {
            TxDiagnosticAreaOffsetInWord = _data_helper.TxDiagnosticAreaOffset;
            TxDiagnosticAreaSizeInWord = _data_helper.TxDiagnosticAreaSize;
            TxDiagnosticAreaActualSizeInByte = _data_helper.TxDiagnosticAreaActualSize;
            __tx_diagnostic_area.Clear();
            foreach (var o in _data_helper.TxDiagnosticArea)
            {
                __tx_diagnostic_area.Add(__object_collection_data_model.ObjectDictionary[o.index]);
            }

            TxBitAreaOffsetInWord = _data_helper.TxBitAreaOffset;
            TxBitAreaSizeInWord = _data_helper.TxBitAreaSize;
            TxBitAreaActualSizeInByte = _data_helper.TxBitAreaActualSize;
            __tx_bit_area.Clear();
            foreach (var o in _data_helper.TxBitArea)
            {
                //ObjectItemDataModel model = new ObjectItemDataModel(o);
                __tx_bit_area.Add(__object_collection_data_model.ObjectDictionary[o.index]);
            }

            TxBlockAreaOffsetInWord = _data_helper.TxBlockAreaOffset;
            TxBlockAreaSizeInWord = _data_helper.TxBlockAreaSize;
            TxBlockAreaActualSizeInByte = _data_helper.TxBlockAreaActualSize;
            __tx_block_area.Clear();
            foreach (var o in _data_helper.TxBlockArea)
            {
                //ObjectItemDataModel model = new ObjectItemDataModel(o);
                __tx_block_area.Add(__object_collection_data_model.ObjectDictionary[o.index]);
            }

            RxControlAreaOffsetInWord = _data_helper.RxControlAreaOffset;
            RxControlAreaSizeInWord = _data_helper.RxControlAreaSize;
            RxControlAreaActualSizeInByte = _data_helper.RxControlAreaActualSize;
            __rx_control_area.Clear();
            foreach (var o in _data_helper.RxControlArea)
            {
                //ObjectItemDataModel model = new ObjectItemDataModel(o);
                __rx_control_area.Add(__object_collection_data_model.ObjectDictionary[o.index]);
            }

            RxBitAreaOffsetInWord = _data_helper.RxBitAreaOffset;
            RxBitAreaSizeInWord = _data_helper.RxBitAreaSize;
            RxBitAreaActualSizeInByte = _data_helper.RxBitAreaActualSize;
            __rx_bit_area.Clear();
            foreach (var o in _data_helper.RxBitArea)
            {
                //ObjectItemDataModel model = new ObjectItemDataModel(o);
                __rx_bit_area.Add(__object_collection_data_model.ObjectDictionary[o.index]);
            }

            RxBlockAreaOffsetInWord = _data_helper.RxBlockAreaOffset;
            RxBlockAreaSizeInWord = _data_helper.RxBlockAreaSize;
            RxBlockAreaActualSizeInByte = _data_helper.RxBlockAreaActualSize;
            __rx_block_area.Clear();
            foreach (var o in _data_helper.RxBlockArea)
            {
                //ObjectItemDataModel model = new ObjectItemDataModel(o);
                __rx_block_area.Add(__object_collection_data_model.ObjectDictionary[o.index]);
            }

            foreach (var def in _data_helper.InterlockDefinitions)
                __intlk_logic_area.Add(__load_interlock_logic_definition(def));
        }

        public void SwapPDOMapping(IO_LIST_PDO_AREA_T area, int firstPos, int secondPos)
        {
            _data_helper.SwapPDOMapping(area, firstPos, secondPos);
            if (firstPos < secondPos)
            {
                __collection_areas[area].Move(secondPos, firstPos);
                __collection_areas[area].Move(firstPos + 1, secondPos);
            }
            else if (firstPos > secondPos)
            {
                __collection_areas[area].Move(firstPos, secondPos);
                __collection_areas[area].Move(secondPos + 1, firstPos);
            }
        }

        public void InsertPDOMapping(int pos, IO_LIST_PDO_AREA_T area, uint objectIndex)
        {
            __updata_area_size(area);
            _data_helper.InsertPDOMapping(pos, area, objectIndex);
            __updata_area_actual_size(area);
            __collection_areas[area].Insert(pos, __object_collection_data_model.ObjectDictionary[objectIndex]);
        }

        public void InsertPDOMapping(int pos, IO_LIST_PDO_AREA_T area, ObjectItemDataModel objectDataModel)
        {
            __updata_area_size(area);
            IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData = _data_helper.IOObjectDictionary[objectDataModel.Index];
            _data_helper.InsertPDOMapping(pos, area, objectData, false);
            __updata_area_actual_size(area);
            __collection_areas[area].Insert(pos, objectDataModel);
        }

        public void RemovePDOMapping(int pos, IO_LIST_PDO_AREA_T area)
        {
            __updata_area_size(area);
            _data_helper.RemovePDOMapping(pos, area);
            __updata_area_actual_size(area);
            __collection_areas[area].RemoveAt(pos);
        }

        public void AppendPDOMapping(IO_LIST_PDO_AREA_T area, uint objectIndex)
        {
            __updata_area_size(area);
            _data_helper.AppendPDOMapping(area, objectIndex);
            __updata_area_actual_size(area);
            __collection_areas[area].Add(__object_collection_data_model.ObjectDictionary[objectIndex]);
        }

        public void AppendPDOMapping(IO_LIST_PDO_AREA_T area, ObjectItemDataModel objectDataModel)
        {
            __updata_area_size(area);
            IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData = _data_helper.IOObjectDictionary[objectDataModel.Index];
            _data_helper.AppendPDOMapping(area, objectData, false);
            __updata_area_actual_size(area);
            __collection_areas[area].Add(objectDataModel);
        }

        public void ReplacePDOMapping(IO_LIST_PDO_AREA_T area, int pos, uint objectIndex)
        {
            __updata_area_size(area);
            _data_helper.ReplacePDOMapping(area, pos, objectIndex);
            __updata_area_actual_size(area);
            __collection_areas[area].RemoveAt(pos);
            __collection_areas[area].Insert(pos, __object_collection_data_model.ObjectDictionary[objectIndex]);
            //__collection_areas[area][pos] = __object_collection_data_model.ObjectDictionary[objectIndex];
        }

        public void ReplacePDOMapping(IO_LIST_PDO_AREA_T area, int pos, ObjectItemDataModel objectDataModel)
        {
            __updata_area_size(area);
            IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData = _data_helper.IOObjectDictionary[objectDataModel.Index];
            _data_helper.ReplacePDOMapping(area, pos, objectData, false);
            __updata_area_actual_size(area);
            __collection_areas[area].RemoveAt(pos);
            __collection_areas[area].Insert(pos, objectDataModel);
            //__collection_areas[area][pos] = objectDataModel;
        }

        public void GroupPDOMappingByBindingModule(IO_LIST_PDO_AREA_T area)
        {
            _data_helper.GroupPDOMappingByBindingModule(area);
            IReadOnlyList<IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T> objectList = null;

            switch (area)
            {
                case IO_LIST_PDO_AREA_T.TX_DIAGNOSTIC:
                    objectList = _data_helper.TxBitArea;
                    break;
                case IO_LIST_PDO_AREA_T.TX_BIT:
                    objectList = _data_helper.TxBitArea;
                    break;
                case IO_LIST_PDO_AREA_T.TX_BLOCK:
                    objectList = _data_helper.TxBlockArea;
                    break;
                case IO_LIST_PDO_AREA_T.RX_CONTROL:
                    objectList = _data_helper.RxControlArea;
                    break;
                case IO_LIST_PDO_AREA_T.RX_BIT:
                    objectList = _data_helper.RxBitArea;
                    break;
                case IO_LIST_PDO_AREA_T.RX_BLOCK:
                    objectList = _data_helper.RxBlockArea;
                    break;
            }
            __collection_areas[area].Clear();
            foreach (var o in objectList)
                __collection_areas[area].Add(__object_collection_data_model.ObjectDictionary[o.index]);
        }

        private List<IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T> __import_interlock_logic_target_objects(string text)
        {
            try
            {
                List<IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T> list = new List<IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T>();

                string[] objectIndexStrings = text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach(var str in objectIndexStrings)
                {
                    uint id = Convert.ToUInt32(str, 16);
                    list.Add(_data_helper.IOObjectDictionary[id]);
                }

                return list;
            }
            catch(Exception e)
            {
                throw new IOListParseExcepetion(IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION, e);
            }
        }

        private IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_EXPRESSION_T __import_interlock_logic_statement(string text)
        {
            return null;
        }

        public void AppendIntlklogicDefinition(string name, string targetString, string statementString)
        {
            IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_DEFINITION_T def =
                new IO_LIST_INTERLOCK_LOGIC_COLLECTION_T.LOGIC_DEFINITION_T(name,
                __import_interlock_logic_target_objects(targetString),
                __import_interlock_logic_statement(statementString));

            _data_helper.AppendInterlockLogicDefinition(def);

            List<ObjectItemDataModel> objectDataModels = new List<ObjectItemDataModel>();
            foreach (var o in def.target_objects)
                objectDataModels.Add(__object_collection_data_model.ObjectDictionary[o.index]);

            IntlkLogicExpression expressionDataModel =
                __load_interlock_logic_statement(def.statement, null) as IntlkLogicExpression;
            __intlk_logic_area.Add(new IntlklogicDefinition(name, objectDataModels, expressionDataModel));
        }
    }

    public abstract class IntlkLogicElement
    {
        public IO_LIST_INTERLOCK_LOGIC_ELEMENT_TYPE Type { get; private set; }
        public IntlkLogicExpression Root { get; private set; }
        public int Layer { get; private set; }
        public IntlkLogicElement(IO_LIST_INTERLOCK_LOGIC_ELEMENT_TYPE type, IntlkLogicExpression root)
        {
            Type = type;
            Root = root;
            if (root == null)
                Layer = 0;
            else
                Layer = root.Layer + 1;
        }
    }

    public class IntlkLogicOperand : IntlkLogicElement
    {
        public ObjectItemDataModel ObjectDataModel { get; private set; }
        public IntlkLogicOperand(ObjectItemDataModel objectDataModel, IntlkLogicExpression root) : base(IO_LIST_INTERLOCK_LOGIC_ELEMENT_TYPE.OPERAND, root)
        {
            ObjectDataModel = objectDataModel;
        }

        public override string ToString()
        {
            return "0x" + ObjectDataModel.Index.ToString("X8") + "\n";
        }
    }

    public class IntlkLogicExpression : IntlkLogicElement
    {
        public IO_LIST_INTERLOCK_LOGIC_OPERATOR_T LogicOperator { get; private set; }
        public List<IntlkLogicElement> Elements { get; private set; }

        public IntlkLogicExpression(IO_LIST_INTERLOCK_LOGIC_OPERATOR_T op, IntlkLogicExpression root) : base(IO_LIST_INTERLOCK_LOGIC_ELEMENT_TYPE.EXPRESSION, root)
        {
            LogicOperator = op;
            Elements = new List<IntlkLogicElement>();
        }

        public override string ToString()
        {
            string str = null;
            foreach (var e in Elements)
            {
                if (e.Type == IO_LIST_INTERLOCK_LOGIC_ELEMENT_TYPE.EXPRESSION)
                {
                    str += (e as IntlkLogicExpression).LogicOperator.ToString() + "\n" + (e as IntlkLogicExpression).Elements.ToString();
                }
                else
                    str += e.ToString();
            }
            return str;
        }
    }

    public class IntlklogicDefinition
    {
        public string Name { get; private set; }
        public List<ObjectItemDataModel> TargetObjects { get; private set; }
        public IntlkLogicExpression Statement { get; private set; }

        public IntlklogicDefinition(string name, List<ObjectItemDataModel> targets, IntlkLogicExpression statement)
        {
            Name = name;
            TargetObjects = targets;
            Statement = statement;
        }

        public string TargetObjectIndexList
        {
            get
            {
                string text = null;
                foreach (ObjectItemDataModel target in TargetObjects)
                    text += "0x" + target.Index.ToString("X8") + "\n";
                return text;
            }
        }
    }
}
