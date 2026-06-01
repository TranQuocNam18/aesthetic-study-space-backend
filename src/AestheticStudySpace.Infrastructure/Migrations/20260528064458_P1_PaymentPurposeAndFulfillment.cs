using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AestheticStudySpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class P1_PaymentPurposeAndFulfillment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFulfilled",
                table: "PaymentTransactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MetadataJson",
                table: "PaymentTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "PaymentTransactions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_Purpose_Status",
                table: "PaymentTransactions",
                columns: new[] { "Purpose", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_Purpose_Status",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "IsFulfilled",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "MetadataJson",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "PaymentTransactions");
        }
    }
}
