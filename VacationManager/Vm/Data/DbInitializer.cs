using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vm.Models;

namespace Vm.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context,
                                              UserManager<ApplicationUser> userManager,
                                              RoleManager<IdentityRole> roleManager)
        {
            // Apply pending migrations first
            await context.Database.MigrateAsync();

            // Seed roles
            if (!context.Roles.Any())
            {
                // Create "Unassigned" first!
                var roles = new List<Role>
            {
                new Role { Name = "Unassigned" },  // Must be first!!!! DO NOT CHANGE THIS MFS
                new Role { Name = "CEO" },
                new Role { Name = "Team Lead" },
                new Role { Name = "Developer" }
            };

                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();

                // Create Identity roles
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role.Name))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role.Name));
                    }
                }
            }

            // Create CEO user
            if (!await userManager.Users.AnyAsync(u => u.Email == "ceo@company.com"))
            {
                var ceoUser = new ApplicationUser
                {
                    UserName = "ceo@company.com",
                    Email = "ceo@company.com",
                    FirstName = "Admin",
                    LastName = "CEO",
                    EmailConfirmed = true,
                    RoleId = context.Roles.First(r => r.Name == "CEO").Id
                };

                var result = await userManager.CreateAsync(ceoUser, "CeoPassword123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(ceoUser, "CEO");
                }
            }
        }

    }
}
