﻿using System;
using System.Collections.Generic;

using ASC.Common;
using ASC.Core.Common.EF.Model;

using Microsoft.EntityFrameworkCore;

namespace ASC.Core.Common.EF.Context
{
    public class MSSqlDbContext : DbContext { }
    public class MySqlDbContext : DbContext { }
    public class PostgreSqlDbContext : DbContext { }
    public class DbContext : BaseDbContext
    {
        public DbSet<MobileAppInstall> MobileAppInstall { get; set; }
        public DbSet<DbipLocation> DbipLocation { get; set; }
        public DbSet<Regions> Regions { get; set; }

        public DbContext()
        {
        }

        public DbContext(DbContextOptions options) : base(options)
        {
        }
        protected override Dictionary<Provider, Func<BaseDbContext>> ProviderContext
        {
            get
            {
                return new Dictionary<Provider, Func<BaseDbContext>>()
                {
                    { Provider.MySql, () => new MySqlDbContext() } ,
                    { Provider.Postgre, () => new PostgreSqlDbContext() } ,
                    { Provider.MSSql, () => new MSSqlDbContext() } ,
                };
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ModelBuilderWrapper
                   .From(modelBuilder, Provider)
                   .AddMobileAppInstall()
                   .AddDbipLocation();
        }
    }

    public static class DbContextExtension
    {
        public static DIHelper AddDbContextService(this DIHelper services)
        {
            return services.AddDbContextManagerService<DbContext>();
        }
    }
}
