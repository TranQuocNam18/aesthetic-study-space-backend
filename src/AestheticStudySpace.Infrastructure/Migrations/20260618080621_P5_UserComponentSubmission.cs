using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AestheticStudySpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class P5_UserComponentSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentThemeId",
                table: "StoreItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoreItems_ParentThemeId",
                table: "StoreItems",
                column: "ParentThemeId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoreItems_StoreItems_ParentThemeId",
                table: "StoreItems",
                column: "ParentThemeId",
                principalTable: "StoreItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoreItems_StoreItems_ParentThemeId",
                table: "StoreItems");

            migrationBuilder.DropIndex(
                name: "IX_StoreItems_ParentThemeId",
                table: "StoreItems");

            migrationBuilder.DropColumn(
                name: "ParentThemeId",
                table: "StoreItems");
        }
    }
}
