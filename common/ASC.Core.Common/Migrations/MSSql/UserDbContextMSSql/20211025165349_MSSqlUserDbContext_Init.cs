﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ASC.Core.Common.Migrations.MSSql.UserDbContextMSSql
{
    public partial class MSSqlUserDbContext_Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "core_acl",
                columns: table => new
                {
                    tenant = table.Column<int>(type: "int", nullable: false),
                    subject = table.Column<Guid>(type: "uniqueidentifier", maxLength: 38, nullable: false),
                    action = table.Column<Guid>(type: "uniqueidentifier", maxLength: 38, nullable: false),
                    @object = table.Column<string>(name: "object", type: "nvarchar(255)", maxLength: 255, nullable: false, defaultValue: "", collation: "LATIN1_GENERAL_100_CI_AS_SC_UTF8"),
                    acetype = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("core_acl_pkey", x => new { x.tenant, x.subject, x.action, x.@object });
                });

            migrationBuilder.CreateTable(
                name: "core_group",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 38, nullable: false),
                    tenant = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, collation: "LATIN1_GENERAL_100_CI_AS_SC_UTF8"),
                    categoryid = table.Column<Guid>(type: "uniqueidentifier", maxLength: 38, nullable: true),
                    parentid = table.Column<Guid>(type: "uniqueidentifier", maxLength: 38, nullable: true),
                    sid = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true, collation: "LATIN1_GENERAL_100_CI_AS_SC_UTF8"),
                    removed = table.Column<bool>(type: "bit", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core_group", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "core_subscription",
                columns: table => new
                {
                    tenant = table.Column<int>(type: "int", nullable: false),
                    source = table.Column<string>(type: "nvarchar(38)", maxLength: 38, nullable: false, collation: "LATIN1_GENERAL_100_CI_AS_SC_UTF8"),
                    action = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, collation: "LATIN1_GENERAL_100_CI_AS_SC_UTF8"),
                    recipient = table.Column<string>(type: "nvarchar(38)", maxLength: 38, nullable: false, collation: "LATIN1_GENERAL_100_CI_AS_SC_UTF8"),
                    @object = table.Column<string>(name: "object", type: "nvarchar(128)", maxLength: 128, nullable: false, collation: "LATIN1_GENERAL_100_CI_AS_SC_UTF8"),
                    unsubscribed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("core_subscription_pkey", x => new { x.tenant, x.source, x.action, x.recipient, x.@object });
                });

            migrationBuilder.CreateTable(
                name: "core_subscriptionmethod",
                columns: table => new
                {
                    tenant = table.Column<int>(type: "int", nullable: false),
                    source = table.Column<string>(type: "nvarchar(38)", maxLength: 38, nullable: false, collation: "LATIN1_GENERAL_100_CI_AS_SC_UTF8"),
                    action = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, collation: "LATIN1_GENERAL_100_CI_AS_SC_UTF8"),
                    recipient = table.Column<string>(type: "nvarchar(38)", maxLength: 38, nullable: false, collation: "LATIN1_GENERAL_100_CI_AS_SC_UTF8"),
                    sender = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false, collation: "LATIN1_GENERAL_100_CI_AS_SC_UTF8")
                },
                constraints: table =>
                {
                    table.PrimaryKey("core_subscriptionmethod_pkey", x => new { x.tenant, x.source, x.action, x.recipient });
                });

            migrationBuilder.CreateTable(
                name: "core_user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 38, nullable: false),
                    tenant = table.Column<int>(type: "int", nullable: false),
                    username = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, collation: "LATIN1_GENERAL_100_CI_AS_SC_UTF8"),
                    firstname = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, collation: "LATIN1_GENERAL_100_CI_AS_SC_UTF8"),
                    lastname = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, collation: "LATIN1_GENERAL_100_CI_AS_SC_UTF8"),
                    sex = table.Column<bool>(type: "bit", nullable: true),
                    bithdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false, defaultValueSql: "1"),
                    activation_status = table.Column<int>(type: "int", nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true, collation: "LATIN1_GENERAL_100_CI_AS_SC_UTF8"),
                    workfromdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    terminateddate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    title = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, collation: "LATIN1_GENERAL_100_CI_AS_SC_UTF8"),
                    culture = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, collation: "LATIN1_GENERAL_100_CI_AS_SC_UTF8"),
                    contacts = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true, collation: "LATIN1_GENERAL_100_CI_AS_SC_UTF8"),
                    phone = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true, collation: "LATIN1_GENERAL_100_CI_AS_SC_UTF8"),
                    phone_activation = table.Column<int>(type: "int", nullable: false),
                    location = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true, collation: "LATIN1_GENERAL_100_CI_AS_SC_UTF8"),
                    notes = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true, collation: "LATIN1_GENERAL_100_CI_AS_SC_UTF8"),
                    sid = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true, collation: "LATIN1_GENERAL_100_CI_AS_SC_UTF8"),
                    sso_name_id = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true, collation: "LATIN1_GENERAL_100_CI_AS_SC_UTF8"),
                    sso_session_id = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true, collation: "LATIN1_GENERAL_100_CI_AS_SC_UTF8"),
                    removed = table.Column<bool>(type: "bit", nullable: false),
                    create_on = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "core_userphoto",
                columns: table => new
                {
                    userid = table.Column<Guid>(type: "uniqueidentifier", maxLength: 38, nullable: false),
                    tenant = table.Column<int>(type: "int", nullable: false),
                    photo = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("core_userphoto_pkey", x => x.userid);
                });

            migrationBuilder.CreateTable(
                name: "core_usergroup",
                columns: table => new
                {
                    tenant = table.Column<int>(type: "int", nullable: false),
                    userid = table.Column<Guid>(type: "uniqueidentifier", maxLength: 38, nullable: false),
                    groupid = table.Column<Guid>(type: "uniqueidentifier", maxLength: 38, nullable: false),
                    ref_type = table.Column<int>(type: "int", nullable: false),
                    removed = table.Column<bool>(type: "bit", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("core_usergroup_pkey", x => new { x.tenant, x.userid, x.groupid, x.ref_type });
                    table.ForeignKey(
                        name: "FK_core_usergroup_core_user_userid",
                        column: x => x.userid,
                        principalTable: "core_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "core_usersecurity",
                columns: table => new
                {
                    userid = table.Column<Guid>(type: "uniqueidentifier", maxLength: 38, nullable: false),
                    tenant = table.Column<int>(type: "int", nullable: false),
                    pwdhash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true, collation: "LATIN1_GENERAL_100_CI_AS_SC_UTF8"),
                    pwdhashsha512 = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true, collation: "LATIN1_GENERAL_100_CI_AS_SC_UTF8"),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("core_usersecurity_pkey", x => x.userid);
                    table.ForeignKey(
                        name: "FK_core_usersecurity_core_user_userid",
                        column: x => x.userid,
                        principalTable: "core_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "core_acl",
                columns: new[] { "action", "object", "subject", "tenant", "acetype" },
                values: new object[,]
                {
                    { new Guid("ef5e6790-f346-4b6e-b662-722bc28cb0db"), "", new Guid("5d5b7260-f7f7-49f1-a1c9-95fbb6a12604"), -1, 0 },
                    { new Guid("00e7dfc5-ac49-4fd3-a1d6-98d84e877ac4"), "", new Guid("bba32183-a14d-48ed-9d39-c6b4d8925fbf"), -1, 0 },
                    { new Guid("14be970f-7af5-4590-8e81-ea32b5f7866d"), "", new Guid("bba32183-a14d-48ed-9d39-c6b4d8925fbf"), -1, 0 },
                    { new Guid("18ecc94d-6afa-4994-8406-aee9dff12ce2"), "", new Guid("bba32183-a14d-48ed-9d39-c6b4d8925fbf"), -1, 0 },
                    { new Guid("298530eb-435e-4dc6-a776-9abcd95c70e9"), "", new Guid("bba32183-a14d-48ed-9d39-c6b4d8925fbf"), -1, 0 },
                    { new Guid("430eaf70-1886-483c-a746-1a18e3e6bb63"), "", new Guid("bba32183-a14d-48ed-9d39-c6b4d8925fbf"), -1, 0 },
                    { new Guid("557d6503-633b-4490-a14c-6473147ce2b3"), "", new Guid("bba32183-a14d-48ed-9d39-c6b4d8925fbf"), -1, 0 },
                    { new Guid("724cbb75-d1c9-451e-bae0-4de0db96b1f7"), "", new Guid("bba32183-a14d-48ed-9d39-c6b4d8925fbf"), -1, 0 },
                    { new Guid("7cb5c0d1-d254-433f-abe3-ff23373ec631"), "", new Guid("bba32183-a14d-48ed-9d39-c6b4d8925fbf"), -1, 0 },
                    { new Guid("91b29dcd-9430-4403-b17a-27d09189be88"), "", new Guid("bba32183-a14d-48ed-9d39-c6b4d8925fbf"), -1, 0 },
                    { new Guid("a18480a4-6d18-4c71-84fa-789888791f45"), "", new Guid("bba32183-a14d-48ed-9d39-c6b4d8925fbf"), -1, 0 },
                    { new Guid("b630d29b-1844-4bda-bbbe-cf5542df3559"), "", new Guid("bba32183-a14d-48ed-9d39-c6b4d8925fbf"), -1, 0 },
                    { new Guid("c62a9e8d-b24c-4513-90aa-7ff0f8ba38eb"), "", new Guid("bba32183-a14d-48ed-9d39-c6b4d8925fbf"), -1, 0 },
                    { new Guid("d7cdb020-288b-41e5-a857-597347618533"), "", new Guid("bba32183-a14d-48ed-9d39-c6b4d8925fbf"), -1, 0 },
                    { new Guid("662f3db7-9bc8-42cf-84da-2765f563e9b0"), "", new Guid("bba32183-a14d-48ed-9d39-c6b4d8925fbf"), -1, 0 },
                    { new Guid("f11e8f3f-46e6-4e55-90e3-09c22ec565bd"), "", new Guid("ba74ca02-873f-43dc-8470-8620c156bc67"), -1, 0 },
                    { new Guid("e0759a42-47f0-4763-a26a-d5aa665bec35"), "", new Guid("712d9ec3-5d2b-4b13-824f-71f00191dcca"), -1, 0 },
                    { new Guid("6f05c382-8bca-4469-9424-c807a98c40d7"), "", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("77777777-32ae-425f-99b5-83176061d1ae"), "ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|1e04460243b54d7982f3fd6208a11960", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("77777777-32ae-425f-99b5-83176061d1ae"), "ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|6743007c6f954d208c88a8601ce5e76d", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("77777777-32ae-425f-99b5-83176061d1ae"), "ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|e67be73df9ae4ce18fec1880cb518cb4", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("77777777-32ae-425f-99b5-83176061d1ae"), "ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|ea942538e68e49079394035336ee0ba8", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("77777777-32ae-425f-99b5-83176061d1ae"), "ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|32d24cb57ece46069c9419216ba42086", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("77777777-32ae-425f-99b5-83176061d1ae"), "ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|bf88953e3c434850a3fbb1e43ad53a3e", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("77777777-32ae-425f-99b5-83176061d1ae"), "ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|2a9230378b2d487b9a225ac0918acf3f", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("77777777-32ae-425f-99b5-83176061d1ae"), "ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|28b10049dd204f54b986873bc14ccfc7", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("77777777-32ae-425f-99b5-83176061d1ae"), "ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|3cfd481b46f24a4ab55cb8c0c9def02c", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("77777777-32ae-425f-99b5-83176061d1ae"), "ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|6a598c7491ae437da5f4ad339bd11bb2", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("77777777-32ae-425f-99b5-83176061d1ae"), "ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|742cf945cbbc4a5782d61600a12cf8ca", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("77777777-32ae-425f-99b5-83176061d1ae"), "ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|853b6eb973ee438d9b098ffeedf36234", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("77777777-32ae-425f-99b5-83176061d1ae"), "ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|46cfa73af32046cf8d5bcd82e1d67f26", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("0d68b142-e20a-446e-a832-0d6b0b65a164"), "", new Guid("bba32183-a14d-48ed-9d39-c6b4d8925fbf"), -1, 0 },
                    { new Guid("f11e88d7-f185-4372-927c-d88008d2c483"), "", new Guid("ba74ca02-873f-43dc-8470-8620c156bc67"), -1, 0 },
                    { new Guid("77777777-32ae-425f-99b5-83176061d1ae"), "ASC.Web.Core.WebItemSecurity+WebItemSecurityObject|f4d98afdd336433287783c6945c81ea0", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("e0759a42-47f0-4763-a26a-d5aa665bec35"), "", new Guid("ba74ca02-873f-43dc-8470-8620c156bc67"), -1, 0 },
                    { new Guid("e37239bd-c5b5-4f1e-a9f8-3ceeac209615"), "", new Guid("ba74ca02-873f-43dc-8470-8620c156bc67"), -1, 0 },
                    { new Guid("f11e8f3f-46e6-4e55-90e3-09c22ec565bd"), "", new Guid("5d5b7260-f7f7-49f1-a1c9-95fbb6a12604"), -1, 0 },
                    { new Guid("088d5940-a80f-4403-9741-d610718ce95c"), "", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("08d66144-e1c9-4065-9aa1-aa4bba0a7bc8"), "", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("0d1f72a8-63da-47ea-ae42-0900e4ac72a9"), "", new Guid("abef62db-11a8-4673-9d32-ef1d8af19dc0"), -1, 0 },
                    { new Guid("13e30b51-5b4d-40a5-8575-cb561899eeb1"), "", new Guid("abef62db-11a8-4673-9d32-ef1d8af19dc0"), -1, 0 },
                    { new Guid("19f658ae-722b-4cd8-8236-3ad150801d96"), "", new Guid("abef62db-11a8-4673-9d32-ef1d8af19dc0"), -1, 0 }
                });

            migrationBuilder.InsertData(
                table: "core_acl",
                columns: new[] { "action", "object", "subject", "tenant", "acetype" },
                values: new object[,]
                {
                    { new Guid("2c6552b3-b2e0-4a00-b8fd-13c161e337b1"), "", new Guid("abef62db-11a8-4673-9d32-ef1d8af19dc0"), -1, 0 },
                    { new Guid("388c29d3-c662-4a61-bf47-fc2f7094224a"), "", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("40bf31f4-3132-4e76-8d5c-9828a89501a3"), "", new Guid("abef62db-11a8-4673-9d32-ef1d8af19dc0"), -1, 0 },
                    { new Guid("49ae8915-2b30-4348-ab74-b152279364fb"), "", new Guid("abef62db-11a8-4673-9d32-ef1d8af19dc0"), -1, 0 },
                    { new Guid("63e9f35f-6bb5-4fb1-afaa-e4c2f4dec9bd"), "", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("9018c001-24c2-44bf-a1db-d1121a570e74"), "", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("948ad738-434b-4a88-8e38-7569d332910a"), "", new Guid("abef62db-11a8-4673-9d32-ef1d8af19dc0"), -1, 0 },
                    { new Guid("9d75a568-52aa-49d8-ad43-473756cd8903"), "", new Guid("abef62db-11a8-4673-9d32-ef1d8af19dc0"), -1, 0 },
                    { new Guid("08d75c97-cf3f-494b-90d1-751c941fe2dd"), "", new Guid("abef62db-11a8-4673-9d32-ef1d8af19dc0"), -1, 0 },
                    { new Guid("c426c349-9ad4-47cd-9b8f-99fc30675951"), "", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("a362fe79-684e-4d43-a599-65bc1f4e167f"), "", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("9018c001-24c2-44bf-a1db-d1121a570e74"), "", new Guid("ba74ca02-873f-43dc-8470-8620c156bc67"), -1, 0 },
                    { new Guid("63e9f35f-6bb5-4fb1-afaa-e4c2f4dec9bd"), "", new Guid("ba74ca02-873f-43dc-8470-8620c156bc67"), -1, 0 },
                    { new Guid("49ae8915-2b30-4348-ab74-b152279364fb"), "", new Guid("ba74ca02-873f-43dc-8470-8620c156bc67"), -1, 0 },
                    { new Guid("13e30b51-5b4d-40a5-8575-cb561899eeb1"), "", new Guid("ba74ca02-873f-43dc-8470-8620c156bc67"), -1, 0 },
                    { new Guid("fcac42b8-9386-48eb-a938-d19b3c576912"), "", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("d1f3b53d-d9e2-4259-80e7-d24380978395"), "", new Guid("ba74ca02-873f-43dc-8470-8620c156bc67"), -1, 0 },
                    { new Guid("e37239bd-c5b5-4f1e-a9f8-3ceeac209615"), "", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("e0759a42-47f0-4763-a26a-d5aa665bec35"), "", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("d852b66f-6719-45e1-8657-18f0bb791690"), "", new Guid("abef62db-11a8-4673-9d32-ef1d8af19dc0"), -1, 0 },
                    { new Guid("d49f4e30-da10-4b39-bc6d-b41ef6e039d3"), "", new Guid("abef62db-11a8-4673-9d32-ef1d8af19dc0"), -1, 0 },
                    { new Guid("d1f3b53d-d9e2-4259-80e7-d24380978395"), "", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("d11ebcb9-0e6e-45e6-a6d0-99c41d687598"), "", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 },
                    { new Guid("fbc37705-a04c-40ad-a68c-ce2f0423f397"), "", new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), -1, 0 }
                });

            migrationBuilder.InsertData(
                table: "core_subscription",
                columns: new[] { "action", "object", "recipient", "source", "tenant", "unsubscribed" },
                values: new object[,]
                {
                    { "send_whats_new", "", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "asc.web.studio", -1, false },
                    { "sharedocument", "", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "6fe286a4-479e-4c25-a8d9-0156e332b0c0", -1, false },
                    { "new post", "", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "6a598c74-91ae-437d-a5f4-ad339bd11bb2", -1, false },
                    { "new topic in forum", "", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "853b6eb9-73ee-438d-9b09-8ffeedf36234", -1, false },
                    { "new photo uploaded", "", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "9d51954f-db9b-4aed-94e3-ed70b914e10", -1, false },
                    { "new bookmark created", "", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "28b10049-dd20-4f54-b986-873bc14ccfc7", -1, false },
                    { "new wiki page", "", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "742cf945-cbbc-4a57-82d6-1600a12cf8ca", -1, false },
                    { "BirthdayReminderd", "", "abef62db-11a8-4673-9d32-ef1d8af19dc0", "37620ae5-c40b-45ce-855a-39dd7d76a1fa", -1, false },
                    { "new feed", "", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "6504977c-75af-4691-9099-084d3ddeea04", -1, false },
                    { "SetAccess", "", "abef62db-11a8-4673-9d32-ef1d8af19dc0", "13ff36fb-0272-4887-b416-74f52b0d0b02", -1, false },
                    { "calendar_sharing", "", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "40650da3-f7c1-424c-8c89-b9c115472e08", -1, false },
                    { "event_alert", "", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "40650da3-f7c1-424c-8c89-b9c115472e08", -1, false },
                    { "admin_notify", "", "cd84e66b-b803-40fc-99f9-b2969a54a1de", "asc.web.studio", -1, false },
                    { "ResponsibleForTask", "", "abef62db-11a8-4673-9d32-ef1d8af19dc0", "13ff36fb-0272-4887-b416-74f52b0d0b02", -1, false },
                    { "AddRelationshipEvent", "", "abef62db-11a8-4673-9d32-ef1d8af19dc0", "13ff36fb-0272-4887-b416-74f52b0d0b02", -1, false },
                    { "ExportCompleted", "", "abef62db-11a8-4673-9d32-ef1d8af19dc0", "13ff36fb-0272-4887-b416-74f52b0d0b02", -1, false },
                    { "CreateNewContact", "", "abef62db-11a8-4673-9d32-ef1d8af19dc0", "13ff36fb-0272-4887-b416-74f52b0d0b02", -1, false },
                    { "ResponsibleForOpportunity", "", "abef62db-11a8-4673-9d32-ef1d8af19dc0", "13ff36fb-0272-4887-b416-74f52b0d0b02", -1, false }
                });

            migrationBuilder.InsertData(
                table: "core_subscription",
                columns: new[] { "action", "object", "recipient", "source", "tenant", "unsubscribed" },
                values: new object[,]
                {
                    { "periodic_notify", "", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "asc.web.studio", -1, false },
                    { "sharefolder", "", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "6fe286a4-479e-4c25-a8d9-0156e332b0c0", -1, false }
                });

            migrationBuilder.InsertData(
                table: "core_subscriptionmethod",
                columns: new[] { "action", "recipient", "source", "tenant", "sender" },
                values: new object[,]
                {
                    { "AddRelationshipEvent", "abef62db-11a8-4673-9d32-ef1d8af19dc0", "13ff36fb-0272-4887-b416-74f52b0d0b02", -1, "email.sender|messanger.sender" },
                    { "periodic_notify", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "asc.web.studio", -1, "email.sender" },
                    { "ResponsibleForOpportunity", "abef62db-11a8-4673-9d32-ef1d8af19dc0", "13ff36fb-0272-4887-b416-74f52b0d0b02", -1, "email.sender|messanger.sender" },
                    { "CreateNewContact", "abef62db-11a8-4673-9d32-ef1d8af19dc0", "13ff36fb-0272-4887-b416-74f52b0d0b02", -1, "email.sender" },
                    { "ExportCompleted", "abef62db-11a8-4673-9d32-ef1d8af19dc0", "13ff36fb-0272-4887-b416-74f52b0d0b02", -1, "email.sender|messanger.sender" },
                    { "ResponsibleForTask", "abef62db-11a8-4673-9d32-ef1d8af19dc0", "13ff36fb-0272-4887-b416-74f52b0d0b02", -1, "email.sender|messanger.sender" },
                    { "updatedocument", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "6fe286a4-479e-4c25-a8d9-0156e332b0c0", -1, "email.sender|messanger.sender" },
                    { "admin_notify", "cd84e66b-b803-40fc-99f9-b2969a54a1de", "asc.web.studio", -1, "email.sender" },
                    { "send_whats_new", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "asc.web.studio", -1, "email.sender" },
                    { "new feed", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "6504977c-75af-4691-9099-084d3ddeea04", -1, "email.sender|messanger.sender" },
                    { "new post", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "6a598c74-91ae-437d-a5f4-ad339bd11bb2", -1, "email.sender|messanger.sender" },
                    { "new topic in forum", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "853b6eb9-73ee-438d-9b09-8ffeedf36234", -1, "email.sender|messanger.sender" },
                    { "new photo uploaded", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "9d51954f-db9b-4aed-94e3-ed70b914e101", -1, "email.sender|messanger.sender" },
                    { "new bookmark created", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "28b10049-dd20-4f54-b986-873bc14ccfc7", -1, "email.sender|messanger.sender" },
                    { "new wiki page", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "742cf945-cbbc-4a57-82d6-1600a12cf8ca", -1, "email.sender|messanger.sender" },
                    { "BirthdayReminder", "abef62db-11a8-4673-9d32-ef1d8af19dc0", "37620ae5-c40b-45ce-855a-39dd7d76a1fa", -1, "email.sender|messanger.sender" },
                    { "sharedocument", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "6fe286a4-479e-4c25-a8d9-0156e332b0c0", -1, "email.sender|messanger.sender" },
                    { "sharefolder", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "6fe286a4-479e-4c25-a8d9-0156e332b0c0", -1, "email.sender|messanger.sender" },
                    { "invitetoproject", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "6045b68c-2c2e-42db-9e53-c272e814c4ad", -1, "email.sender|messanger.sender" },
                    { "milestonedeadline", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "6045b68c-2c2e-42db-9e53-c272e814c4ad", -1, "email.sender|messanger.sender" },
                    { "newcommentformessage", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "6045b68c-2c2e-42db-9e53-c272e814c4ad", -1, "email.sender|messanger.sender" },
                    { "newcommentformilestone", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "6045b68c-2c2e-42db-9e53-c272e814c4ad", -1, "email.sender|messanger.sender" },
                    { "newcommentfortask", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "6045b68c-2c2e-42db-9e53-c272e814c4ad", -1, "email.sender|messanger.sender" },
                    { "projectcreaterequest", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "6045b68c-2c2e-42db-9e53-c272e814c4ad", -1, "email.sender|messanger.sender" },
                    { "projecteditrequest", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "6045b68c-2c2e-42db-9e53-c272e814c4ad", -1, "email.sender|messanger.sender" },
                    { "removefromproject", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "6045b68c-2c2e-42db-9e53-c272e814c4ad", -1, "email.sender|messanger.sender" },
                    { "responsibleforproject", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "6045b68c-2c2e-42db-9e53-c272e814c4ad", -1, "email.sender|messanger.sender" },
                    { "responsiblefortask", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "6045b68c-2c2e-42db-9e53-c272e814c4ad", -1, "email.sender|messanger.sender" },
                    { "taskclosed", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "6045b68c-2c2e-42db-9e53-c272e814c4ad", -1, "email.sender|messanger.sender" },
                    { "calendar_sharing", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "40650da3-f7c1-424c-8c89-b9c115472e08", -1, "email.sender|messanger.sender" },
                    { "event_alert", "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", "40650da3-f7c1-424c-8c89-b9c115472e08", -1, "email.sender|messanger.sender" },
                    { "SetAccess", "abef62db-11a8-4673-9d32-ef1d8af19dc0", "13ff36fb-0272-4887-b416-74f52b0d0b02", -1, "email.sender|messanger.sender" }
                });

            migrationBuilder.InsertData(
                table: "core_user",
                columns: new[] { "id", "activation_status", "bithdate", "contacts", "culture", "email", "firstname", "last_modified", "lastname", "location", "notes", "phone", "phone_activation", "removed", "sex", "sid", "sso_name_id", "sso_session_id", "status", "tenant", "terminateddate", "title", "username", "workfromdate" },
                values: new object[] { new Guid("66faa6e4-f133-11ea-b126-00ffeec8b4ef"), 0, null, null, null, "", "Administrator", new DateTime(2021, 10, 25, 16, 53, 48, 919, DateTimeKind.Utc).AddTicks(9368), "", null, null, null, 0, false, null, null, null, null, 1, 1, null, null, "administrator", new DateTime(2021, 10, 25, 16, 53, 48, 919, DateTimeKind.Utc).AddTicks(8929) });

            migrationBuilder.InsertData(
                table: "core_usergroup",
                columns: new[] { "groupid", "ref_type", "tenant", "userid", "removed" },
                values: new object[] { new Guid("cd84e66b-b803-40fc-99f9-b2969a54a1de"), 0, 1, new Guid("66faa6e4-f133-11ea-b126-00ffeec8b4ef"), false });

            migrationBuilder.InsertData(
                table: "core_usersecurity",
                columns: new[] { "userid", "LastModified", "pwdhash", "pwdhashsha512", "tenant" },
                values: new object[] { new Guid("66faa6e4-f133-11ea-b126-00ffeec8b4ef"), new DateTime(2021, 10, 25, 16, 53, 48, 927, DateTimeKind.Utc).AddTicks(7256), "jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=", null, 1 });

            migrationBuilder.CreateIndex(
                name: "last_modified",
                table: "core_group",
                column: "last_modified");

            migrationBuilder.CreateIndex(
                name: "parentid",
                table: "core_group",
                columns: new[] { "tenant", "parentid" });

            migrationBuilder.CreateIndex(
                name: "email",
                table: "core_user",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "last_modified_core_user",
                table: "core_user",
                column: "last_modified");

            migrationBuilder.CreateIndex(
                name: "username",
                table: "core_user",
                columns: new[] { "username", "tenant" });

            migrationBuilder.CreateIndex(
                name: "IX_core_usergroup_userid",
                table: "core_usergroup",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "last_modified_core_usergroup",
                table: "core_usergroup",
                column: "last_modified");

            migrationBuilder.CreateIndex(
                name: "tenant_core_userphoto",
                table: "core_userphoto",
                column: "tenant");

            migrationBuilder.CreateIndex(
                name: "pwdhash",
                table: "core_usersecurity",
                column: "pwdhash");

            migrationBuilder.CreateIndex(
                name: "tenant_core_usersecurity",
                table: "core_usersecurity",
                column: "tenant");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "core_acl");

            migrationBuilder.DropTable(
                name: "core_group");

            migrationBuilder.DropTable(
                name: "core_subscription");

            migrationBuilder.DropTable(
                name: "core_subscriptionmethod");

            migrationBuilder.DropTable(
                name: "core_usergroup");

            migrationBuilder.DropTable(
                name: "core_userphoto");

            migrationBuilder.DropTable(
                name: "core_usersecurity");

            migrationBuilder.DropTable(
                name: "core_user");
        }
    }
}
