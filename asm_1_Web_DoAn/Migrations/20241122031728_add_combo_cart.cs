using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace asm_1_Web_DoAn.Migrations
{
    /// <inheritdoc />
    public partial class add_combo_cart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ComboId",
                table: "CartItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ComboId1",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ComboId",
                table: "CartItems",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ComboId1",
                table: "CartItems",
                column: "ComboId1",
                unique: true,
                filter: "[ComboId1] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Combos_ComboId",
                table: "CartItems",
                column: "ComboId",
                principalTable: "Combos",
                principalColumn: "ComboId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Combos_ComboId1",
                table: "CartItems",
                column: "ComboId1",
                principalTable: "Combos",
                principalColumn: "ComboId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Combos_ComboId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Combos_ComboId1",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_ComboId",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_ComboId1",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "ComboId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "ComboId1",
                table: "CartItems");
        }
    }
}
