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

        // Aquí le decimos a .NET qué modelos se van a convertir en tablas en SQL
        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Proveedores> Proveedores { get; set; }
        public DbSet<Clientes> Clientes { get; set; }
        public DbSet<Productos> Productos { get; set; }
        public DbSet<IngresoProductos> IngresoProductos { get; set; }
        public DbSet<SalidaProductos> SalidaProductos { get; set; }
    }
}