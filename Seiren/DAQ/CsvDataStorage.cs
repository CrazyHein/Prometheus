using CsvHelper;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.DAQ
{
    internal class CsvDataStorage:IDisposable
    {
        public static readonly string FILE_TIMESTAMP_FORMAT = @"MMddyyHHmmssfff";
        public static readonly string RECORD_TIMESTAMP_FORMAT = @"MMddyy HH:mm:ss:fff";
        private string __storage_file_path;
        private string __storage_file_name_prefix;
        private int __storage_file_size_limit = 0;

        private FileStream __file_stream;
        private StreamWriter __stream_writer;
        private CsvWriter __csv_writer;

        private List<AcquisitionDataIndex> __diag_area, __tx_bit_area, __tx_blk_area, __ctrl_area, __rx_bit_area, __rx_blk_area;
        private bool disposedValue;

        private uint __last_time = 0;
        private System.DateTime __last_data_acquisition_date_time;

        public CsvDataStorage(string path, string prefix, int size, 
            IEnumerable<AcquisitionDataIndex> diag, IEnumerable<AcquisitionDataIndex> DI, IEnumerable<AcquisitionDataIndex> AI,
            IEnumerable<AcquisitionDataIndex> ctrl, IEnumerable<AcquisitionDataIndex> DO, IEnumerable<AcquisitionDataIndex> AO)
        {
            __diag_area = new List<AcquisitionDataIndex>(diag);
            __tx_bit_area = new List<AcquisitionDataIndex>(DI);
            __tx_blk_area = new List<AcquisitionDataIndex>(AI);
            __ctrl_area = new List<AcquisitionDataIndex>(ctrl);
            __rx_bit_area = new List<AcquisitionDataIndex>(DO);
            __rx_blk_area = new List<AcquisitionDataIndex>(AO);
            __storage_file_path = path;
            __storage_file_name_prefix = prefix;
            __storage_file_size_limit = size*1024;

            __last_data_acquisition_date_time = System.DateTime.Now;
        }

        public void InitializeTimestamp(System.DateTime date, uint start)
        {
            __last_time = start;
            __last_data_acquisition_date_time = date;
        }

        public void WriteRecord(uint time, 
                                ReadOnlySpan<byte> diagdata, ReadOnlySpan<byte> txbitdata, ReadOnlySpan<byte> txblkdata,
                                ReadOnlySpan<byte> ctrldata, ReadOnlySpan<byte> rxbitdata, ReadOnlySpan<byte> rxblkdata)
        {

            if(__file_stream != null && __file_stream.Length > __storage_file_size_limit)
            {
                __csv_writer?.Flush();
                __csv_writer?.Dispose();
                __stream_writer?.Dispose();
                __file_stream.Dispose();

                __file_stream = null;
                __stream_writer = null;
                __csv_writer = null;
            }

            if (__file_stream == null)
            {
                __file_stream = new FileStream(Path.Combine(__storage_file_path, __storage_file_name_prefix + $"{DateTime.Now.ToString(FILE_TIMESTAMP_FORMAT)}.csv"), FileMode.Create, FileAccess.Write, FileShare.Read);
                __stream_writer = new StreamWriter(__file_stream, Encoding.ASCII);
                __csv_writer = new CsvWriter(__stream_writer, CultureInfo.InvariantCulture);

                __csv_writer.WriteField("Timestamp");
                foreach(var f in __diag_area)
                    __csv_writer.WriteField(f.FriendlyName);
                foreach (var f in __tx_bit_area)
                    __csv_writer.WriteField(f.FriendlyName);
                foreach (var f in __tx_blk_area)
                    __csv_writer.WriteField(f.FriendlyName);

                foreach (var f in __ctrl_area)
                    __csv_writer.WriteField(f.FriendlyName);
                foreach (var f in __rx_bit_area)
                    __csv_writer.WriteField(f.FriendlyName);
                foreach (var f in __rx_blk_area)
                    __csv_writer.WriteField(f.FriendlyName);

                __csv_writer.NextRecord();
            }

            __last_data_acquisition_date_time += TimeSpan.FromMilliseconds((time - __last_time + 500) / 1000);
            __csv_writer.WriteField(__last_data_acquisition_date_time.ToString(RECORD_TIMESTAMP_FORMAT));
            foreach (var f in __diag_area)
                __csv_writer.WriteField(f.DataStringValue(diagdata));
            foreach (var f in __tx_bit_area)
                __csv_writer.WriteField(f.DataStringValue(txbitdata));
            foreach (var f in __tx_blk_area)
                __csv_writer.WriteField(f.DataStringValue(txblkdata));

            foreach (var f in __ctrl_area)
                __csv_writer.WriteField(f.DataStringValue(ctrldata));
            foreach (var f in __rx_bit_area)
                __csv_writer.WriteField(f.DataStringValue(rxbitdata));
            foreach (var f in __rx_blk_area)
                __csv_writer.WriteField(f.DataStringValue(rxblkdata));

            __csv_writer.NextRecord();

            __last_time = time;
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    __csv_writer?.Flush();
                    __csv_writer?.Dispose();
                    __stream_writer?.Dispose();
                    __file_stream?.Dispose();

                    __file_stream = null;
                    __stream_writer = null;
                    __csv_writer = null;
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~CsvDataStorage()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
