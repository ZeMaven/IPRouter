using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomoSwitch.models.database.Migrations
{
    /// <inheritdoc />
    public partial class migration14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyReconciliationTb",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentRef = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Processor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EwpSessionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MsrSessionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessorSessionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EwpResponseCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MsrResponseCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessorResponseCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyReconciliationTb", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyReconciliationTb");
        }
    }
}
