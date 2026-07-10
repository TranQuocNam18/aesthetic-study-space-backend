using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AestheticStudySpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDefaultBankDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultBankAccountNumber",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultBankAccountOwnerName",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultBankName",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultBankAccountNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DefaultBankAccountOwnerName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DefaultBankName",
                table: "Users");
        }
    }
}
