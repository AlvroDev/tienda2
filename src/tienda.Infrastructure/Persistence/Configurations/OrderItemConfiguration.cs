using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using tienda.Domain.Entities;

namespace tienda.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(i => i.UnitPrice).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(i => i.Quantity).IsRequired();

        // Subtotal se calcula (UnitPrice * Quantity), no se guarda en la base
        builder.Ignore(i => i.Subtotal);

        // Cada ítem apunta a un producto existente. No se puede borrar un producto que fue vendido
        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}