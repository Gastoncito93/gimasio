using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCoachAndUsuarioToSocio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdCoach",
                table: "Socios",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdUsuario",
                table: "Socios",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Socios_IdCoach",
                table: "Socios",
                column: "IdCoach");

            migrationBuilder.CreateIndex(
                name: "IX_Socios_IdUsuario",
                table: "Socios",
                column: "IdUsuario",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Socios_Usuarios_IdCoach",
                table: "Socios",
                column: "IdCoach",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Socios_Usuarios_IdUsuario",
                table: "Socios",
                column: "IdUsuario",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Socios_Usuarios_IdCoach",
                table: "Socios");

            migrationBuilder.DropForeignKey(
                name: "FK_Socios_Usuarios_IdUsuario",
                table: "Socios");

            migrationBuilder.DropIndex(
                name: "IX_Socios_IdCoach",
                table: "Socios");

            migrationBuilder.DropIndex(
                name: "IX_Socios_IdUsuario",
                table: "Socios");

            migrationBuilder.DropColumn(
                name: "IdCoach",
                table: "Socios");

            migrationBuilder.DropColumn(
                name: "IdUsuario",
                table: "Socios");
        }
    }
}
