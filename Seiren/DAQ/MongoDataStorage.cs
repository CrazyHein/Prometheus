﻿using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.DAQ
{
    internal class MongoDataStorage : IDataStorage
    {
        public class ValidateResult : IDisposable
        {
            private bool disposedValue;

            public Exception? Exception { get; init; }
            public MongoClient? Client { get; init; }
            public IMongoCollection<BsonDocument>? Collection { get; init; }
            public bool IsCapped { get; init; } = false;
            public int Size { get; init; } = 0;
            public bool New { get; init; } = false;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: 释放托管状态(托管对象)
                        Client?.Dispose();
                    }

                    // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                    // TODO: 将大型字段设置为 null
                    disposedValue = true;
                }
            }

            // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
            // ~ValidateResult()
            // {
            //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            //     Dispose(disposing: false);
            // }

            void IDisposable.Dispose()
            {
                // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
        public static ValidateResult ValidateMongoDBStorage(string connectiongString, string databaseName, string collectionName, bool createIfNotExist = false, int preferSize = 0)
        {
            try
            {
                var client = new MongoClient(connectiongString);
                var info = client.GetDatabase(databaseName).ListCollections(new ListCollectionsOptions { Filter = new BsonDocument("name", collectionName) }).FirstOrDefault();
                
                if(info == null)
                {
                    if (createIfNotExist)
                    {
                        client.GetDatabase(databaseName).CreateCollection(collectionName, new CreateCollectionOptions() { Capped = true, MaxSize = preferSize });
                        return new ValidateResult() { IsCapped = true, Size = preferSize, New = true, Client = client, Collection = client.GetDatabase(databaseName).GetCollection<BsonDocument>(collectionName) };
                    }
                    else
                        return new ValidateResult();
                }
                else if(info["options"] != null)
                {
                    var options = info["options"].AsBsonDocument;
                    bool capped = false;
                    if (options["capped"] != null)
                        capped = options["capped"].AsBoolean;
                    int size = 0;
                    if (options["size"] != null)
                        size = options["size"].AsInt32;
                    return new ValidateResult() { IsCapped = capped, Size = size, Client = client, Collection = client.GetDatabase(databaseName).GetCollection<BsonDocument>(collectionName) };
                }
                else
                    return new ValidateResult() {Client = client, Collection = client.GetDatabase(databaseName).GetCollection<BsonDocument>(collectionName) };
            }
            catch (Exception e)
            {
                return new ValidateResult() { Exception = e};
            }
        }

        private ValidateResult? __validate_result;
        private uint __last_time = 0;
        private System.DateTime __last_data_acquisition_date_time;
        private List<AcquisitionDataIndex> __diag_area, __tx_bit_area, __tx_blk_area, __ctrl_area, __rx_bit_area, __rx_blk_area;
        private List<BsonDocument> __documents = new List<BsonDocument>();

        public MongoDataStorage(string connectiongString, string databaseName, string collectionName, int preferSize,
            IEnumerable<AcquisitionDataIndex> diag, IEnumerable<AcquisitionDataIndex> DI, IEnumerable<AcquisitionDataIndex> AI,
            IEnumerable<AcquisitionDataIndex> ctrl, IEnumerable<AcquisitionDataIndex> DO, IEnumerable<AcquisitionDataIndex> AO)
        {
            var res = ValidateMongoDBStorage(connectiongString, databaseName, collectionName, true, preferSize);
            if (res.Exception != null)
                throw res.Exception;

            __validate_result = res;
            __diag_area = new List<AcquisitionDataIndex>(diag);
            __tx_bit_area = new List<AcquisitionDataIndex>(DI);
            __tx_blk_area = new List<AcquisitionDataIndex>(AI);
            __ctrl_area = new List<AcquisitionDataIndex>(ctrl);
            __rx_bit_area = new List<AcquisitionDataIndex>(DO);
            __rx_blk_area = new List<AcquisitionDataIndex>(AO);
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
            __last_data_acquisition_date_time += TimeSpan.FromMilliseconds((time - __last_time + 500) / 1000);
            BsonDocument bsonDocument = new BsonDocument("time", new BsonDateTime(__last_data_acquisition_date_time));
            foreach (var f in __diag_area)
                bsonDocument.Add(f.FriendlyName, f.DataBsonValue(diagdata));
            foreach (var f in __tx_bit_area)
                bsonDocument.Add(f.FriendlyName, f.DataBsonValue(txbitdata));
            foreach (var f in __tx_blk_area)
                bsonDocument.Add(f.FriendlyName, f.DataBsonValue(txblkdata));
            foreach (var f in __ctrl_area)
                bsonDocument.Add(f.FriendlyName, f.DataBsonValue(ctrldata));
            foreach (var f in __rx_bit_area)
                bsonDocument.Add(f.FriendlyName, f.DataBsonValue(rxbitdata));
            foreach (var f in __rx_blk_area)
                bsonDocument.Add(f.FriendlyName, f.DataBsonValue(rxblkdata));

            __documents.Add(bsonDocument);

            __last_time = time;
        }

        public void Flush()
        {
            if (__documents.Count > 0)
            {
                __validate_result.Collection.InsertMany(__documents);
                __documents.Clear();
            }
        }

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    Flush();
                    (__validate_result as IDisposable)?.Dispose();
                    __validate_result = null;
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~MongoDataStorage()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        void IDisposable.Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
