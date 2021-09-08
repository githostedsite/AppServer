/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using System.Linq;

using ASC.Common;
using ASC.Common.Caching;
using ASC.Core.Data;

namespace ASC.Core.Caching
{
    [Scope]
    class SubscriptionServiceCache
    {
        internal DistributedCache<SubscriptionsStore> CacheSubscriptionsStore { get; }

        public SubscriptionServiceCache(DistributedCache<SubscriptionsStore> cacheSubscriptionsStore)
        {
            CacheSubscriptionsStore = cacheSubscriptionsStore;
        }

        private SubscriptionsStore GetSubsciptionsStore(int tenant, string sourceId, string actionId)
        {
            return CacheSubscriptionsStore.Get(GetKey(tenant, sourceId, actionId));
        }

        public static string GetKey(int tenant, string sourceId, string actionId)
        {
            return string.Format("sub/{0}/{1}/{2}", tenant, sourceId, actionId);
        }
    }

    [Scope]
    class CachedSubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionService service;
        private readonly DistributedCache<SubscriptionsStore> CacheSubscriptionsStore;

        private TimeSpan CacheExpiration { get; set; }

        public CachedSubscriptionService(DbSubscriptionService service, 
            SubscriptionServiceCache subscriptionServiceCache,
            DistributedCache<SubscriptionsStore> cacheSubscriptionsStore)
        {
            this.service = service ?? throw new ArgumentNullException("service");
            CacheSubscriptionsStore = cacheSubscriptionsStore;
            CacheExpiration = TimeSpan.FromMinutes(5);
        }


        public IEnumerable<SubscriptionRecord> GetSubscriptions(int tenant, string sourceId, string actionId)
        {
            var store = GetSubsciptionsStore(tenant, sourceId, actionId);
            lock (store)
            {
                return store.GetSubscriptions();
            }
        }

        public IEnumerable<SubscriptionRecord> GetSubscriptions(int tenant, string sourceId, string actionId, string recipientId, string objectId)
        {
            var store = GetSubsciptionsStore(tenant, sourceId, actionId);
            lock (store)
            {
                return store.GetSubscriptions(recipientId, objectId);
            }
        }

        public string[] GetRecipients(int tenant, string sourceID, string actionID, string objectID)
        {
            return service.GetRecipients(tenant, sourceID, actionID, objectID);
        }

        public string[] GetSubscriptions(int tenant, string sourceId, string actionId, string recipientId, bool checkSubscribe)
        {
            return service.GetSubscriptions(tenant, sourceId, actionId, recipientId, checkSubscribe);
        }

        public SubscriptionRecord GetSubscription(int tenant, string sourceId, string actionId, string recipientId, string objectId)
        {
            var store = GetSubsciptionsStore(tenant, sourceId, actionId);
            lock (store)
            {
                return store.GetSubscription(recipientId, objectId);
            }
        }

        public void SaveSubscription(SubscriptionRecord s)
        {
            service.SaveSubscription(s);

            var store = GetSubsciptionsStore(s.Tenant, s.SourceId, s.ActionId);
            store.SaveSubscription(s);

            CacheSubscriptionsStore.Insert(SubscriptionServiceCache.GetKey(s.Tenant, s.SourceId, s.ActionId),
                store, CacheExpiration);
        }

        public void RemoveSubscriptions(int tenant, string sourceId, string actionId)
        {
            service.RemoveSubscriptions(tenant, sourceId, actionId);

            var store = GetSubsciptionsStore(tenant, sourceId, actionId);
            store.RemoveSubscriptions(string.Empty);

            CacheSubscriptionsStore.Insert(SubscriptionServiceCache.GetKey(tenant, sourceId, actionId),
                store, CacheExpiration);
        }

        public void RemoveSubscriptions(int tenant, string sourceId, string actionId, string objectId)
        {
            service.RemoveSubscriptions(tenant, sourceId, actionId, objectId);

            var store = GetSubsciptionsStore(tenant, sourceId, actionId);
            store.RemoveSubscriptions(objectId);

            CacheSubscriptionsStore.Insert(SubscriptionServiceCache.GetKey(tenant, sourceId, actionId),
                store, CacheExpiration);
        }

        public IEnumerable<SubscriptionMethod> GetSubscriptionMethods(int tenant, string sourceId, string actionId, string recipientId)
        {
            var store = GetSubsciptionsStore(tenant, sourceId, actionId);
            lock (store)
            {
                return store.GetSubscriptionMethods(recipientId);
            }
        }

        public void SetSubscriptionMethod(SubscriptionMethod m)
        {
            service.SetSubscriptionMethod(m);

            var store = GetSubsciptionsStore(m.Tenant, m.SourceId, m.ActionId);
            store.SetSubscriptionMethod(m);

            CacheSubscriptionsStore.Insert(SubscriptionServiceCache.GetKey(m.Tenant, m.SourceId, m.ActionId),
                store, CacheExpiration);
        }


