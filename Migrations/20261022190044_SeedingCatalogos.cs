using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace tp5_Trani_Joaco_Alex.Migrations
{
    /// <inheritdoc />
    public partial class SeedingCatalogos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "CategoriaId", "Activo", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "Gaseosas, aguas, jugos", "Bebidas" },
                    { 2, true, "Papas, palitos, chizitos", "Snacks" },
                    { 3, true, "Leche, yogur, quesos", "Lácteos" },
                    { 4, true, "Fideos, arroz, conservas", "Almacén" },
                    { 5, true, "Detergentes, lavandina", "Limpieza" }
                });

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "ClienteId", "Activo", "Email", "Nombre", "Telefono" },
                values: new object[,]
                {
                    { 1, true, "cf@correo.com", "Consumidor Final", "-" },
                    { 2, true, "alex@correo.com", "Alex Comprador", "2954-111111" },
                    { 3, true, "maria@correo.com", "Maria Cliente", "2954-222222" },
                    { 4, true, "compras@empresalocal.com", "Empresa Local S.R.L.", "2954-333333" },
                    { 5, true, "juanp@correo.com", "Juan Perez", "2954-444444" }
                });

            migrationBuilder.InsertData(
                table: "Proveedores",
                columns: new[] { "ProveedorId", "Activo", "Email", "RazonSocial", "Telefono" },
                values: new object[,]
                {
                    { 1, true, "ventas@bebidas.com", "Distribuidora Bebidas S.A.", "11-5555-0001" },
                    { 2, true, "pedidos@snackspampeanos.com", "Snacks Pampeanos", "2954-5555-0002" },
                    { 3, true, "contacto@lacteosdelsur.com", "Lácteos del Sur", "2954-5555-0003" },
                    { 4, true, "info@mayorista.com", "Mayorista Almacén", "11-5555-0004" },
                    { 5, true, "ventas@quimica.com", "Química Limpieza", "2954-5555-0005" }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "UsuarioId", "Activo", "Email", "Nombre", "Password", "Rol" },
                values: new object[,]
                {
                    { 1, true, "admin@ites.edu.ar", "Joaquin Admin", "123", "Admin" },
                    { 2, true, "caja1@ites.edu.ar", "Caja 1 - Turno Mañana", "123", "Operador" },
                    { 3, true, "caja2@ites.edu.ar", "Caja 2 - Turno Tarde", "123", "Operador" },
                    { 4, true, "alex@correo.com", "Alex Comprador", "123", "Cliente" },
                    { 5, true, "maria@correo.com", "Maria Cliente", "123", "Cliente" }
                });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "ProductoId", "Activo", "CategoriaId", "Imagen", "Nombre", "Precio", "Stock" },
                values: new object[,]
                {
                    { 1, true, 1, "coca.jpg", "Coca Cola 2.25L", 2500m, 50 },
                    { 2, true, 1, "sprite.jpg", "Sprite 2.25L", 2400m, 40 },
                    { 3, true, 2, "lays.jpg", "Papas Lays Clásicas 145g", 1800m, 30 },
                    { 4, true, 3, "leche.jpg", "Leche Serenísima 1L", 1200m, 60 },
                    { 5, true, 4, "fideos.jpg", "Fideos Matarazzo 500g", 1100m, 100 },
                    { 6, true, 5, "lavandina.jpg", "Lavandina Ayudín 1L", 950m, 25 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "ClienteId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "ClienteId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "ClienteId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "ClienteId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "ClienteId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "ProductoId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "ProductoId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "ProductoId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "ProductoId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "ProductoId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "ProductoId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Proveedores",
                keyColumn: "ProveedorId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Proveedores",
                keyColumn: "ProveedorId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Proveedores",
                keyColumn: "ProveedorId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Proveedores",
                keyColumn: "ProveedorId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Proveedores",
                keyColumn: "ProveedorId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "UsuarioId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "UsuarioId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "UsuarioId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "UsuarioId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "UsuarioId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "CategoriaId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "CategoriaId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "CategoriaId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "CategoriaId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "CategoriaId",
                keyValue: 5);
        }
    }
}
