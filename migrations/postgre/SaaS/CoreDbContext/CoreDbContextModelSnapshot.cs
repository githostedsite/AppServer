// <auto-generated />
using System;
using ASC.Core.Common.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ASC.Migrations.PostgreSql.Migrations.CoreDb
{
    [DbContext(typeof(CoreDbContext))]
    partial class CoreDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "7.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("ASC.Core.Common.EF.DbQuota", b =>
                {
                    b.Property<int>("Tenant")
                        .HasColumnType("integer")
                        .HasColumnName("tenant");

                    b.Property<string>("Description")
                        .HasColumnType("character varying")
                        .HasColumnName("description");

                    b.Property<string>("Features")
                        .HasColumnType("text")
                        .HasColumnName("features");

                    b.Property<string>("Name")
                        .HasColumnType("character varying")
                        .HasColumnName("name");

                    b.Property<decimal>("Price")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(10,2)")
                        .HasColumnName("price")
                        .HasDefaultValueSql("0.00");

                    b.Property<string>("ProductId")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("product_id")
                        .HasDefaultValueSql("NULL");

                    b.Property<bool>("Visible")
                        .HasColumnType("boolean")
                        .HasColumnName("visible");

                    b.HasKey("Tenant")
                        .HasName("tenants_quota_pkey");

                    b.ToTable("tenants_quota", "onlyoffice");

                    b.HasData(
                        new
                        {
                            Tenant = -1,
                            Features = "trial,audit,ldap,sso,whitelabel,thirdparty,restore,total_size:107374182400,file_size:100,manager:1",
                            Name = "trial",
                            Price = 0m,
                            Visible = false
                        },
                        new
                        {
                            Tenant = -2,
                            Features = "audit,ldap,sso,whitelabel,thirdparty,restore,contentsearch,total_size:107374182400,file_size:1024,manager:1",
                            Name = "admin",
                            Price = 30m,
                            ProductId = "1002",
                            Visible = true
                        },
                        new
                        {
                            Tenant = -3,
                            Features = "free,total_size:2147483648,manager:3,room:12",
                            Name = "startup",
                            Price = 0m,
                            Visible = false
                        },
                        new
                        {
                            Tenant = -4,
                            Features = "total_size:1073741824",
                            Name = "disk",
                            Price = 0m,
                            ProductId = "1004",
                            Visible = false
                        },
                        new
                        {
                            Tenant = -5,
                            Features = "manager:1",
                            Name = "admin1",
                            Price = 0m,
                            ProductId = "1005",
                            Visible = false
                        },
                        new
                        {
                            Tenant = -6,
                            Features = "audit,ldap,sso,whitelabel,thirdparty,restore,oauth,contentsearch,file_size:1024",
                            Name = "subscription",
                            Price = 0m,
                            ProductId = "1001",
                            Visible = false
                        },
                        new
                        {
                            Tenant = -7,
                            Features = "non-profit,audit,ldap,sso,thirdparty,restore,oauth,total_size:2147483648,file_size:1024,manager:20",
                            Name = "nonprofit",
                            Price = 0m,
                            Visible = false
                        });
                });

            modelBuilder.Entity("ASC.Core.Common.EF.DbQuotaRow", b =>
                {
                    b.Property<int>("Tenant")
                        .HasColumnType("integer")
                        .HasColumnName("tenant");

                    b.Property<string>("Path")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("path");

                    b.Property<long>("Counter")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("counter")
                        .HasDefaultValueSql("'0'");

                    b.Property<DateTime>("LastModified")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_modified")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Tag")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(1024)
                        .HasColumnType("character varying(1024)")
                        .HasColumnName("tag")
                        .HasDefaultValueSql("'0'");

                    b.Property<Guid>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(36)
                        .HasColumnType("uuid")
                        .HasColumnName("user_id")
                        .HasDefaultValueSql("NULL");

                    b.HasKey("Tenant", "Path")
                        .HasName("tenants_quotarow_pkey");

                    b.HasIndex("LastModified")
                        .HasDatabaseName("last_modified_tenants_quotarow");

                    b.ToTable("tenants_quotarow", "onlyoffice");
                });

            modelBuilder.Entity("ASC.Core.Common.EF.DbTariff", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Comment")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("comment")
                        .HasDefaultValueSql("NULL");

                    b.Property<DateTime>("CreateOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("create_on")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("CustomerId")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("customer_id")
                        .HasDefaultValueSql("NULL");

                    b.Property<DateTime>("Stamp")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("stamp");

                    b.Property<int>("Tenant")
                        .HasColumnType("integer")
                        .HasColumnName("tenant");

                    b.HasKey("Id");

                    b.HasIndex("Tenant")
                        .HasDatabaseName("tenant_tenants_tariff");

                    b.ToTable("tenants_tariff", "onlyoffice");
                });

            modelBuilder.Entity("ASC.Core.Common.EF.DbTariffRow", b =>
                {
                    b.Property<int>("Tenant")
                        .HasColumnType("int")
                        .HasColumnName("tenant");

                    b.Property<int>("TariffId")
                        .HasColumnType("int")
                        .HasColumnName("tariff_id");

                    b.Property<int>("Quota")
                        .HasColumnType("int")
                        .HasColumnName("quota");

                    b.Property<int>("Quantity")
                        .HasColumnType("int")
                        .HasColumnName("quantity");

                    b.HasKey("Tenant", "TariffId", "Quota")
                        .HasName("PRIMARY");

                    b.ToTable("tenants_tariffrow", "onlyoffice");
                });
#pragma warning restore 612, 618
        }
    }
}
