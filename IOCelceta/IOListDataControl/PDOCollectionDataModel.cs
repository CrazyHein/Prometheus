using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.Catalogue;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
        public string FilterFriendlyName { get; set; }


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


            __object_collection_data_model = objectCollectionDataModel;

            FilterFriendlyName = "";
            AvailableObjectItemFilter = new ObjectItemFilter(null, null, FilterFriendlyName);
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

        public void UpdateAreaActualSize(IO_LIST_PDO_AREA_T area)
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

        public override void UpdateDataHelper()
        {
            throw new NotImplementedException();
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
        }

        public void SwapPDOMapping(IO_LIST_PDO_AREA_T area, int first, int second)
        {
            _data_helper.SwapPDOMapping(area, first, second);
            //UpdateAreaActualSize(area);
            ObjectItemDataModel temp = __collection_areas[area][first];
            //__collection_areas[area][first] = __collection_areas[area][second];
            __collection_areas[area].Move(second, first);
            __collection_areas[area][second] = temp;
        }

        public void InsertPDOMapping(int pos, IO_LIST_PDO_AREA_T area, uint objectIndex)
        {
            IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData = _data_helper.IOObjectDictionary[objectIndex];
            _data_helper.InsertPDOMapping(pos, area, objectData);
            UpdateAreaActualSize(area);
            __collection_areas[area].Insert(pos, __object_collection_data_model.ObjectDictionary[objectIndex]);
        }

        public void InsertPDOMapping(int pos, IO_LIST_PDO_AREA_T area, ObjectItemDataModel objectDataModel)
        {
            IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData = _data_helper.IOObjectDictionary[objectDataModel.Index];
            _data_helper.InsertPDOMapping(pos, area, objectData);
            UpdateAreaActualSize(area);
            __collection_areas[area].Insert(pos, objectDataModel);
        }

        public void RemovePDOMapping(int pos, IO_LIST_PDO_AREA_T area)
        {
            _data_helper.RemovePDOMapping(pos, area);
            UpdateAreaActualSize(area);
            __collection_areas[area].RemoveAt(pos);
        }

        public void AppendPDOMapping(IO_LIST_PDO_AREA_T area, uint objectIndex)
        {
            IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData = _data_helper.IOObjectDictionary[objectIndex];
            _data_helper.AppendPDOMapping(area, objectData);
            UpdateAreaActualSize(area);
            __collection_areas[area].Add(__object_collection_data_model.ObjectDictionary[objectIndex]);
        }

        public void AppendPDOMapping(IO_LIST_PDO_AREA_T area, ObjectItemDataModel objectDataModel)
        {
            IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData = _data_helper.IOObjectDictionary[objectDataModel.Index];
            _data_helper.AppendPDOMapping(area, objectData);
            UpdateAreaActualSize(area);
            __collection_areas[area].Add(objectDataModel);
        }

        public void ReplacePDOMapping(IO_LIST_PDO_AREA_T area, int pos, uint objectIndex)
        {
            IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData = _data_helper.IOObjectDictionary[objectIndex];
            _data_helper.ReplacePDOMapping(area, pos, objectData);
            UpdateAreaActualSize(area);
            __collection_areas[area][pos] = __object_collection_data_model.ObjectDictionary[objectIndex];
        }

        public void ReplacePDOMapping(IO_LIST_PDO_AREA_T area, int pos, ObjectItemDataModel objectDataModel)
        {
            IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T objectData = _data_helper.IOObjectDictionary[objectDataModel.Index];
            _data_helper.ReplacePDOMapping(area, pos, objectData);
            UpdateAreaActualSize(area);
            __collection_areas[area][pos] = objectDataModel;
        }
    }
}
