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
using IdentityServer3.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver;

namespace IdentityServer3.Admin.MongoDb
{
    class CleanupExpiredTokens : ICleanupExpiredTokens
    {
        private readonly IMongoDatabase _db;
        private readonly StoreSettings _settings;
        public CleanupExpiredTokens(IMongoDatabase db, StoreSettings settings)
        {
            _db = db;
            _settings = settings;
        }

        public async Task CleanupAuthorizationCodes(DateTime removeTokensBefore)
        {
            var collection = _db.GetCollection<BsonDocument>(_settings.AuthorizationCodeCollection);
            await collection.DeleteManyAsync(Builders<BsonDocument>.Filter.Lt("_expires", removeTokensBefore));
        }

        public async Task CleanupTokenHandles(DateTime removeTokensBefore)
        {
            var collection = _db.GetCollection<BsonDocument>(_settings.TokenHandleCollection);
            await collection.DeleteManyAsync(Builders<BsonDocument>.Filter.Lt("_expires", removeTokensBefore));
        }

        public async Task CleanupRefreshTokens(DateTime removeTokensBefore)
        {
            var collection = _db.GetCollection<BsonDocument>(_settings.RefreshTokenCollection);
            await collection.DeleteManyAsync(Builders<BsonDocument>.Filter.Lt("_expires", removeTokensBefore));
        }
    }
}
