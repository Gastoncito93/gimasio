using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddSociosProgresosTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SociosProgresos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdSocio = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PesoKg = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Observaciones = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RutaFotoFrente = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RutaFotoPerfil = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RutaFotoEspalda = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreadoAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SociosProgresos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SociosProgresos_Socios_IdSocio",
                        column: x => x.IdSocio,
                        principalTable: "Socios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Actividades",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreadoAt",
                value: new DateTime(2026, 7, 29, 19, 14, 50, 85, DateTimeKind.Utc).AddTicks(3317));

            migrationBuilder.UpdateData(
                table: "Actividades",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreadoAt",
                value: new DateTime(2026, 7, 29, 19, 14, 50, 85, DateTimeKind.Utc).AddTicks(4000));

            migrationBuilder.UpdateData(
                table: "Actividades",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreadoAt",
                value: new DateTime(2026, 7, 29, 19, 14, 50, 85, DateTimeKind.Utc).AddTicks(4002));

            migrationBuilder.UpdateData(
                table: "Actividades",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreadoAt",
                value: new DateTime(2026, 7, 29, 19, 14, 50, 85, DateTimeKind.Utc).AddTicks(4003));

            migrationBuilder.CreateIndex(
                name: "IX_SociosProgresos_IdSocio",
                table: "SociosProgresos",
                column: "IdSocio");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SociosProgresos");

            migrationBuilder.UpdateData(
                table: "Actividades",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreadoAt",
                value: new DateTime(2026, 7, 29, 12, 45, 52, 857, DateTimeKind.Utc).AddTicks(4414));

            migrationBuilder.UpdateData(
                table: "Actividades",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreadoAt",
                value: new DateTime(2026, 7, 29, 12, 45, 52, 857, DateTimeKind.Utc).AddTicks(5155));

            migrationBuilder.UpdateData(
                table: "Actividades",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreadoAt",
                value: new DateTime(2026, 7, 29, 12, 45, 52, 857, DateTimeKind.Utc).AddTicks(5158));

            migrationBuilder.UpdateData(
                table: "Actividades",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreadoAt",
                value: new DateTime(2026, 7, 29, 12, 45, 52, 857, DateTimeKind.Utc).AddTicks(5159));
        }
    }
}
