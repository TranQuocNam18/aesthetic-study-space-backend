using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AestheticStudySpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class P3_UserThemeSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "StoreItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionNote",
                table: "StoreItems",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "StoreItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "StoreItems",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "AdminCreated");

            migrationBuilder.CreateIndex(
                name: "IX_StoreItems_CreatorId",
                table: "StoreItems",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreItems_Status",
                table: "StoreItems",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_StoreItems_Users_CreatorId",
                table: "StoreItems",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoreItems_Users_CreatorId",
                table: "StoreItems");

            migrationBuilder.DropIndex(
                name: "IX_StoreItems_CreatorId",
                table: "StoreItems");

            migrationBuilder.DropIndex(
                name: "IX_StoreItems_Status",
                table: "StoreItems");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "StoreItems");

            migrationBuilder.DropColumn(
                name: "RejectionNote",
                table: "StoreItems");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "StoreItems");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "StoreItems");
        }
    }
}
