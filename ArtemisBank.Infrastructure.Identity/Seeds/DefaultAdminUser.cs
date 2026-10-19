using ArtemisBank.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace ArtemisBank.Infrastructure.Identity.Seeds
{
    public static class DefaultAdminUser
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager)
        {
            ApplicationUser user = new()
            {
                FirstName = "Admin",
                LastName = "Artemis",
                IdCard = "00000000001",
                UserName = "adminArtemis",
                Email = "admin@artemisbank.com",
                UserType = "Administrador",
                IsActive = true,
                EmailConfirmed = true
            };

            if (userManager.Users.All(u => u.UserName != user.UserName))
            {
                var existingUser = await userManager.FindByEmailAsync(user.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(user, "Admin@123");
                    await userManager.AddToRoleAsync(user, "Administrador");
                }
            }
        }
    }
}