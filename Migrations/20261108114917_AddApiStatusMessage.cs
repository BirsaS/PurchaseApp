using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseApp.Migrations
{
    public partial class AddApiStatusMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiStatusMessage",
                table: "PurchaseRequests",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiStatusMessage",
                table: "PurchaseRequests");
        }
    }
}
