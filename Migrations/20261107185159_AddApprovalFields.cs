using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseApp.Migrations
{
    public partial class AddApprovalFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDepartmentHeadApproved",
                table: "PurchaseRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDirectorApproved",
                table: "PurchaseRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFinanceApproved",
                table: "PurchaseRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "PurchaseRequests",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDepartmentHeadApproved",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "IsDirectorApproved",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "IsFinanceApproved",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "PurchaseRequests");
        }
    }
}
