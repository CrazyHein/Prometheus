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

        private uint __tx_diag_offset_in_word, __tx_diag_size_in_word, __tx_diag_actual_size_in_word;
        private uint __tx_bit_offset_in_word, __tx_bit_size_in_word, __tx_bit_actual_size_in_word;
        private uint __tx_block_offset_in_word, __tx_block_size_in_word, __tx_block_actual_size_in_word;
        private uint __rx_control_offset_in_word, __rx_control_size_in_word;
        private uint __rx_bit_offset_in_word, __rx_bit_size_in_word;
        private uint __rx_block_offset_in_word, __rx_block_size_in_word;

        public PDOCollectionDataModel(IOListDataHelper helper) : base(helper)
        {
            TxDiagnosticArea = new ObservableCollection<ObjectItemDataModel>();
            TxBitArea = new ObservableCollection<ObjectItemDataModel>();
            TxBlockArea = new ObservableCollection<ObjectItemDataModel>();
            RxControlArea = new ObservableCollection<ObjectItemDataModel>();
            RxBitArea = new ObservableCollection<ObjectItemDataModel>();
            RxBlockArea = new ObservableCollection<ObjectItemDataModel>();
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
        public uint TxDiagnosticAreaActualSizeInWord
        {
            get { return __tx_diag_actual_size_in_word; }
            private set { SetProperty(ref __tx_diag_actual_size_in_word, value); }
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
        public uint TxBitAreaActualSizeInWord
        {
            get { return __tx_bit_actual_size_in_word; }
            private set { SetProperty(ref __tx_bit_actual_size_in_word, value); }
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
        public uint TxBlockAreaActualSizeInWord
        {
            get { return __tx_block_actual_size_in_word; }
            private set { SetProperty(ref __tx_block_actual_size_in_word, value); }
        }

        public override void UpdateDataHelper()
        {
            throw new NotImplementedException();
        }

        public override void UpdateDataModel()
        {
            TxDiagnosticAreaOffsetInWord = _data_helper.TxDiagnosticAreaOffset;
            TxDiagnosticAreaSizeInWord = _data_helper.TxDiagnosticAreaSize;
            TxDiagnosticAreaActualSizeInWord = _data_helper.TxDiagnosticAreaActualSize;
            TxDiagnosticArea.Clear();
            foreach (var o in _data_helper.TxDiagnosticArea)
            {
                ObjectItemDataModel model = new ObjectItemDataModel(o);
                TxDiagnosticArea.Add(model);
            }

            TxBitAreaOffsetInWord = _data_helper.TxBitAreaOffset;
            TxBitAreaSizeInWord = _data_helper.TxBitAreaAreaSize;
            TxBitAreaActualSizeInWord = _data_helper.TxBitAreaAreaActualSize;
            TxBitArea.Clear();
            foreach (var o in _data_helper.TxBitArea)
            {
                ObjectItemDataModel model = new ObjectItemDataModel(o);
                TxBitArea.Add(model);
            }

            TxBlockAreaOffsetInWord = _data_helper.TxBlockAreaOffset;
            TxBlockAreaSizeInWord = _data_helper.TxBlockAreaAreaSize;
            TxBlockAreaActualSizeInWord = _data_helper.TxBlockAreaAreaActualSize;
            TxBlockArea.Clear();
            foreach (var o in _data_helper.TxBlockArea)
            {
                ObjectItemDataModel model = new ObjectItemDataModel(o);
                TxBlockArea.Add(model);
            }

            RxControlArea.Clear();
            foreach (var o in _data_helper.RxControlArea)
            {
                ObjectItemDataModel model = new ObjectItemDataModel(o);
                RxControlArea.Add(model);
            }

            RxBitArea.Clear();
            foreach (var o in _data_helper.RxBitArea)
            {
                ObjectItemDataModel model = new ObjectItemDataModel(o);
                RxBitArea.Add(model);
            }

            RxBlockArea.Clear();
            foreach (var o in _data_helper.RxBlockArea)
            {
                ObjectItemDataModel model = new ObjectItemDataModel(o);
                TxBlockArea.Add(model);
            }
        }
    
    }
}
