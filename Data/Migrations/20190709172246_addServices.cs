using Microsoft.EntityFrameworkCore.Migrations;

namespace SammysAuto.Data.Migrations
{
    public partial class addServices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_Cars_CardId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_CardId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "CardId",
                table: "Services");

            migrationBuilder.RenameColumn(
                name: "ServicesTypeId",
                table: "Services",
                newName: "ServiceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_CarId",
                table: "Services",
                column: "CarId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Cars_CarId",
                table: "Services",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_Cars_CarId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_CarId",
                table: "Services");

            migrationBuilder.RenameColumn(
                name: "ServiceTypeId",
                table: "Services",
                newName: "ServicesTypeId");

            migrationBuilder.AddColumn<int>(
                name: "CardId",
                table: "Services",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Services_CardId",
                table: "Services",
                column: "CardId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Cars_CardId",
                table: "Services",
                column: "CardId",
                principalTable: "Cars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
