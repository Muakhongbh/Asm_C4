using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace asm_1_Web_DoAn.Migrations
{
    /// <inheritdoc />
    public partial class add_combo_giohang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Combos_ComboId",
                table: "CartItems");

            migrationBuilder.AlterColumn<int>(
                name: "ComboId",
                table: "CartItems",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Combos_ComboId",
                table: "CartItems",
                column: "ComboId",
                principalTable: "Combos",
                principalColumn: "ComboId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Combos_ComboId",
                table: "CartItems");

            migrationBuilder.AlterColumn<int>(
                name: "ComboId",
                table: "CartItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Combos_ComboId",
                table: "CartItems",
                column: "ComboId",
                principalTable: "Combos",
                principalColumn: "ComboId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
