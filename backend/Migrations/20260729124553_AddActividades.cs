using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddActividades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdActividad",
                table: "Usuarios",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Actividades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Estado = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Activo")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreadoAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actividades", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Actividades",
                columns: new[] { "Id", "CreadoAt", "Descripcion", "Estado", "Nombre" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 7, 29, 12, 45, 52, 857, DateTimeKind.Utc).AddTicks(4414), "Entrenamiento de fuerza y sala libre", "Activo", "Musculación" },
                    { 2, new DateTime(2026, 7, 29, 12, 45, 52, 857, DateTimeKind.Utc).AddTicks(5155), "Entrenamiento funcional de alta intensidad", "Activo", "Crossfit" },
                    { 3, new DateTime(2026, 7, 29, 12, 45, 52, 857, DateTimeKind.Utc).AddTicks(5158), "Ciclismo de interior guiado", "Activo", "Spinning" },
                    { 4, new DateTime(2026, 7, 29, 12, 45, 52, 857, DateTimeKind.Utc).AddTicks(5159), "Flexibilidad, postura y relajación", "Activo", "Yoga" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_IdActividad",
                table: "Usuarios",
                column: "IdActividad");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Actividades_IdActividad",
                table: "Usuarios",
                column: "IdActividad",
                principalTable: "Actividades",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Actividades_IdActividad",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "Actividades");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_IdActividad",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "IdActividad",
                table: "Usuarios");
        }
    }
}
