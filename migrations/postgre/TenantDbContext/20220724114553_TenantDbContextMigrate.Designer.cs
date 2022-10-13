// <auto-generated />
using System;
using ASC.Core.Common.EF.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ASC.Migrations.PostgreSql.Migrations
{
    [DbContext(typeof(TenantDbContext))]
    [Migration("20220724114553_TenantDbContextMigrate")]
    partial class TenantDbContextMigrate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "6.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("ASC.Core.Common.EF.Model.DbCoreSettings", b =>
                {
                    b.Property<int>("Tenant")
                        .HasColumnType("integer")
                        .HasColumnName("tenant");

                    b.Property<string>("Id")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("id");

                    b.Property<DateTime>("LastModified")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_modified")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<byte[]>("Value")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("value");

                    b.HasKey("Tenant", "Id")
                        .HasName("core_settings_pkey");

                    b.ToTable("core_settings", "onlyoffice");

                    b.HasData(
                        new
                        {
                            Tenant = -1,
                            Id = "CompanyWhiteLabelSettings",
                            LastModified = new DateTime(2022, 7, 8, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Value = new byte[] { 245, 71, 4, 138, 72, 101, 23, 21, 135, 217, 206, 188, 138, 73, 108, 96, 29, 150, 3, 31, 44, 28, 62, 145, 96, 53, 57, 66, 238, 118, 93, 172, 211, 22, 244, 181, 244, 40, 146, 67, 111, 196, 162, 27, 154, 109, 248, 255, 181, 17, 253, 127, 42, 65, 19, 90, 26, 206, 203, 145, 159, 159, 243, 105, 24, 71, 188, 165, 53, 85, 57, 37, 186, 251, 57, 96, 18, 162, 218, 80, 0, 101, 250, 100, 66, 97, 24, 51, 240, 215, 216, 169, 105, 100, 15, 253, 29, 83, 182, 236, 203, 53, 68, 251, 2, 150, 149, 148, 58, 136, 84, 37, 151, 82, 92, 227, 30, 52, 111, 40, 154, 155, 7, 126, 149, 100, 169, 87, 10, 129, 228, 138, 177, 101, 77, 67, 177, 216, 189, 201, 1, 213, 136, 216, 107, 198, 253, 221, 106, 255, 198, 17, 68, 14, 110, 90, 174, 182, 68, 222, 188, 77, 157, 19, 26, 68, 86, 97, 15, 81, 24, 171, 214, 114, 191, 175, 56, 56, 48, 52, 125, 82, 253, 113, 71, 41, 201, 5, 8, 118, 162, 191, 99, 196, 48, 198, 223, 79, 204, 174, 31, 97, 236, 20, 213, 218, 85, 34, 16, 74, 196, 209, 235, 14, 71, 209, 32, 131, 195, 84, 11, 66, 74, 19, 115, 255, 99, 69, 235, 210, 204, 15, 13, 4, 143, 127, 152, 125, 212, 91 }
                        },
                        new
                        {
                            Tenant = -1,
                            Id = "FullTextSearchSettings",
                            LastModified = new DateTime(2022, 7, 8, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Value = new byte[] { 8, 120, 207, 5, 153, 181, 23, 202, 162, 211, 218, 237, 157, 6, 76, 62, 220, 238, 175, 67, 31, 53, 166, 246, 66, 220, 173, 160, 72, 23, 227, 81, 50, 39, 187, 177, 222, 110, 43, 171, 235, 158, 16, 119, 178, 207, 49, 140, 72, 152, 20, 84, 94, 135, 117, 1, 246, 51, 251, 190, 148, 2, 44, 252, 221, 2, 91, 83, 149, 151, 58, 245, 16, 148, 52, 8, 187, 86, 150, 46, 227, 93, 163, 95, 47, 131, 116, 207, 95, 209, 38, 149, 53, 148, 73, 215, 206, 251, 194, 199, 189, 17, 42, 229, 135, 82, 23, 154, 162, 165, 158, 94, 23, 128, 30, 88, 12, 204, 96, 250, 236, 142, 189, 211, 214, 18, 196, 136, 102, 102, 217, 109, 108, 240, 96, 96, 94, 100, 201, 10, 31, 170, 128, 192 }
                        },
                        new
                        {
                            Tenant = -1,
                            Id = "SmtpSettings",
                            LastModified = new DateTime(2022, 7, 8, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Value = new byte[] { 240, 82, 224, 144, 161, 163, 117, 13, 173, 205, 78, 153, 97, 218, 4, 170, 81, 239, 1, 151, 226, 192, 98, 60, 241, 44, 88, 56, 191, 164, 10, 155, 72, 186, 239, 203, 227, 113, 88, 119, 49, 215, 227, 220, 158, 124, 96, 9, 116, 47, 158, 65, 93, 86, 219, 15, 10, 224, 142, 50, 248, 144, 75, 44, 68, 28, 198, 87, 198, 69, 67, 234, 238, 38, 32, 68, 162, 139, 67, 53, 220, 176, 240, 196, 233, 64, 29, 137, 31, 160, 99, 105, 249, 132, 202, 45, 71, 92, 134, 194, 55, 145, 121, 97, 197, 130, 119, 105, 131, 21, 133, 35, 10, 102, 172, 119, 135, 230, 251, 86, 253, 62, 55, 56, 146, 103, 164, 106 }
                        });
                });

            modelBuilder.Entity("ASC.Core.Common.EF.Model.DbTenant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Alias")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("alias");

                    b.Property<bool>("Calls")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasColumnName("calls")
                        .HasDefaultValueSql("true");

                    b.Property<DateTime>("CreationDateTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("creationdatetime");

                    b.Property<int>("Industry")
                        .HasColumnType("integer")
                        .HasColumnName("industry");

                    b.Property<string>("Language")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(10)
                        .HasColumnType("character(10)")
                        .HasColumnName("language")
                        .HasDefaultValueSql("'en-US'")
                        .IsFixedLength();

                    b.Property<DateTime>("LastModified")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_modified")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("MappedDomain")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("mappeddomain")
                        .HasDefaultValueSql("NULL");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("name");

                    b.Property<Guid?>("OwnerId")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(38)
                        .HasColumnType("uuid")
                        .HasColumnName("owner_id")
                        .HasDefaultValueSql("NULL");

                    b.Property<string>("PaymentId")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(38)
                        .HasColumnType("character varying(38)")
                        .HasColumnName("payment_id")
                        .HasDefaultValueSql("NULL");

                    b.Property<bool>("Spam")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasColumnName("spam")
                        .HasDefaultValueSql("true");

                    b.Property<int>("Status")
                        .HasColumnType("integer")
                        .HasColumnName("status");

                    b.Property<DateTime?>("StatusChanged")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("statuschanged");

                    b.Property<string>("TimeZone")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("timezone")
                        .HasDefaultValueSql("NULL");

                    b.Property<int>("TrustedDomainsEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("trusteddomainsenabled")
                        .HasDefaultValueSql("1");

                    b.Property<string>("TrustedDomainsRaw")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(1024)
                        .HasColumnType("character varying(1024)")
                        .HasColumnName("trusteddomains")
                        .HasDefaultValueSql("NULL");

                    b.Property<int>("Version")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("version")
                        .HasDefaultValueSql("2");

                    b.Property<DateTime?>("Version_Changed")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("version_changed");

                    b.HasKey("Id");

                    b.HasIndex("Alias")
                        .IsUnique()
                        .HasDatabaseName("alias");

                    b.HasIndex("LastModified")
                        .HasDatabaseName("last_modified_tenants_tenants");

                    b.HasIndex("MappedDomain")
                        .HasDatabaseName("mappeddomain");

                    b.HasIndex("Version")
                        .HasDatabaseName("version");

                    b.ToTable("tenants_tenants", "onlyoffice");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Alias = "localhost",
                            Calls = false,
                            CreationDateTime = new DateTime(2021, 3, 9, 17, 46, 59, 97, DateTimeKind.Utc).AddTicks(4317),
                            Industry = 0,
                            LastModified = new DateTime(2022, 7, 8, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Name = "Web Office",
                            OwnerId = new Guid("66faa6e4-f133-11ea-b126-00ffeec8b4ef"),
                            Spam = false,
                            Status = 0,
                            TrustedDomainsEnabled = 0,
                            Version = 0
                        });
                });

            modelBuilder.Entity("ASC.Core.Common.EF.Model.DbTenantForbiden", b =>
                {
                    b.Property<string>("Address")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("address");

                    b.HasKey("Address")
                        .HasName("tenants_forbiden_pkey");

                    b.ToTable("tenants_forbiden", "onlyoffice");

                    b.HasData(
                        new
                        {
                            Address = "controlpanel"
                        },
                        new
                        {
                            Address = "localhost"
                        });
                });

            modelBuilder.Entity("ASC.Core.Common.EF.Model.DbTenantVersion", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<int>("DefaultVersion")
                        .HasColumnType("integer")
                        .HasColumnName("default_version");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)")
                        .HasColumnName("url");

                    b.Property<string>("Version")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)")
                        .HasColumnName("version");

                    b.Property<bool>("Visible")
                        .HasColumnType("boolean")
                        .HasColumnName("visible");

                    b.HasKey("Id");

                    b.ToTable("tenants_version", "onlyoffice");
                });

            modelBuilder.Entity("ASC.Core.Common.EF.Model.TenantIpRestrictions", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Ip")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("ip");

                    b.Property<int>("Tenant")
                        .HasColumnType("integer")
                        .HasColumnName("tenant");

                    b.HasKey("Id");

                    b.HasIndex("Tenant")
                        .HasDatabaseName("tenant_tenants_iprestrictions");

                    b.ToTable("tenants_iprestrictions", "onlyoffice");
                });
#pragma warning restore 612, 618
        }
    }
}
