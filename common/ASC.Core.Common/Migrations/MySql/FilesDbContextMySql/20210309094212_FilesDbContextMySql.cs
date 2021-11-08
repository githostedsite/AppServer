﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace ASC.Core.Common.Migrations.MySql.FilesDbContextMySql
{
    public partial class FilesDbContextMySql : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "files_converts",
                columns: table => new
                {
                    input = table.Column<string>(type: "varchar(50)", nullable: false, collation: "utf8_general_ci")
                        .Annotation("MySql:CharSet", "utf8"),
                    output = table.Column<string>(type: "varchar(50)", nullable: false, collation: "utf8_general_ci")
                        .Annotation("MySql:CharSet", "utf8")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.input, x.output });
                });

            migrationBuilder.InsertData(
                table: "files_converts",
                columns: new[] { "input", "output" },
                values: new object[,]
                {
                    { ".csv", ".ods" },
                    { ".pps", ".pdf" },
                    { ".pps", ".pptx" },
                    { ".ppsm", ".odp" },
                    { ".ppsm", ".pdf" },
                    { ".ppsm", ".pptx" },
                    { ".ppsx", ".odp" },
                    { ".pps", ".odp" },
                    { ".ppsx", ".pdf" },
                    { ".ppt", ".odp" },
                    { ".ppt", ".pdf" },
                    { ".ppt", ".pptx" },
                    { ".pptm", ".odp" },
                    { ".pptm", ".pdf" },
                    { ".pptm", ".pptx" },
                    { ".ppsx", ".pptx" },
                    { ".pptt", ".odp" },
                    { ".potx", ".pptx" },
                    { ".potx", ".odp" },
                    { ".odt", ".pdf" },
                    { ".odt", ".rtf" },
                    { ".odt", ".txt" },
                    { ".ott", ".docx" },
                    { ".ott", ".odt" },
                    { ".ott", ".pdf" },
                    { ".potx", ".pdf" },
                    { ".ott", ".rtf" },
                    { ".pot", ".odp" },
                    { ".pot", ".pdf" },
                    { ".pot", ".pptx" },
                    { ".potm", ".odp" },
                    { ".potm", ".pdf" },
                    { ".potm", ".pptx" },
                    { ".ott", ".txt" },
                    { ".odt", ".docx" },
                    { ".pptt", ".pdf" },
                    { ".pptx", ".odp" },
                    { ".xlst", ".xlsx" },
                    { ".xlst", ".csv" },
                    { ".xlst", ".ods" },
                    { ".xlt", ".csv" },
                    { ".xlt", ".ods" },
                    { ".xlt", ".pdf" },
                    { ".xlst", ".pdf" },
                    { ".xlt", ".xlsx" },
                    { ".xltm", ".ods" },
                    { ".xltm", ".pdf" },
                    { ".xltm", ".xlsx" },
                    { ".xltx", ".pdf" },
                    { ".xltx", ".csv" },
                    { ".xltx", ".ods" },
                    { ".xltm", ".csv" },
                    { ".pptt", ".pptx" },
                    { ".xlsm", ".xlsx" },
                    { ".xlsm", ".pdf" },
                    { ".pptx", ".pdf" },
                    { ".rtf", ".odp" },
                    { ".rtf", ".pdf" },
                    { ".rtf", ".docx" },
                    { ".rtf", ".txt" },
                    { ".txt", ".pdf" },
                    { ".xlsm", ".ods" },
                    { ".txt", ".docx" },
                    { ".txt", ".rtx" },
                    { ".xls", ".csv" },
                    { ".xls", ".ods" },
                    { ".xls", ".pdf" },
                    { ".xls", ".xlsx" },
                    { ".xlsm", ".csv" },
                    { ".txt", ".odp" },
                    { ".xltx", ".xlsx" },
                    { ".ots", ".xlsx" },
                    { ".ots", ".ods" },
                    { ".dot", ".odt" },
                    { ".dot", ".pdf" },
                    { ".dot", ".rtf" },
                    { ".dot", ".txt" },
                    { ".dotm", ".docx" },
                    { ".dotm", ".odt" },
                    { ".dot", ".docx" },
                    { ".dotm", ".pdf" },
                    { ".dotm", ".txt" },
                    { ".dotx", ".docx" },
                    { ".dotx", ".odt" },
                    { ".dotx", ".pdf" },
                    { ".dotx", ".rtf" },
                    { ".dotx", ".txt" },
                    { ".dotm", ".rtf" },
                    { ".epub", ".docx" },
                    { ".docx", ".txt" },
                    { ".docx", ".pdf" },
                    { ".csv", ".pdf" },
                    { ".csv", ".xlsx" },
                    { ".doc", ".docx" },
                    { ".doc", ".odt" },
                    { ".doc", ".pdf" },
                    { ".doc", ".rtf" },
                    { ".docx", ".rtf" },
                    { ".doc", ".txt" },
                    { ".docm", ".odt" },
                    { ".docm", ".pdf" },
                    { ".docm", ".rtf" },
                    { ".docm", ".txt" },
                    { ".doct", ".docx" },
                    { ".docx", ".odt" },
                    { ".docm", ".docx" },
                    { ".ots", ".pdf" },
                    { ".epub", ".odt" },
                    { ".epub", ".rtf" },
                    { ".mht", ".docx" },
                    { ".mht", ".odt" },
                    { ".mht", ".pdf" },
                    { ".mht", ".rtf" },
                    { ".mht", ".txt" },
                    { ".odp", ".pdf" },
                    { ".html", ".txt" },
                    { ".odp", ".pptx" },
                    { ".otp", ".pdf" },
                    { ".otp", ".pptx" },
                    { ".ods", ".csv" },
                    { ".ods", ".pdf" },
                    { ".ods", ".xlsx" },
                    { ".ots", ".csv" },
                    { ".otp", ".odp" },
                    { ".epub", ".pdf" },
                    { ".html", ".rtf" },
                    { ".html", ".odt" },
                    { ".epub", ".txt" },
                    { ".fodp", ".odp" },
                    { ".fodp", ".pdf" },
                    { ".fodp", ".pptx" },
                    { ".fods", ".csv" },
                    { ".fods", ".ods" },
                    { ".html", ".pdf" },
                    { ".fods", ".pdf" },
                    { ".fodt", ".docx" },
                    { ".fodt", ".odt" },
                    { ".fodt", ".pdf" },
                    { ".fodt", ".rtf" },
                    { ".fodt", ".txt" },
                    { ".html", ".docx" },
                    { ".fods", ".xlsx" },
                    { ".xps", ".pdf" },
                    { ".fb2", ".docx" },
                    { ".fb2", ".odt" },
                    { ".fb2", ".pdf" },
                    { ".fb2", ".rtf" },
                    { ".fb2", ".txt" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "files_converts");
        }
    }
}
