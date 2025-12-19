using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bags_Shop_API.Migrations
{
    /// <inheritdoc />
    public partial class addwebhook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentWebhooks_Orders_OrderId",
                table: "PaymentWebhooks");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentWebhooks_Payments_PaymentId",
                table: "PaymentWebhooks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentWebhooks",
                table: "PaymentWebhooks");

            migrationBuilder.RenameTable(
                name: "PaymentWebhooks",
                newName: "PaymentWebhook");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentWebhooks_PaymentId",
                table: "PaymentWebhook",
                newName: "IX_PaymentWebhook_PaymentId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentWebhooks_OrderId",
                table: "PaymentWebhook",
                newName: "IX_PaymentWebhook_OrderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentWebhook",
                table: "PaymentWebhook",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentWebhook_Orders_OrderId",
                table: "PaymentWebhook",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentWebhook_Payments_PaymentId",
                table: "PaymentWebhook",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentWebhook_Orders_OrderId",
                table: "PaymentWebhook");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentWebhook_Payments_PaymentId",
                table: "PaymentWebhook");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentWebhook",
                table: "PaymentWebhook");

            migrationBuilder.RenameTable(
                name: "PaymentWebhook",
                newName: "PaymentWebhooks");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentWebhook_PaymentId",
                table: "PaymentWebhooks",
                newName: "IX_PaymentWebhooks_PaymentId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentWebhook_OrderId",
                table: "PaymentWebhooks",
                newName: "IX_PaymentWebhooks_OrderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentWebhooks",
                table: "PaymentWebhooks",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentWebhooks_Orders_OrderId",
                table: "PaymentWebhooks",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentWebhooks_Payments_PaymentId",
                table: "PaymentWebhooks",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id");
        }
    }
}
