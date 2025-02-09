using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace asm_1_Web_DoAn.Migrations
{
    /// <inheritdoc />
    public partial class acc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_FoodItems_FoodItemId1",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_FoodItemId1",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "FoodItemId1",
                table: "CartItems");

            migrationBuilder.AddColumn<string>(
                name: "UserNameCart",
                table: "Carts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserNameCart",
                table: "Carts");

            migrationBuilder.AddColumn<int>(
                name: "FoodItemId1",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_FoodItemId1",
                table: "CartItems",
                column: "FoodItemId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_FoodItems_FoodItemId1",
                table: "CartItems",
                column: "FoodItemId1",
                principalTable: "FoodItems",
                principalColumn: "FoodItemId");
        }
    }
}
