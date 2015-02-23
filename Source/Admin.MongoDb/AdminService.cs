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
using IdentityServer.Core.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Wrappers;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;

namespace IdentityServer.Admin.MongoDb
{
    internal class AdminService : IAdminService
    {
        private readonly MongoDatabase _db;
        private readonly StoreSettings _settings;
        private readonly ClientSerializer _clientSerializer;
        private static readonly ILog _log = LogProvider.For<AdminService>();

        public AdminService(MongoDatabase db, StoreSettings settings)
        {
            _db = db;
            _settings = settings;
            _clientSerializer = new ClientSerializer();
        }

        public void CreateDatabase(bool expireUsingIndex = true)
        {

            if (!_db.CollectionExists(_settings.ClientCollection))
            {
                var result = _db.CreateCollection(_settings.ClientCollection);
                _log.Debug(result.Response.ToString);
            }
            if (!_db.CollectionExists(_settings.ScopeCollection))
            {
                var result = _db.CreateCollection(_settings.ScopeCollection);
                _log.Debug(result.Response.ToString);
            }
            if (!_db.CollectionExists(_settings.ConsentCollection))
            {
                MongoCollection<BsonDocument> collection = _db.GetCollection(_settings.ConsentCollection);
                var result = collection.CreateIndex("subject");
                _log.Debug(result.Response.ToString);
                result = collection.CreateIndex("clientId", "subject");
                _log.Debug(result.Response.ToString);
            }

            var tokenCollections = new[]
            {
                _settings.AuthorizationCodeCollection,
                _settings.RefreshTokenCollection,
                _settings.TokenHandleCollection
            };

            foreach (string tokenCollection in tokenCollections)
            {
                var options = new IndexOptionsBuilder();
                var keys = new IndexKeysBuilder();
                keys.Ascending("_expires");
                if (expireUsingIndex)
                {
                    options.SetTimeToLive(TimeSpan.FromSeconds(1));
                }
                MongoCollection<BsonDocument> collection = _db.GetCollection(tokenCollection);
                
                var result = collection.CreateIndex("_clientId", "_subjectId");
                _log.Debug(result.Response.ToString);
                
                result = collection.CreateIndex("_subjectId");
                _log.Debug(result.Response.ToString);
                try
                {
                    result = collection.CreateIndex(keys, options);
                    _log.Debug(result.Response.ToString);
                } catch (MongoWriteConcernException)
                {
                    var cr = collection.DropIndex("_expires");
                    _log.Debug(cr.Response.ToString);
                    result = collection.CreateIndex(keys, options);
                    _log.Debug(result.Response.ToString);
                }
            }
        }

        public void Save(Scope scope)
        {
            BsonDocument doc = new ScopeSerializer().Serialize(scope);
            MongoCollection<BsonDocument> collection = _db.GetCollection(_settings.ScopeCollection);
            var result = collection.Save(doc);
            _log.Debug(result.Response.ToString);
        }

        public void Save(Client client)
        {
            BsonDocument doc = _clientSerializer.Serialize(client);
            MongoCollection<BsonDocument> collection = _db.GetCollection(_settings.ClientCollection);
            var result = collection.Save(doc);
            _log.Debug(result.Response.ToString);
        }

        public void RemoveDatabase()
        {
            _db.Drop();
        }

        public void DeleteClient(string clientId)
        {
            MongoCollection<BsonDocument> collection = _db.GetCollection(_settings.ClientCollection);
            collection.Remove(new QueryWrapper(new{_id = clientId}));
        }

        public void DeleteScope(string scopeName)
        {
            MongoCollection<BsonDocument> collection = _db.GetCollection(_settings.ScopeCollection);
            collection.Remove(new QueryWrapper(new { _id = scopeName}));
        }
    }
}