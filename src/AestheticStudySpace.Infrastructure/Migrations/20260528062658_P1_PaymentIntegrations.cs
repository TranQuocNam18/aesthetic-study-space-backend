using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AestheticStudySpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class P1_PaymentIntegrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Token column and index already exist from InitPostgres migration
            // Removed duplicate AddColumn operation
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No operations to undo - Token column and index already exist from InitPostgres
        }
    }
}
