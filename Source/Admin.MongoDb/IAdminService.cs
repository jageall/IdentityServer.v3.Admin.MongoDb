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

using System.Threading.Tasks;
using IdentityServer3.Core.Models;

namespace IdentityServer3.Admin.MongoDb
{
    public interface IAdminService
    {
        Task CreateDatabase(bool expireUsingIndex = true);
        Task Save(Scope scope);
        Task Save(Client client);
        Task RemoveDatabase();
        Task DeleteClient(string clientId);
        Task DeleteScope(string scopeName);
    }
}