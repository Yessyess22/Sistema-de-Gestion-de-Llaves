using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGestionLlaves.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFirmaPrestamo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "firma_base64",
                table: "Prestamo",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "firma_base64",
                table: "Prestamo");
        }
    }
}
