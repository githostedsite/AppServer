﻿using System;
using System.Collections.Generic;

using ASC.Common;
using ASC.Core.Common.EF.Model;

using Microsoft.EntityFrameworkCore;

namespace ASC.Core.Common.EF.Context
{
    public class MSSqlAccountLinkContext : AccountLinkContext { }
    public class MySqlAccountLinkContext : AccountLinkContext { }
    public class PostgreSqlAccountLinkContext : AccountLinkContext { }
    public class AccountLinkContext : BaseDbContext
    {
        public DbSet<AccountLinks> AccountLinks { get; set; }

        protected override Dictionary<Provider, Func<BaseDbContext>> ProviderContext
        {
            get
            {
                return new Dictionary<Provider, Func<BaseDbContext>>()
                {
                    { Provider.MySql, () => new MySqlAccountLinkContext() } ,
                    { Provider.Postgre, () => new PostgreSqlAccountLinkContext() } ,
                    { Provider.MSSql, () => new MSSqlAccountLinkContext() } ,
                };
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ModelBuilderWrapper
               .From(modelBuilder, Provider)
               .AddAccountLinks();
        }
    }

    public static class AccountLinkContextExtension
    {
        public static DIHelper AddAccountLinkContextService(this DIHelper services)
        {
            return services.AddDbContextManagerService<AccountLinkContext>();
        }
    }
}
