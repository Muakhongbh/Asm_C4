using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace asm_1_Web_DoAn.Migrations
{
    /// <inheritdoc />
    public partial class combo_sl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ComboQuantity",
                table: "Combos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComboQuantity",
                table: "Combos");
        }
    }
}
