using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer3.MongoDb;
using IdentityServer3.Admin.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace Admin.MongoDb.Tests
{
    public class CreateDatabaseTests: IAsyncLifetime
    {
        private readonly StoreSettings _settings;
        private readonly MongoClient _client;
        private readonly IAdminService _service;

        public CreateDatabaseTests()
        {
            _settings = StoreSettings.DefaultSettings();
            _settings.Database = Guid.NewGuid().ToString();
            _service = AdminServiceFactory.Create(_settings);
            _client = new MongoClient(_settings.ConnectionString);
        }

        [Fact]
        public async Task ShouldCreateDatabase()
        {
            var cursor = await _client.ListDatabasesAsync();
            var list = await cursor.ToListAsync();
            Assert.Contains(list, x=>x["name"] == _settings.Database);
        }

        [Fact]
        public async Task ShouldCreateClientCollection()
        {
            var db = _client.GetDatabase(_settings.Database);
            var cursor = await db.ListCollectionsAsync();
            var list = await cursor.ToListAsync();
            Assert.Contains(list, x => x["name"] == _settings.ClientCollection);
            var collection = db.GetCollection<BsonDocument>(_settings.ClientCollection);
            cursor = await collection.Indexes.ListAsync();
            var indexes = await cursor.ToListAsync();
            Assert.Contains(indexes, x =>
            {
                var key = x["key"].AsBsonDocument;
                return key.Count() == 1 && key.First().Name == "_id";
            });
        }

        [Fact]
        public async Task ShouldCreateScopeCollection()
        {
            var db = _client.GetDatabase(_settings.Database);
            var cursor = await db.ListCollectionsAsync();
            var list = await cursor.ToListAsync();
            Assert.Contains(list, x => x["name"] == _settings.ScopeCollection);
            var collection = db.GetCollection<BsonDocument>(_settings.ScopeCollection);
            cursor = await collection.Indexes.ListAsync();
            var indexes = await cursor.ToListAsync();
            Assert.Contains(indexes, x =>
            {
                var key = x["key"].AsBsonDocument;
                return key.Count() == 1 && key.First().Name == "_id";
            });
        }
        [Fact]
        public async Task ShouldCreateConsentCollection()
        {
            var db = _client.GetDatabase(_settings.Database);
            var cursor = await db.ListCollectionsAsync();
            var list = await cursor.ToListAsync();
            Assert.Contains(list, x => x["name"] == _settings.ConsentCollection);
            var collection = db.GetCollection<BsonDocument>(_settings.ConsentCollection);
            cursor = await collection.Indexes.ListAsync();
            var indexes = await cursor.ToListAsync();
            Assert.Contains(indexes, x =>
            {
                var key = x["key"].AsBsonDocument;
                return key.Count() == 1 && key.First().Name == "_id";
            });
            Assert.Contains(indexes, index =>
            {
                var key = index["key"].AsBsonDocument;
                return key.Count() == 2 && key.Any(x => x.Name == "clientId") && key.Any(x => x.Name == "subject");
            });
        }

        [Fact]
        public async Task ShouldCreateAuthorizationCodeCollection()
        {
            var db = _client.GetDatabase(_settings.Database);
            var cursor = await db.ListCollectionsAsync();
            var list = await cursor.ToListAsync();
            Assert.Contains(list, x => x["name"] == _settings.AuthorizationCodeCollection);
            var collection = db.GetCollection<BsonDocument>(_settings.AuthorizationCodeCollection);
            cursor = await collection.Indexes.ListAsync();
            var indexes = await cursor.ToListAsync();
            VerifyTokenCollectionIndexes(indexes);
        }

        [Fact]
        public async Task ShouldCreateRefreshTokenCollection()
        {
            var db = _client.GetDatabase(_settings.Database);
            var cursor = await db.ListCollectionsAsync();
            var list = await cursor.ToListAsync();
            Assert.Contains(list, x => x["name"] == _settings.RefreshTokenCollection);
            var collection = db.GetCollection<BsonDocument>(_settings.RefreshTokenCollection);
            cursor = await collection.Indexes.ListAsync();
            var indexes = await cursor.ToListAsync();
            VerifyTokenCollectionIndexes(indexes);
        }
        [Fact]
        public async Task ShouldCreateTokenHandleCollection()
        {
            var db = _client.GetDatabase(_settings.Database);
            var cursor = await db.ListCollectionsAsync();
            var list = await cursor.ToListAsync();
            Assert.Contains(list, x => x["name"] == _settings.TokenHandleCollection);
            var collection = db.GetCollection<BsonDocument>(_settings.TokenHandleCollection);
            cursor = await collection.Indexes.ListAsync();
            var indexes = await cursor.ToListAsync();
            VerifyTokenCollectionIndexes(indexes);
        }

        private static void VerifyTokenCollectionIndexes(List<BsonDocument> indexes)
        {
            Assert.Contains(indexes, x =>
            {
                var key = x["key"].AsBsonDocument;
                return key.Count() == 1 && key.First().Name == "_id";
            });

            Assert.Contains(indexes, index =>
            {
                var key = index["key"].AsBsonDocument;
                return key.Count() == 2 && key.Any(x => x.Name == "_clientId") && key.Any(x => x.Name == "_subjectId");
            });

            Assert.Contains(indexes, index =>
            {
                var key = index["key"].AsBsonDocument;
                if (key.Count() == 1 && key.First().Name == "_expires")
                {
                    return index["expireAfterSeconds"] == 1;
                }
                return false;
            });
        }

        [Fact]
        public async Task ShouldBeAbleToRunCreateDatabaseMultipleTimes()
        {
            await _service.CreateDatabase();
            await _service.CreateDatabase();
        }

        
        public Task InitializeAsync()
        {
            return _service.CreateDatabase();
        }

        public Task DisposeAsync()
        {
            return _client.DropDatabaseAsync(_settings.Database);
        }
    }
}
