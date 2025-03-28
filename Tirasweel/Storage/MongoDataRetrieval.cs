using MongoDB.Bson;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Tirasweel.Storage
{
    public class MongoDataRetrieval : IDataRetrieval
    {
        public MongoClient? Client { get; init; }
        public IMongoCollection<BsonDocument>? Collection { get; init; }
        private string __connection_string;
        private string __database_name;
        private string __collection_name;
        private bool disposedValue;

        public MongoDataRetrieval(string connection, string database, string collection)
        {
            __connection_string = connection;
            __database_name = database;
            __collection_name = collection;

            Client = new MongoClient(__connection_string);
            Collection = Client.GetDatabase(__database_name).GetCollection<BsonDocument>(__collection_name);
        }
        public (long counts, DateTime start, DateTime end) Summary(CancellationToken cancel)
        {
            try
            {        
                var counts = Collection.EstimatedDocumentCount();
                if (counts > 0)
                {
                    var filter = Builders<BsonDocument>.Filter.Empty;
                    var project = Builders<BsonDocument>.Projection.Include("time");
                    var asort = Builders<BsonDocument>.Sort.Ascending("time");
                    var dsort = Builders<BsonDocument>.Sort.Descending("time");
                    var startBson = Collection.Find(filter).Sort(asort).Project(project).Limit(1).First(cancel);
                    var start = startBson["time"].ToUniversalTime();
                    var endBson = Collection.Find(filter).Sort(dsort).Project(project).Limit(1).First(cancel);
                    var end = endBson["time"].ToUniversalTime();

                    return (counts, start, end);
                }
                else
                {
                    return (0, DateTime.MinValue, DateTime.MinValue);
                }
            }
            catch (Exception)
            {
                return (0, DateTime.MinValue, DateTime.MinValue);
            }
        }

        public IEnumerable<byte[]> Latest(int milliseconds, CancellationToken cancel)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Empty;
                var project = Builders<BsonDocument>.Projection.Include("time");
                var asort = Builders<BsonDocument>.Sort.Ascending("time");
                var dsort = Builders<BsonDocument>.Sort.Descending("time");
                var endBson = Collection.Find(filter).Sort(dsort).Project(project).Limit(1).First(cancel);
                var end = endBson["time"].ToUniversalTime();

                var start = end - TimeSpan.FromMilliseconds(milliseconds);
                project = Builders<BsonDocument>.Projection.Exclude("_id");

                filter = Builders<BsonDocument>.Filter.Gte("time", start) & Builders<BsonDocument>.Filter.Lte("time", end);
                return Collection.Find(filter).Sort(asort).Project(project).ToEnumerable(cancel).Select(b => b.ToBson());
            }
            catch (Exception)
            {
                return Enumerable.Empty<byte[]>();
            }
        }

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
        // ~MongoDataRetrieval()
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

        public IEnumerable<byte[]> Head(int counts, CancellationToken cancel)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Empty;
                var dsort = Builders<BsonDocument>.Sort.Descending("time");
                var project = Builders<BsonDocument>.Projection.Exclude("_id");

                return Collection.Find(filter).Sort(dsort).Project(project).Limit(counts).ToEnumerable(cancel).Select(b => b.ToBson());
            }
            catch (Exception)
            {
                return Enumerable.Empty<byte[]>();
            }
        }

        public IEnumerable<byte[]> Tail(int counts, CancellationToken cancel)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Empty;
                var asort = Builders<BsonDocument>.Sort.Ascending("time");
                var project = Builders<BsonDocument>.Projection.Exclude("_id");

                return Collection.Find(filter).Sort(asort).Project(project).Limit(counts).ToEnumerable(cancel).Select(b => b.ToBson());
            }
            catch (Exception)
            {
                return Enumerable.Empty<byte[]>();
            }
        }
    }
}
