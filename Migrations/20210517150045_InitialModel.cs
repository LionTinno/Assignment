using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FMS.Migrations
{
    public partial class InitialModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "POTransaction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    transactionid = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    amount = table.Column<int>(type: "decimal", nullable: false),
                    currencycode = table.Column<string>(type: "nvarchar(5)", nullable: true),
                    transactiondate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<string>(type: "nvarchar(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POTransaction", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "POTransaction");
        }
    }
}
