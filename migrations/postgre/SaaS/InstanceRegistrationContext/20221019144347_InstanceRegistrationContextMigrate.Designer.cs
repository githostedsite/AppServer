// <auto-generated />
using System;
using ASC.Core.Common.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ASC.Migrations.PostgreSql.Migrations
{
    [DbContext(typeof(InstanceRegistrationContext))]
    [Migration("20221019144347_InstanceRegistrationContextMigrate")]
    partial class InstanceRegistrationContextMigrate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "6.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("ASC.Core.Common.Hosting.InstanceRegistration", b =>
                {
                    b.Property<string>("InstanceRegistrationId")
                        .HasColumnType("varchar(255)")
                        .HasColumnName("instance_registration_id")
                        .UseCollation("utf8_general_ci")
                        .HasAnnotation("MySql:CharSet", "utf8");

                    b.Property<bool>("IsActive")
                        .HasColumnType("tinyint(4)")
                        .HasColumnName("is_active");

                    b.Property<DateTime?>("LastUpdated")
                        .HasColumnType("datetime")
                        .HasColumnName("last_updated");

                    b.Property<string>("WorkerTypeName")
                        .IsRequired()
                        .HasColumnType("varchar(255)")
                        .HasColumnName("worker_type_name")
                        .UseCollation("utf8_general_ci")
                        .HasAnnotation("MySql:CharSet", "utf8");

                    b.HasKey("InstanceRegistrationId")
                        .HasName("PRIMARY");

                    b.HasIndex("WorkerTypeName")
                        .HasDatabaseName("worker_type_name");

                    b.ToTable("hosting_instance_registration", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
