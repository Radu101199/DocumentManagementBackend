using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentManagementBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "DocumentManagement",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "DocumentManagement",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "DocumentManagement",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "DocumentManagement",
                table: "Documents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "DocumentManagement",
                table: "Documents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "DocumentManagement",
                table: "Documents",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "DocumentManagement",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "DocumentManagement",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "DocumentManagement",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "DocumentManagement",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "DocumentManagement",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "DocumentManagement",
                table: "Documents");
        }
    }
}
