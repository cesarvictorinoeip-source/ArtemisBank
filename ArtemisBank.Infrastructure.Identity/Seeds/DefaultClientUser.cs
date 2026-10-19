using ArtemisBank.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace ArtemisBank.Infrastructure.Identity.Seeds
{
    public static class DefaultClientUser
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager)
        {
            ApplicationUser user = new()
            {
                FirstName = "Cliente",
                LastName = "Artemis",
                IdCard = "00000000003",
                UserName = "clienteArtemis",
                Email = "cliente@artemisbank.com",
                UserType = "Cliente",
                IsActive = true,
                EmailConfirmed = true
            };

            if (userManager.Users.All(u => u.UserName != user.UserName))
            {
                var existingUser = await userManager.FindByEmailAsync(user.Email);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(user, "Cliente@123");
                    await userManager.AddToRoleAsync(user, "Cliente");
                }
            }
        }
    }
}