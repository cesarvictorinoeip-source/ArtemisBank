using ArtemisBank.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace ArtemisBank.Infrastructure.Identity.Seeds
{
    public static class DefaultRoles
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!roleManager.Roles.Any())
            {
                await roleManager.CreateAsync(new IdentityRole("Administrador"));
                await roleManager.CreateAsync(new IdentityRole("Cajero"));
                await roleManager.CreateAsync(new IdentityRole("Cliente"));
            }
        }
    }
}