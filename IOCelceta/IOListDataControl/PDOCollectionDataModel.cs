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
        public ObservableCollection<ObjectItemDataModel> TxDiagnosticArea { get; private set; }
        public ObservableCollection<ObjectItemDataModel> TxBitArea { get; private set; }
        public ObservableCollection<ObjectItemDataModel> TxBlockArea { get; private set; }
        public ObservableCollection<ObjectItemDataModel> RxControlArea { get; private set; }
        public ObservableCollection<ObjectItemDataModel> RxBitArea { get; private set; }
        public ObservableCollection<ObjectItemDataModel> RxBlockArea { get; private set; }
        public ObservableCollection<ObjectItemDataModel> AvailableObjects { get { return __object_collection_data_model.Objects; }}
        public ObjectItemFilter AvailableObjectItemFilter { get; private set; }
        public string FilterFriendlyName { get; set; }


        private uint __tx_diag_offset_in_word, __tx_diag_size_in_word, __tx_diag_actual_size_in_byte;
        private uint __tx_bit_offset_in_word, __tx_bit_size_in_word, __tx_bit_actual_size_in_byte;
        private uint __tx_block_offset_in_word, __tx_block_size_in_word, __tx_block_actual_size_in_byte;
        private uint __rx_control_offset_in_word, __rx_control_size_in_word, __rx_control_actual_size_in_byte;
        private uint __rx_bit_offset_in_word, __rx_bit_size_in_word, __rx_bit_actual_size_in_byte;
        private uint __rx_block_offset_in_word, __rx_block_size_in_word, __rx_block_actual_size_in_byte;

        private ObjectCollectionDataModel __object_collection_data_model;

        public PDOCollectionDataModel(IOListDataHelper helper, ObjectCollectionDataModel objectCollectionDataModel) : base(helper)
        {
            TxDiagnosticArea = new ObservableCollection<ObjectItemDataModel>();
            TxBitArea = new ObservableCollection<ObjectItemDataModel>();
            TxBlockArea = new ObservableCollection<ObjectItemDataModel>();
            RxControlArea = new ObservableCollection<ObjectItemDataModel>();
            RxBitArea = new ObservableCollection<ObjectItemDataModel>();
            RxBlockArea = new ObservableCollection<ObjectItemDataModel>();
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
            TxDiagnosticArea.Clear();
            foreach (var o in _data_helper.TxDiagnosticArea)
            {
                TxDiagnosticArea.Add(__object_collection_data_model.ObjectDictionary[o.index]);
            }

            TxBitAreaOffsetInWord = _data_helper.TxBitAreaOffset;
            TxBitAreaSizeInWord = _data_helper.TxBitAreaSize;
            TxBitAreaActualSizeInByte = _data_helper.TxBitAreaActualSize;
            TxBitArea.Clear();
            foreach (var o in _data_helper.TxBitArea)
            {
                //ObjectItemDataModel model = new ObjectItemDataModel(o);
                TxBitArea.Add(__object_collection_data_model.ObjectDictionary[o.index]);
            }

            TxBlockAreaOffsetInWord = _data_helper.TxBlockAreaOffset;
            TxBlockAreaSizeInWord = _data_helper.TxBlockAreaSize;
            TxBlockAreaActualSizeInByte = _data_helper.TxBlockAreaActualSize;
            TxBlockArea.Clear();
            foreach (var o in _data_helper.TxBlockArea)
            {
                //ObjectItemDataModel model = new ObjectItemDataModel(o);
                TxBlockArea.Add(__object_collection_data_model.ObjectDictionary[o.index]);
            }

            RxControlAreaOffsetInWord = _data_helper.RxControlAreaOffset;
            RxControlAreaSizeInWord = _data_helper.RxControlAreaSize;
            RxControlAreaActualSizeInByte = _data_helper.RxControlAreaActualSize;
            RxControlArea.Clear();
            foreach (var o in _data_helper.RxControlArea)
            {
                //ObjectItemDataModel model = new ObjectItemDataModel(o);
                RxControlArea.Add(__object_collection_data_model.ObjectDictionary[o.index]);
            }

            RxBitAreaOffsetInWord = _data_helper.RxBitAreaOffset;
            RxBitAreaSizeInWord = _data_helper.RxBitAreaSize;
            RxBitAreaActualSizeInByte = _data_helper.RxBitAreaActualSize;
            RxBitArea.Clear();
            foreach (var o in _data_helper.RxBitArea)
            {
                //ObjectItemDataModel model = new ObjectItemDataModel(o);
                RxBitArea.Add(__object_collection_data_model.ObjectDictionary[o.index]);
            }

            RxBlockAreaOffsetInWord = _data_helper.RxBlockAreaOffset;
            RxBlockAreaSizeInWord = _data_helper.RxBlockAreaSize;
            RxBlockAreaActualSizeInByte = _data_helper.RxBlockAreaActualSize;
            RxBlockArea.Clear();
            foreach (var o in _data_helper.RxBlockArea)
            {
                //ObjectItemDataModel model = new ObjectItemDataModel(o);
                RxBlockArea.Add(__object_collection_data_model.ObjectDictionary[o.index]);
            }
        }
    
    }
}
