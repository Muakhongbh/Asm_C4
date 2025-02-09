using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace asm_1_Web_DoAn.Migrations
{
    /// <inheritdoc />
    public partial class add_giohang : Migration
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

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ComboId",
                table: "CartItems",
                column: "ComboId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Combos_ComboId",
                table: "CartItems",
                column: "ComboId",
                principalTable: "Combos",
                principalColumn: "ComboId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Combos_ComboId",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_ComboId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "ComboId",
                table: "CartItems");
        }
    }
}
