using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bags_Shop_API.Migrations
{
    /// <inheritdoc />
    public partial class updatepaymententity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentIntentionId",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentLink",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentLinkExpiresAt",
                table: "Payments",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentIntentionId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentLink",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentLinkExpiresAt",
                table: "Payments");
        }
    }
}
