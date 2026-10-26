using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using tienda.Domain.Entities;

namespace tienda.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever(); // el Guid lo genera el dominio

        builder.Property(o => o.UserId).IsRequired();
        builder.Property(o => o.CreatedAt).IsRequired();

        // Enum guardado como texto legible ("Pending", "Paid"...) en vez de número
        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.PaymentTransactionId).HasMaxLength(50);

        // Money es un Value Object: se guarda como dos columnas dentro de la tabla Orders
        builder.OwnsOne(o => o.Total, total =>
        {
            total.Property(m => m.Amount).HasColumnName("Total").HasColumnType("decimal(18,2)").IsRequired();
            total.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3).IsRequired();
        });

        // Relación 1:N con los ítems. Si se borra la orden, se borran sus ítems
        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // EF escribe en el campo privado _items (la propiedad Items es de solo lectura)
        builder.Navigation(o => o.Items).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Cada orden pertenece a un usuario existente. No se puede borrar un usuario con órdenes
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(o => o.UserId); // búsquedas rápidas de "mis órdenes"
    }
}