        private SubscriptionsStore GetSubsciptionsStore(int tenant, string sourceId, string actionId)
        {
            var key = SubscriptionServiceCache.GetKey(tenant, sourceId, actionId);
            var store = CacheSubscriptionsStore.Get(key);
            if (store == null)
            {
                var records = service.GetSubscriptions(tenant, sourceId, actionId);
                var methods = service.GetSubscriptionMethods(tenant, sourceId, actionId, null);
                CacheSubscriptionsStore.Insert(key, store = new SubscriptionsStore(records, methods), DateTime.UtcNow.Add(CacheExpiration));
            }
            return store;
        }
        public bool IsUnsubscribe(int tenant, string sourceId, string actionId, string recipientId, string objectId)
        {
            return service.IsUnsubscribe(tenant, sourceId, actionId, recipientId, objectId);
        }
    }
}

namespace ASC.Core
{
    public partial class SubscriptionsStore : ICustomSer<SubscriptionsStore>
    {
        private IDictionary<string, List<SubscriptionRecord>> recordsByRec;
        private IDictionary<string, List<SubscriptionRecord>> recordsByObj;
        private IDictionary<string, List<SubscriptionMethod>> methodsByRec;

        public SubscriptionsStore(IEnumerable<SubscriptionRecord> records, IEnumerable<SubscriptionMethod> methods)
        {
            Records.AddRange(records);
            Methods.AddRange(methods);
            BuildSubscriptionsIndex(records);
            BuildMethodsIndex(methods);
        }

        public IEnumerable<SubscriptionRecord> GetSubscriptions()
        {
            return Records.ToList();
        }

        public IEnumerable<SubscriptionRecord> GetSubscriptions(string recipientId, string objectId)
        {
            return recipientId != null ?
                recordsByRec.ContainsKey(recipientId) ? recordsByRec[recipientId].ToList() : new List<SubscriptionRecord>() :
                recordsByObj.ContainsKey(objectId ?? string.Empty) ? recordsByObj[objectId ?? string.Empty].ToList() : new List<SubscriptionRecord>();
        }

        public SubscriptionRecord GetSubscription(string recipientId, string objectId)
        {
            return recordsByRec.ContainsKey(recipientId) ?
                recordsByRec[recipientId].Where(s => s.ObjectId == objectId).FirstOrDefault() :
                null;
        }

        public void SaveSubscription(SubscriptionRecord s)
        {
            var old = GetSubscription(s.RecipientId, s.ObjectId);
            if (old != null)
            {
                old.Subscribed = s.Subscribed;
            }
            else
            {
                Records.Add(s);
                BuildSubscriptionsIndex(Records);
            }
        }

        public void RemoveSubscriptions()
        {
            Records.Clear();
            BuildSubscriptionsIndex(Records);
        }

        public void RemoveSubscriptions(string objectId)
        {
            Records.RemoveAll(s => s.ObjectId == objectId);
            BuildSubscriptionsIndex(Records);
        }

        public IEnumerable<SubscriptionMethod> GetSubscriptionMethods(string recipientId)
        {
            return string.IsNullOrEmpty(recipientId) ?
                Methods.ToList() :
                methodsByRec.ContainsKey(recipientId) ? methodsByRec[recipientId].ToList() : new List<SubscriptionMethod>();
        }

        public void SetSubscriptionMethod(SubscriptionMethod m)
        {
            Methods.RemoveAll(r => r.Tenant == m.Tenant && r.SourceId == m.SourceId && r.ActionId == m.ActionId && r.RecipientId == m.RecipientId);
            if (m.Methods != null && 0 < m.Methods.Length)
            {
                Methods.Add(m);
            }
            BuildMethodsIndex(Methods);
        }

        private void BuildSubscriptionsIndex(IEnumerable<SubscriptionRecord> records)
        {
            recordsByRec = records.GroupBy(r => r.RecipientId).ToDictionary(g => g.Key, g => g.ToList());
            recordsByObj = records.GroupBy(r => r.ObjectId ?? string.Empty).ToDictionary(g => g.Key, g => g.ToList());
        }

        private void BuildMethodsIndex(IEnumerable<SubscriptionMethod> methods)
        {
            methodsByRec = methods.GroupBy(r => r.RecipientId).ToDictionary(g => g.Key, g => g.ToList());
        }

        public void CustomSer()
        {
            foreach (var method in Methods)
            {
                method.CustomSer();
            }
        }

        public void CustomDeSer()
        {
            foreach (var method in Methods)
            {
                method.CustomDeSer();
            }
        }
    }
}
