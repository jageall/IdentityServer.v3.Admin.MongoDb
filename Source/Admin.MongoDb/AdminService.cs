/*
 * Copyright 2014, 2015 James Geall
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Threading.Tasks;
using IdentityServer.Core.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;
using Builder = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>;
namespace IdentityServer.Admin.MongoDb
{
    internal class AdminService : IAdminService
    {
        private readonly IMongoClient _client;
        private readonly IMongoDatabase _db;
        private readonly StoreSettings _settings;
        private readonly ClientSerializer _clientSerializer;
        private static readonly ILog _log = LogProvider.For<AdminService>();

        public AdminService(IMongoClient client, IMongoDatabase db, StoreSettings settings)
        {
            _client = client;
            _db = db;
            _settings = settings;
            _clientSerializer = new ClientSerializer();
        }

        public async Task CreateDatabase(bool expireUsingIndex = true)
        {
            var cursor = await _db.ListCollectionsAsync();
            var list = await cursor.ToListAsync();

            if (list.CollectionExists(_settings.ClientCollection))
            {
                await _db.CreateCollectionAsync(_settings.ClientCollection);
            }
            if (!list.CollectionExists(_settings.ScopeCollection))
            {
                await _db.CreateCollectionAsync(_settings.ScopeCollection);
            }
            if (!list.CollectionExists(_settings.ConsentCollection))
            {
                var collection = _db.GetCollection<BsonDocument>(_settings.ConsentCollection);

                await collection.Indexes.CreateOneAsync("subject");


                await collection.Indexes.CreateOneAsync(Builder.IndexKeys.Combine("clientId", "subject"));

            }

            var tokenCollections = new[]
            {
                _settings.AuthorizationCodeCollection,
                _settings.RefreshTokenCollection,
                _settings.TokenHandleCollection
            };

            foreach (string tokenCollection in tokenCollections)
            {
                IMongoCollection<BsonDocument> collection = _db.GetCollection<BsonDocument>(tokenCollection);
                var options = new CreateIndexOptions() {ExpireAfter = TimeSpan.FromSeconds(1)};


                await collection.Indexes.CreateOneAsync(Builder.IndexKeys.Combine(
                    Builder.IndexKeys.Ascending("_clientId"), Builder.IndexKeys.Ascending("_subjectId")));
                
                try
                {
                    await collection.Indexes.CreateOneAsync(Builder.IndexKeys
                        .Ascending("_expires"),
                        options);
                }
                catch (MongoWriteConcernException)
                {
                    await collection.Indexes.DropOneAsync("_expires");
                    await collection.Indexes.CreateOneAsync(Builder.IndexKeys
                    .Ascending("_expires"),
                    options);
                }
            }
        }

        public async Task Save(Scope scope)
        {
            BsonDocument doc = new ScopeSerializer().Serialize(scope);
            IMongoCollection<BsonDocument> collection = _db.GetCollection<BsonDocument>(_settings.ScopeCollection);
            var result = await collection.ReplaceOneAsync(Filter.ById(scope.Name), doc, new UpdateOptions() {IsUpsert = true} );
            _log.Debug(result.ToString);
        }

        public async Task Save(Client client)
        {
            BsonDocument doc = _clientSerializer.Serialize(client);
            IMongoCollection<BsonDocument> collection = _db.GetCollection<BsonDocument>(_settings.ClientCollection);
            var result = await collection.ReplaceOneAsync(
                Filter.ById(client.ClientId), 
                doc,
                new UpdateOptions() { IsUpsert = true}
                );
            _log.Debug(result.ToString);
        }

        public async Task RemoveDatabase()
        {
            await _client.DropDatabaseAsync(_settings.Database);
        }

        public async Task DeleteClient(string clientId)
        {
            IMongoCollection<BsonDocument> collection = _db.GetCollection<BsonDocument>(_settings.ClientCollection);
            var result = await collection.DeleteOneByIdAsync(clientId);
            _log.Debug(result.ToString);
        }

        public async Task DeleteScope(string scopeName)
        {
            IMongoCollection<BsonDocument> collection = _db.GetCollection<BsonDocument> (_settings.ScopeCollection);
            var result = await collection.DeleteOneByIdAsync(scopeName );
            _log.Debug(result.ToString);
        }
    }
}