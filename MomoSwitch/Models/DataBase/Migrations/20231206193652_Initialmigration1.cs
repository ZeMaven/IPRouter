using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomoSwitch.models.database.Migrations
{
    /// <inheritdoc />
    public partial class Initialmigration1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TransactionId",
                table: "TransactionTb",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTb_TransactionId",
                table: "TransactionTb",
                column: "TransactionId",
                unique: true,
                filter: "[TransactionId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TransactionTb_TransactionId",
                table: "TransactionTb");

            migrationBuilder.AlterColumn<string>(
                name: "TransactionId",
                table: "TransactionTb",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
