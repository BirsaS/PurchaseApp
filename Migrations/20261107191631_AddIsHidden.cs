using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseApp.Migrations
{
    public partial class AddIsHidden : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "PurchaseRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "PurchaseRequests");
        }
    }
}
