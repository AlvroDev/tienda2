using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using tienda.Application.Contracts.Authentication;
using tienda.Domain.Entities;
using tienda.Domain.Enums;
using tienda.Domain.ValueObjects;

namespace tienda.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAdminAsync(AppDbContext context, IPasswordHasher hasher, IConfiguration config)
    {
        var email = config["SeedAdmin:Email"];
        var password = config["SeedAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;

        // Si ya existe, no lo vuelve a crear (el seed corre en cada arranque)
        if (await context.Users.AnyAsync(u => u.Email.Value == email)) return;

        var admin = User.Create("Admin", "Tienda", Email.Create(email),
                                hasher.HashPassword(password), UserRole.Admin);
        context.Users.Add(admin);
        await context.SaveChangesAsync();
    }
}