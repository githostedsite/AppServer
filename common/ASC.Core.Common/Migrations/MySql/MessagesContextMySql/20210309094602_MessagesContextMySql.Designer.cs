﻿// <auto-generated />
using System;
using ASC.Core.Common.EF.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ASC.Core.Common.Migrations.MySql.MessagesContextMySql
{
    [DbContext(typeof(MySqlMessagesContext))]
    [Migration("20210309094602_MessagesContextMySql")]
    partial class MessagesContextMySql
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.3");

            modelBuilder.Entity("ASC.Core.Common.EF.Model.AuditEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<int>("Action")
                        .HasColumnType("int")
                        .HasColumnName("action");

                    b.Property<string>("Browser")
                        .HasColumnType("varchar(200)")
                        .HasColumnName("browser")
                        .UseCollation("utf8_general_ci")
                        .HasCharSet("utf8");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime")
                        .HasColumnName("date");

                    b.Property<string>("Description")
                        .HasColumnType("varchar(20000)")
                        .HasColumnName("description")
                        .UseCollation("utf8_general_ci")
                        .HasCharSet("utf8");

                    b.Property<string>("Initiator")
                        .HasColumnType("varchar(200)")
                        .HasColumnName("initiator")
                        .UseCollation("utf8_general_ci")
                        .HasCharSet("utf8");

                    b.Property<string>("Ip")
                        .HasColumnType("varchar(50)")
                        .HasColumnName("ip")
                        .UseCollation("utf8_general_ci")
                        .HasCharSet("utf8");

                    b.Property<string>("Page")
                        .HasColumnType("varchar(300)")
                        .HasColumnName("page")
                        .UseCollation("utf8_general_ci")
                        .HasCharSet("utf8");

                    b.Property<string>("Platform")
                        .HasColumnType("varchar(200)")
                        .HasColumnName("platform")
                        .UseCollation("utf8_general_ci")
                        .HasCharSet("utf8");

                    b.Property<string>("Target")
                        .HasColumnType("text")
                        .HasColumnName("target")
                        .UseCollation("utf8_general_ci")
                        .HasCharSet("utf8");

                    b.Property<int>("TenantId")
                        .HasColumnType("int")
                        .HasColumnName("tenant_id");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("char(38)")
                        .HasColumnName("user_id")
                        .UseCollation("utf8_general_ci")
                        .HasCharSet("utf8");

                    b.HasKey("Id");

                    b.HasIndex("TenantId", "Date")
                        .HasDatabaseName("date");

                    b.ToTable("audit_events");
                });

            modelBuilder.Entity("ASC.Core.Common.EF.Model.DbTenant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<string>("Alias")
                        .IsRequired()
                        .HasColumnType("varchar(100)")
                        .HasColumnName("alias")
                        .UseCollation("utf8_general_ci")
                        .HasCharSet("utf8");

                    b.Property<bool>("Calls")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("calls")
                        .HasDefaultValueSql("true");

                    b.Property<DateTime>("CreationDateTime")
                        .HasColumnType("datetime")
                        .HasColumnName("creationdatetime");

                    b.Property<int?>("Industry")
                        .HasColumnType("int")
                        .HasColumnName("industry");

                    b.Property<string>("Language")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(10)")
                        .HasColumnName("language")
                        .HasDefaultValueSql("'en-US'")
                        .UseCollation("utf8_general_ci")
                        .HasCharSet("utf8");

                    b.Property<DateTime>("LastModified")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp")
                        .HasColumnName("last_modified")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("MappedDomain")
                        .HasColumnType("varchar(100)")
                        .HasColumnName("mappeddomain")
                        .UseCollation("utf8_general_ci")
                        .HasCharSet("utf8");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(255)")
                        .HasColumnName("name")
                        .UseCollation("utf8_general_ci")
                        .HasCharSet("utf8");

                    b.Property<string>("OwnerId")
                        .IsRequired()
                        .HasColumnType("varchar(38)")
                        .HasColumnName("owner_id")
                        .UseCollation("utf8_general_ci")
                        .HasCharSet("utf8");

                    b.Property<string>("PaymentId")
                        .HasColumnType("varchar(38)")
                        .HasColumnName("payment_id")
                        .UseCollation("utf8_general_ci")
                        .HasCharSet("utf8");

                    b.Property<bool>("Public")
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("public");

                    b.Property<string>("PublicVisibleProducts")
                        .HasColumnType("varchar(1024)")
                        .HasColumnName("publicvisibleproducts")
                        .UseCollation("utf8_general_ci")
                        .HasCharSet("utf8");

                    b.Property<bool>("Spam")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("spam")
                        .HasDefaultValueSql("true");

                    b.Property<int>("Status")
                        .HasColumnType("int")
                        .HasColumnName("status");

                    b.Property<DateTime?>("StatusChanged")
                        .HasColumnType("datetime")
                        .HasColumnName("statuschanged");

                    b.Property<string>("TimeZone")
                        .HasColumnType("varchar(50)")
                        .HasColumnName("timezone")
                        .UseCollation("utf8_general_ci")
                        .HasCharSet("utf8");

                    b.Property<string>("TrustedDomains")
                        .HasColumnType("varchar(1024)")
                        .HasColumnName("trusteddomains")
                        .UseCollation("utf8_general_ci")
                        .HasCharSet("utf8");

                    b.Property<int>("TrustedDomainsEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("trusteddomainsenabled")
                        .HasDefaultValueSql("'1'");

                    b.Property<int>("Version")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("version")
                        .HasDefaultValueSql("'2'");

                    b.Property<DateTime?>("Version_Changed")
                        .HasColumnType("datetime")
                        .HasColumnName("version_changed");

                    b.HasKey("Id");

                    b.HasIndex("LastModified")
                        .HasDatabaseName("last_modified");

                    b.HasIndex("MappedDomain")
                        .HasDatabaseName("mappeddomain");

                    b.HasIndex("Version")
                        .HasDatabaseName("version");

                    b.ToTable("tenants_tenants");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Alias = "localhost",
                            Calls = false,
                            CreationDateTime = new DateTime(2021, 3, 9, 9, 46, 1, 830, DateTimeKind.Utc).AddTicks(546),
                            LastModified = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Name = "Web Office",
                            OwnerId = "66faa6e4-f133-11ea-b126-00ffeec8b4ef",
                            Public = false,
                            Spam = false,
                            Status = 0,
                            TrustedDomainsEnabled = 0,
                            Version = 0
                        });
                });

            modelBuilder.Entity("ASC.Core.Common.EF.Model.LoginEvents", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<int>("Action")
                        .HasColumnType("int")
                        .HasColumnName("action");

                    b.Property<string>("Browser")
                        .HasColumnType("varchar(200)")
                        .HasColumnName("browser")
                        .UseCollation("utf8_general_ci")
                        .HasCharSet("utf8");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime")
                        .HasColumnName("date");

                    b.Property<string>("Description")
                        .HasColumnType("varchar(500)")
                        .HasColumnName("description")
                        .UseCollation("utf8_general_ci")
                        .HasCharSet("utf8");

                    b.Property<string>("Ip")
                        .HasColumnType("varchar(50)")
                        .HasColumnName("ip")
                        .UseCollation("utf8_general_ci")
                        .HasCharSet("utf8");

                    b.Property<string>("Login")
                        .HasColumnType("varchar(200)")
                        .HasColumnName("login")
                        .UseCollation("utf8_general_ci")
                        .HasCharSet("utf8");

                    b.Property<string>("Page")
                        .HasColumnType("varchar(300)")
                        .HasColumnName("page")
                        .UseCollation("utf8_general_ci")
                        .HasCharSet("utf8");

                    b.Property<string>("Platform")
                        .HasColumnType("varchar(200)")
                        .HasColumnName("platform")
                        .UseCollation("utf8_general_ci")
                        .HasCharSet("utf8");

                    b.Property<int>("TenantId")
                        .HasColumnType("int")
                        .HasColumnName("tenant_id");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("char(38)")
                        .HasColumnName("user_id")
                        .UseCollation("utf8_general_ci")
                        .HasCharSet("utf8");

                    b.HasKey("Id");

                    b.HasIndex("Date")
                        .HasDatabaseName("date");

                    b.HasIndex("TenantId", "UserId")
                        .HasDatabaseName("tenant_id");

                    b.ToTable("login_events");
                });
#pragma warning restore 612, 618
        }
    }
}
