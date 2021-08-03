/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.Linq;

using ASC.Common;
using ASC.Files.Core.Security;
using ASC.Projects.Classes;

using Microsoft.Extensions.DependencyInjection;

namespace ASC.Projects.Engine
{
    [Scope]
    public class SecurityAdapterProvider : IFileSecurityProvider
    {
        private IServiceProvider ServiceProvider { get; set; }
        private EngineFactory EngineFactory { get; set; }
        public SecurityAdapterProvider(EngineFactory engineFactory, IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            EngineFactory = engineFactory;
        }
        public IFileSecurity GetFileSecurity(string data)
        {
            int id;
            return int.TryParse(data, out id) ? GetFileSecurity(id) : null;
        }

        public Dictionary<object, IFileSecurity> GetFileSecurity(Dictionary<string, string> data)
        {
            var projectIds = data.Select(r =>
            {
                int id;
                if (!int.TryParse(r.Value, out id))
                {
                    id = -1;
                }
                return id;
            }).ToList();

            return EngineFactory.GetProjectEngine().GetByID(projectIds, false).
                ToDictionary(
                    r =>
                    {
                        var folder = data.First(d => d.Value == r.ID.ToString());
                        if (!folder.Equals(default(KeyValuePair<string, string>)))
                        {
                            return (object)folder.Key;
                        }
                        return "";
                    },
                    r => (IFileSecurity)ServiceProvider.GetService<SecurityAdapter>().Init(r));
        }

        public IFileSecurity GetFileSecurity(int projectId)
        {
            return ServiceProvider.GetService<SecurityAdapter>().Init(projectId);
        }
    }
}