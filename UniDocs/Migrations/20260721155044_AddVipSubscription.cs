using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniDocs.Migrations
{
    /// <inheritdoc />
    public partial class AddVipSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DailyDownloadCount",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDownloadDate",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VipExpirationDate",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyDownloadCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastDownloadDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "VipExpirationDate",
                table: "Users");
        }
    }
}
