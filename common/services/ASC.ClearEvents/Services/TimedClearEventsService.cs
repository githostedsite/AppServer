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
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using ASC.Common;
using ASC.Common.Logging;
using ASC.Core.Common.EF;
using ASC.Core.Common.EF.Context;
using ASC.Core.Common.EF.Model;
using ASC.Core.Tenants;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ASC.ClearEvents.Services
{
    [Scope(Additional = typeof(MessagesRepositoryExtension))]
    public class TimedClearEventsService : IHostedService, IDisposable
    {
        private readonly ILog _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private Timer _timer = null!;

        public TimedClearEventsService(IOptionsMonitor<ILog> options, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = options.CurrentValue;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Info("Timer Clear Events Service running.");

            _timer = new Timer(DeleteOldEvents, null, TimeSpan.Zero, 
                TimeSpan.FromDays(1));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Info("Timed Clear Events Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose() => _timer?.Dispose();

        private void DeleteOldEvents(object state)
        {
            try
            {
                GetOldEvents(r => r.LoginEvents, "LoginHistoryLifeTime");
                GetOldEvents(r => r.AuditEvents, "AuditTrailLifeTime");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
        }

        private void GetOldEvents<T>(Expression<Func<Messages, DbSet<T>>> func, string settings) where T : MessageEvent
        {
            List<T> ids;
            var compile = func.Compile();
            do
            {
                using var scope = _serviceScopeFactory.CreateScope();
                using var ef = scope.ServiceProvider.GetService<DbContextManager<Messages>>().Get("messages");
                var table = compile.Invoke(ef);

                var ae = table
                    .Join(ef.Tenants, r => r.TenantId, r => r.Id, (audit, tenant) => audit)
                    .Select(r => new
                    {
                        r.Id,
                        r.Date,
                        r.TenantId,
                        ef = r
                    })
                    .Where(r => r.Date < DateTime.UtcNow.AddDays(-Convert.ToDouble(
                        ef.WebstudioSettings
                        .Where(a => a.TenantId == r.TenantId && a.Id == TenantAuditSettings.Guid)
                        .Select(r => JsonExtensions.JsonValue(nameof(r.Data).ToLower(), settings))
                        .FirstOrDefault() ?? TenantAuditSettings.MaxLifeTime.ToString())))
                    .Take(1000);

                ids = ae.Select(r => r.ef).ToList();

                if (!ids.Any()) return;

                table.RemoveRange(ids);
                ef.SaveChanges();

            } while (ids.Any());
        }
    }

    public class Messages : MessagesContext
    {
        public DbSet<AuditEvent> AuditEvents { get; }
        public DbSet<DbTenant> Tenants { get; }
        public DbSet<DbWebstudioSettings> WebstudioSettings { get; }
    }
    
    public class MessagesRepositoryExtension
    {
        public static void Register(DIHelper services) => services.TryAdd<DbContextManager<Messages>>();
    }
}