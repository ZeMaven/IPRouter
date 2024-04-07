using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomoSwitch.models.database.Migrations
{
    /// <inheritdoc />
    public partial class migration9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DailyLimit",
                table: "SwitchTb",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTb_Date_Processor",
                table: "TransactionTb",
                columns: new[] { "Date", "Processor" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TransactionTb_Date_Processor",
                table: "TransactionTb");

            migrationBuilder.DropColumn(
                name: "DailyLimit",
                table: "SwitchTb");
        }
    }
}
