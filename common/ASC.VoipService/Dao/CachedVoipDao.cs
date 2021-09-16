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
using System.Globalization;
using System.Linq;

using ASC.Common;
using ASC.Common.Caching;
using ASC.Core;
using ASC.Core.Common;
using ASC.Core.Common.Configuration;
using ASC.Core.Common.EF;
using ASC.Core.Common.EF.Context;
using ASC.Core.Tenants;

namespace ASC.VoipService.Dao
{
    [Scope]
    public class VoipDaoCache
    {
        internal DistributedCache Cache { get; }

        public VoipDaoCache(DistributedCache cache)
        {
            Cache = cache;
        }

        public void ResetCache(string key)
        {
            Cache.Remove(key);
        }
    }

    [Scope]
    public class CachedVoipDao : VoipDao
    {
        private readonly DistributedCache cache;
        private static readonly TimeSpan timeout = TimeSpan.FromDays(1);

        private VoipDaoCache VoipDaoCache { get; }

        public CachedVoipDao(
            TenantManager tenantManager,
            DbContextManager<VoipDbContext> dbOptions,
            AuthContext authContext,
            TenantUtil tenantUtil,
            SecurityContext securityContext,
            BaseCommonLinkUtility baseCommonLinkUtility,
            ConsumerFactory consumerFactory,
            VoipDaoCache voipDaoCache)
            : base(tenantManager, dbOptions, authContext, tenantUtil, securityContext, baseCommonLinkUtility, consumerFactory)
        {
            cache = voipDaoCache.Cache;
            VoipDaoCache = voipDaoCache;
        }

        public override VoipPhone SaveOrUpdateNumber(VoipPhone phone)
        {
            var result = base.SaveOrUpdateNumber(phone);
            VoipDaoCache.ResetCache(GetCacheKey(TenantID));
            return result;
        }

        public override void DeleteNumber(string phoneId = "")
        {
            base.DeleteNumber(phoneId);
            VoipDaoCache.ResetCache(GetCacheKey(TenantID));
        }

        public override IEnumerable<VoipPhone> GetNumbers(params string[] ids)
        {
            var numbers = cache.Get<VoipPhoneList>(GetCacheKey(TenantID));
            if (numbers == null)
            {
                numbers = new VoipPhoneList(base.GetAllNumbers());
                cache.Insert(GetCacheKey(TenantID), numbers, DateTime.UtcNow.Add(timeout));
            }

            numbers.AddSettings(new VoipSettings(AuthContext, 
                TenantUtil, SecurityContext, BaseCommonLinkUtility));

            return ids.Any() ? numbers.Where(r => ids.Contains(r.Id) || ids.Contains(r.Number)).ToList() : numbers;
        }

        public static string GetCacheKey(int tenant)
        {
            return "voip" + tenant.ToString(CultureInfo.InvariantCulture);
        }
    }
}