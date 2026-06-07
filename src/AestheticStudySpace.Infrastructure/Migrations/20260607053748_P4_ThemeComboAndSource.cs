using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AestheticStudySpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class P4_ThemeComboAndSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ThemeAmbientSoundItemId",
                table: "StoreItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ThemeBackgroundItemId",
                table: "StoreItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ThemeEffectItemId",
                table: "StoreItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThemeSource",
                table: "StoreItems",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ThemeStickerItemId",
                table: "StoreItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoreItems_ThemeSource",
                table: "StoreItems",
                column: "ThemeSource");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StoreItems_ThemeSource",
                table: "StoreItems");

            migrationBuilder.DropColumn(
                name: "ThemeAmbientSoundItemId",
                table: "StoreItems");

            migrationBuilder.DropColumn(
                name: "ThemeBackgroundItemId",
                table: "StoreItems");

            migrationBuilder.DropColumn(
                name: "ThemeEffectItemId",
                table: "StoreItems");

            migrationBuilder.DropColumn(
                name: "ThemeSource",
                table: "StoreItems");

            migrationBuilder.DropColumn(
                name: "ThemeStickerItemId",
                table: "StoreItems");
        }
    }
}
