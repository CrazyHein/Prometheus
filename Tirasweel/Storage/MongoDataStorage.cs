using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMEC.PCSoftware.CommunicationProtocol.CrazyHein.OrbmentDAQ.Storage
{
    public class MongoDataStorage : IDataStorage
    {
        private static readonly byte[] __DUMMY_MD5 = new byte[16];

        public static readonly string TIME_FIELD = "time";
        public static readonly string TIME_INDEX_NAME = "time_ascending";

        public static readonly string RECIPE_ENGINE_STATE_VARIABLE_NAME = "Recipe Engine State";
        public static readonly string RECIPE_ENGINE_STATE_FIELD = "engine_state";
        public static AcquisitionDataType RECIPE_ENGINE_STATE_VARIABLE_TYPE = AcquisitionDataType.UINT;
        public static readonly string RECIPE_ID_VARIABLE_NAME = "Recipe ID";
        public static readonly string RECIPE_ID_FIELD = "recipe_id";
        public static AcquisitionDataType RECIPE_ID_TYPE = AcquisitionDataType.FINGERPRINT;
        public static readonly string RECIPE_PROCESS_DATA_INDEX_NAME = "recipe_process_data";

        public class ValidateResult : IDisposable
        {
            private bool disposedValue;

            public Exception? Exception { get; init; }
            public MongoClient? Client { get; init; }
            public IMongoCollection<BsonDocument>? Collection { get; init; }
            public bool IsCapped { get; init; } = false;
            public long Size { get; init; } = 0;
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
        public static ValidateResult ValidateMongoDBStorage(string connectiongString, string databaseName, string collectionName, bool createIfNotExist = false, long preferSize = 0)
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
                        var collection = client.GetDatabase(databaseName).GetCollection<BsonDocument>(collectionName);
                        collection.Indexes.CreateOne(new CreateIndexModel<BsonDocument>($"{{'{TIME_FIELD}': 1}}", new CreateIndexOptions { Name = TIME_INDEX_NAME }));

                        collection.Indexes.CreateOne(new CreateIndexModel<BsonDocument>($"{{'{RECIPE_ID_FIELD}': 1, '{RECIPE_ENGINE_STATE_FIELD}': 1}}", new CreateIndexOptions { Name = RECIPE_PROCESS_DATA_INDEX_NAME }));

                        return new ValidateResult() { IsCapped = true, Size = preferSize, New = true, Client = client, Collection = collection };
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
                    long size = 0;
                    if (options["size"] != null)
                    {
                        if (options["size"].BsonType == BsonType.Int32)
                            size = options["size"].AsInt32;
                        else
                            size = options["size"].AsInt64;
                    }
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

        public MongoDataStorage(string connectiongString, string databaseName, string collectionName, long preferSize,
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
            BsonDocument rootBsonDocument = new BsonDocument("time", new BsonDateTime(__last_data_acquisition_date_time));
            rootBsonDocument.Add("recipe_id", new BsonBinaryData(__DUMMY_MD5, BsonBinarySubType.MD5));
            rootBsonDocument.Add("engine_state", new BsonInt64(0));

            BsonArray variables = new BsonArray();
            foreach (var f in __diag_area)
            {
                BsonDocument variable = new BsonDocument("name", f.FriendlyName);
                variable.Add("bits", f.BitPos);
                variable.Add("value", f.DataBsonValue(diagdata));
                variables.Add(variable);
                if(f.FriendlyName == RECIPE_ENGINE_STATE_VARIABLE_NAME &&  f.DataType == RECIPE_ENGINE_STATE_VARIABLE_TYPE)
                    rootBsonDocument[RECIPE_ENGINE_STATE_FIELD] = variable["value"].AsBsonValue;
            }
            rootBsonDocument.Add("tx_diagnostic_area", variables);

            variables = new BsonArray();
            foreach (var f in __tx_bit_area)
            {
                BsonDocument variable = new BsonDocument("name", f.FriendlyName);
                variable.Add("bits", f.BitPos);
                variable.Add("value", f.DataBsonValue(txbitdata));
                variables.Add(variable);
            }
            rootBsonDocument.Add("tx_bit_area", variables);

            variables = new BsonArray();
            foreach (var f in __tx_blk_area)
            {
                BsonDocument variable = new BsonDocument("name", f.FriendlyName);
                variable.Add("bits", f.BitPos);
                variable.Add("value", f.DataBsonValue(txblkdata));
                variables.Add(variable);
            }
            rootBsonDocument.Add("tx_block_area", variables);

            variables = new BsonArray();
            foreach (var f in __ctrl_area)
            {
                BsonDocument variable = new BsonDocument("name", f.FriendlyName);
                variable.Add("bits", f.BitPos);
                variable.Add("value", f.DataBsonValue(ctrldata));
                variables.Add(variable);
                if (f.FriendlyName == RECIPE_ID_VARIABLE_NAME && f.DataType == RECIPE_ID_TYPE)
                    rootBsonDocument[RECIPE_ID_FIELD] = variable["value"].AsBsonValue;
            }
            rootBsonDocument.Add("rx_control_area", variables);

            variables = new BsonArray();
            foreach (var f in __rx_bit_area)
            {
                BsonDocument variable = new BsonDocument("name", f.FriendlyName);
                variable.Add("bits", f.BitPos);
                variable.Add("value", f.DataBsonValue(rxbitdata));
                variables.Add(variable);
            }
            rootBsonDocument.Add("rx_bit_area", variables);

            variables = new BsonArray();
            foreach (var f in __rx_blk_area)
            {
                BsonDocument variable = new BsonDocument("name", f.FriendlyName);
                variable.Add("bits", f.BitPos);
                variable.Add("value", f.DataBsonValue(rxblkdata));
                variables.Add(variable);
            }
            rootBsonDocument.Add("rx_block_area", variables);

            __documents.Add(rootBsonDocument);

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
                    try
                    {
                        Flush();
                    }
                    finally
                    {
                        (__validate_result as IDisposable)?.Dispose();
                        __validate_result = null;
                    }
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
