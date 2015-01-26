using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using MongoDB.Bson;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace IdentityServer.Core.MongoDb
{
    class TokenSerializer
    {
        private readonly IClientStore _clientStore;
        private static readonly ClaimSetSerializer ClaimsSetSerializer = new ClaimSetSerializer();
        private static readonly IDictionary<int, Func<BsonDocument, IClientStore, Task<Token>>>  Deserializers
            = new Dictionary<int, Func<BsonDocument, IClientStore, Task<Token>>>
            {
                {1, Version1}
            }; 
        public TokenSerializer(IClientStore clientStore)
        {
            _clientStore = clientStore;
        }

        public BsonDocument Serialize(string key, Token token)
        {
            var doc = new BsonDocument();
            doc["_id"] = key;
            doc["_expires"] = token.CreationTime.AddSeconds(token.Lifetime).ToBsonDateTime();
            doc["_clientId"] = token.ClientId;
            doc["_subjectId"] = token.SubjectId;
            Serialize(doc, token);
            return doc;
        }

        public void Serialize(BsonDocument doc, Token token)
        {
            doc["_version"] = 1;
            doc["audience"] = token.Audience;
            doc["claims"] = ClaimsSetSerializer.Serialize(token.Claims);
            doc["client"] = token.Client.ClientId;
            doc["creationTime"] = token.CreationTime.ToBsonDateTime();
            doc["issuer"] = token.Issuer;
            doc["lifetime"] = token.Lifetime;
            doc["type"] = token.Type;
            doc["version"] = token.Version;
        }

        public Task<Token> Deserialize(BsonDocument doc)
        {
            var version = doc["_version"].AsInt32;
            Func<BsonDocument, IClientStore, Task<Token>> deserialier;
            if (Deserializers.TryGetValue(version, out deserialier))
            {
                return deserialier(doc, _clientStore);
            }
            throw new InvalidOperationException("No deserializers available for token version " + version);
        }

        private static async Task<Token> Version1(BsonDocument doc, IClientStore clientStore)
        {
            var token = new Token();
            token.Audience = doc.GetValueOrDefault("audience", token.Audience);
            token.Claims = new List<Claim>(doc.GetNestedValueOrDefault("claims", ClaimsSetSerializer.Deserialize, new List<Claim>()));
            var clientId = doc.GetValueOrDefault("client", (string)null);
            var client = await clientStore.FindClientByIdAsync(clientId);
            if (client == null)
                throw new InvalidOperationException("Client not found when deserializing token. Client id: " + clientId);
            token.Client = client;
            token.CreationTime = doc.GetValueOrDefault("creationTime", token.CreationTime);
            token.Issuer = doc.GetValueOrDefault("issuer", token.Issuer);
            token.Lifetime = doc.GetValueOrDefault("lifetime", token.Lifetime);
            token.Type = doc.GetValueOrDefault("type", token.Type);
            token.Version = doc.GetValueOrDefault("version", token.Version);
            return token;
        }
    }
}