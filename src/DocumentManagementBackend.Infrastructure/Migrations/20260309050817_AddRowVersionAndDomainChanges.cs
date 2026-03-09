using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentManagementBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionAndDomainChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "DocumentManagement",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Role",
                schema: "DocumentManagement",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "Roles",
                schema: "DocumentManagement",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "DocumentManagement",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovalExpiresAt",
                schema: "DocumentManagement",
                table: "Documents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovalRequestedAt",
                schema: "DocumentManagement",
                table: "Documents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "DocumentManagement",
                table: "Documents",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Roles",
                schema: "DocumentManagement",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "DocumentManagement",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ApprovalExpiresAt",
                schema: "DocumentManagement",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ApprovalRequestedAt",
                schema: "DocumentManagement",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "DocumentManagement",
                table: "Documents");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "DocumentManagement",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Role",
                schema: "DocumentManagement",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
