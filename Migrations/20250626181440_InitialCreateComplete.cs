using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CafeteriaSistema.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateComplete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Categoria = table.Column<int>(type: "INTEGER", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    StockDisponible = table.Column<int>(type: "INTEGER", nullable: false),
                    StockMinimo = table.Column<int>(type: "INTEGER", nullable: false),
                    EsMenuDelDia = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaUltimaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Documento = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    TipoUsuario = table.Column<int>(type: "INTEGER", nullable: false),
                    ModalidadPago = table.Column<int>(type: "INTEGER", nullable: true),
                    CodigoEstudiante = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosSistema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NombreUsuario = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    NombreCompleto = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Rol = table.Column<int>(type: "INTEGER", nullable: false),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaUltimoAcceso = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaUltimoCambioPassword = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IntentosLoginFallidos = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaBloqueo = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notas = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosSistema", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fichos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Numero = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaServicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaUso = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    Observaciones = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fichos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fichos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transacciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NumeroTransaccion = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TipoPago = table.Column<int>(type: "INTEGER", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PorcentajeDescuento = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    MontoDescuento = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    EsSubsidiado = table.Column<bool>(type: "INTEGER", nullable: false),
                    Observaciones = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transacciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transacciones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesTransaccion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TransaccionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Cantidad = table.Column<int>(type: "INTEGER", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Notas = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesTransaccion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesTransaccion_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesTransaccion_Transacciones_TransaccionId",
                        column: x => x.TransaccionId,
                        principalTable: "Transacciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "Id", "Activo", "Categoria", "Codigo", "Descripcion", "EsMenuDelDia", "FechaCreacion", "FechaUltimaModificacion", "Nombre", "Precio", "StockDisponible", "StockMinimo" },
                values: new object[,]
                {
                    { 1, true, 2, "ALM001", "Sopa, plato principal, jugo y postre", true, new DateTime(2025, 6, 26, 13, 14, 40, 606, DateTimeKind.Local).AddTicks(3085), null, "Almuerzo Ejecutivo", 12000m, 100, 20 },
                    { 2, true, 4, "BEB001", "Bebida gaseosa en lata", false, new DateTime(2025, 6, 26, 13, 14, 40, 606, DateTimeKind.Local).AddTicks(3102), null, "Gaseosa 350ml", 3500m, 50, 10 },
                    { 3, true, 1, "DES001", "Café, pan, huevos y fruta", false, new DateTime(2025, 6, 26, 13, 14, 40, 606, DateTimeKind.Local).AddTicks(3104), null, "Desayuno Completo", 8000m, 50, 10 }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Activo", "Apellido", "CodigoEstudiante", "Documento", "Email", "FechaRegistro", "ModalidadPago", "Nombre", "Telefono", "TipoUsuario" },
                values: new object[] { 1, true, "Sistema", null, "1234567890", "admin@unal.edu.co", new DateTime(2025, 6, 26, 13, 14, 40, 606, DateTimeKind.Local).AddTicks(3177), null, "Admin", "3001234567", 3 });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesTransaccion_ProductoId",
                table: "DetallesTransaccion",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesTransaccion_TransaccionId",
                table: "DetallesTransaccion",
                column: "TransaccionId");

            migrationBuilder.CreateIndex(
                name: "IX_Fichos_Numero",
                table: "Fichos",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fichos_UsuarioId_FechaServicio",
                table: "Fichos",
                columns: new[] { "UsuarioId", "FechaServicio" },
                unique: true,
                filter: "[Estado] != 3");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Codigo",
                table: "Productos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transacciones_NumeroTransaccion",
                table: "Transacciones",
                column: "NumeroTransaccion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transacciones_UsuarioId",
                table: "Transacciones",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Documento",
                table: "Usuarios",
                column: "Documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallesTransaccion");

            migrationBuilder.DropTable(
                name: "Fichos");

            migrationBuilder.DropTable(
                name: "UsuariosSistema");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Transacciones");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
