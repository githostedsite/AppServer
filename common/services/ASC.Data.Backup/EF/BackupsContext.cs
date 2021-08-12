﻿using ASC.Common;
using ASC.Core.Common.EF;
using ASC.Core.Common.EF.Model;
using ASC.Data.Backup.EF.Model;

using Microsoft.EntityFrameworkCore;


namespace ASC.Data.Backup.EF.Context
{
    public class BackupsContext : BaseDbContext
    {
        public DbSet<BackupRecord> Backups { get; set; }
        public DbSet<BackupSchedule> Schedules { get; set; }

        public BackupsContext() { }
        public BackupsContext(DbContextOptions<BackupsContext> options)
            : base(options)
        {
        }
    }

    public static class BackupsContextExtension
    {
        public static DIHelper AddBackupsContext(this DIHelper services)
        {
            return services.AddDbContextManagerService<BackupsContext>();
        }
    }
}
