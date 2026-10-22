using Microsoft.EntityFrameworkCore;
using tp5_Trani_Joaco_Alex.Models;

namespace tp5_Trani_Joaco_Alex.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Proveedores> Proveedores { get; set; }
        public DbSet<Clientes> Clientes { get; set; }
        public DbSet<Productos> Productos { get; set; }
        public DbSet<IngresoProductos> IngresoProductos { get; set; }
        public DbSet<SalidaProductos> SalidaProductos { get; set; }
        public DbSet<DetalleIngresoProductos> DetalleIngresoProductos { get; set; }
        public DbSet<DetalleSalidaProductos> DetalleSalidaProductos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Usuarios (Administrador, 2 Cajeros, 2 Clientes Móviles)
            modelBuilder.Entity<Usuarios>().HasData(
                new Usuarios { UsuarioId = 1, Nombre = "Joaquin Admin", Email = "admin@ites.edu.ar", Password = "123", Rol = Roles.Admin, Activo = true },
                new Usuarios { UsuarioId = 2, Nombre = "Caja 1 - Turno Mañana", Email = "caja1@ites.edu.ar", Password = "123", Rol = Roles.Operador, Activo = true },
                new Usuarios { UsuarioId = 3, Nombre = "Caja 2 - Turno Tarde", Email = "caja2@ites.edu.ar", Password = "123", Rol = Roles.Operador, Activo = true },
                new Usuarios { UsuarioId = 4, Nombre = "Alex Comprador", Email = "alex@correo.com", Password = "123", Rol = Roles.Cliente, Activo = true },
                new Usuarios { UsuarioId = 5, Nombre = "Maria Cliente", Email = "maria@correo.com", Password = "123", Rol = Roles.Cliente, Activo = true }
            );

            // 2. Clientes (Consumidor Final y clientes registrados)
            modelBuilder.Entity<Clientes>().HasData(
                new Clientes { ClienteId = 1, Nombre = "Consumidor Final", Email = "cf@correo.com", Telefono = "-", Activo = true },
                new Clientes { ClienteId = 2, Nombre = "Alex Comprador", Email = "alex@correo.com", Telefono = "2954-111111", Activo = true },
                new Clientes { ClienteId = 3, Nombre = "Maria Cliente", Email = "maria@correo.com", Telefono = "2954-222222", Activo = true },
                new Clientes { ClienteId = 4, Nombre = "Empresa Local S.R.L.", Email = "compras@empresalocal.com", Telefono = "2954-333333", Activo = true },
                new Clientes { ClienteId = 5, Nombre = "Juan Perez", Email = "juanp@correo.com", Telefono = "2954-444444", Activo = true }
            );

            // 3. Categorías
            modelBuilder.Entity<Categoria>().HasData(
                new Categoria { CategoriaId = 1, Nombre = "Bebidas", Descripcion = "Gaseosas, aguas, jugos", Activo = true },
                new Categoria { CategoriaId = 2, Nombre = "Snacks", Descripcion = "Papas, palitos, chizitos", Activo = true },
                new Categoria { CategoriaId = 3, Nombre = "Lácteos", Descripcion = "Leche, yogur, quesos", Activo = true },
                new Categoria { CategoriaId = 4, Nombre = "Almacén", Descripcion = "Fideos, arroz, conservas", Activo = true },
                new Categoria { CategoriaId = 5, Nombre = "Limpieza", Descripcion = "Detergentes, lavandina", Activo = true }
            );

            // 4. Proveedores
            modelBuilder.Entity<Proveedores>().HasData(
                new Proveedores { ProveedorId = 1, RazonSocial = "Distribuidora Bebidas S.A.", Telefono = "11-5555-0001", Email = "ventas@bebidas.com", Activo = true },
                new Proveedores { ProveedorId = 2, RazonSocial = "Snacks Pampeanos", Telefono = "2954-5555-0002", Email = "pedidos@snackspampeanos.com", Activo = true },
                new Proveedores { ProveedorId = 3, RazonSocial = "Lácteos del Sur", Telefono = "2954-5555-0003", Email = "contacto@lacteosdelsur.com", Activo = true },
                new Proveedores { ProveedorId = 4, RazonSocial = "Mayorista Almacén", Telefono = "11-5555-0004", Email = "info@mayorista.com", Activo = true },
                new Proveedores { ProveedorId = 5, RazonSocial = "Química Limpieza", Telefono = "2954-5555-0005", Email = "ventas@quimica.com", Activo = true }
            );

            // 5. Productos
            modelBuilder.Entity<Productos>().HasData(
                new Productos { ProductoId = 1, Nombre = "Coca Cola 2.25L", Precio = 2500m, Stock = 50, CategoriaId = 1, Imagen = "coca.jpg", Activo = true },
                new Productos { ProductoId = 2, Nombre = "Sprite 2.25L", Precio = 2400m, Stock = 40, CategoriaId = 1, Imagen = "sprite.jpg", Activo = true },
                new Productos { ProductoId = 3, Nombre = "Papas Lays Clásicas 145g", Precio = 1800m, Stock = 30, CategoriaId = 2, Imagen = "lays.jpg", Activo = true },
                new Productos { ProductoId = 4, Nombre = "Leche Serenísima 1L", Precio = 1200m, Stock = 60, CategoriaId = 3, Imagen = "leche.jpg", Activo = true },
                new Productos { ProductoId = 5, Nombre = "Fideos Matarazzo 500g", Precio = 1100m, Stock = 100, CategoriaId = 4, Imagen = "fideos.jpg", Activo = true },
                new Productos { ProductoId = 6, Nombre = "Lavandina Ayudín 1L", Precio = 950m, Stock = 25, CategoriaId = 5, Imagen = "lavandina.jpg", Activo = true }
            );
        }
    }
}