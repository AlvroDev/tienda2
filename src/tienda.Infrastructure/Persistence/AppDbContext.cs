using Microsoft.EntityFrameworkCore;
using tienda.Domain.Entities;

namespace tienda.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    //Tabla de Productos mapeada en la Base de Datos
    public DbSet<Product> Products { get; set; } = null!;

    //Tabla de Usuarios mapeada en la Base de Datos
    public DbSet<User> Users { get; set; } = null!;

    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Esto le dice a EF Core que busque automáticamente todas las configuraciones de Fluent API
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}