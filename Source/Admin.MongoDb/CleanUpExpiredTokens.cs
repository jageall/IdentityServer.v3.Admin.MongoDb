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
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace IdentityServer.Core.MongoDb
{
    class CleanupExpiredTokens : ICleanupExpiredTokens
    {
        private readonly MongoDatabase _db;
        private readonly StoreSettings _settings;

        public CleanupExpiredTokens(MongoDatabase db, StoreSettings settings)
        {
            _db = db;
            _settings = settings;
        }

        public void CleanupAuthorizationCodes(DateTime removeTokensBefore)
        {
            var collection = _db.GetCollection(_settings.AuthorizationCodeCollection);
            collection.Remove(Query.LT("_expires", removeTokensBefore));
        }

        public void CleanupTokenHandles(DateTime removeTokensBefore)
        {
            var collection = _db.GetCollection(_settings.TokenHandleCollection);
            collection.Remove(Query.LT("_expires", removeTokensBefore));
        }

        public void CleanupRefreshTokens(DateTime removeTokensBefore)
        {
            var collection = _db.GetCollection(_settings.RefreshTokenCollection);
            collection.Remove(Query.LT("_expires", removeTokensBefore));
        }
    }
}
