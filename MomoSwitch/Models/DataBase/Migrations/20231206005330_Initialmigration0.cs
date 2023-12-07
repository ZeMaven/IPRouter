using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomoSwitch.models.database.Migrations
{
    /// <inheritdoc />
    public partial class Initialmigration0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AmountRuleTb",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AmountA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountZ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Processor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmountRuleTb", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankSwitchTb",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Processor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankSwitchTb", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PriorityTb",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Rule = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriorityTb", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SwitchTb",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Processor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEnquiryUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransferUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TranQueryUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SwitchTb", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimeRuleTb",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeZ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Processor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeRuleTb", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionTb",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Processor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PaymentReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BenefBankCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BenefAccountName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BenefAccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BenefBvn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BenefKycLevel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Narration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceAccountName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceAccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceBvn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceKycLevel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceBankCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChannelCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManadateRef = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEnquiryRef = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ResponseCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionTb", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTb_Date",
                table: "TransactionTb",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTb_SessionId",
                table: "TransactionTb",
                column: "SessionId",
                unique: true,
                filter: "[SessionId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AmountRuleTb");

            migrationBuilder.DropTable(
                name: "BankSwitchTb");

            migrationBuilder.DropTable(
                name: "PriorityTb");

            migrationBuilder.DropTable(
                name: "SwitchTb");

            migrationBuilder.DropTable(
                name: "TimeRuleTb");

            migrationBuilder.DropTable(
                name: "TransactionTb");
        }
    }
}
