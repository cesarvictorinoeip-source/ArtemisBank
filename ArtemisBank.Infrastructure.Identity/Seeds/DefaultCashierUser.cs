using ArtemisBank.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace ArtemisBank.Infrastructure.Identity.Seeds
{
    public static class DefaultCashierUser
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager)
        {
            ApplicationUser user = new()
            {
                FirstName = "Cajero",
                LastName = "Artemis",
                IdCard = "00000000002",
                UserName = "cajeroArtemis",
                Email = "cajero@artemisbank.com",
                UserType = "Cajero",
                IsActive = true,
                EmailConfirmed = true
            };

            if (userManager.Users.All(u => u.UserName != user.UserName))
            {
                var existingUser = await userManager.FindByEmailAsync(user.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(user, "Cajero@123");
                    await userManager.AddToRoleAsync(user, "Cajero");
                }
            }
        }
    }
}