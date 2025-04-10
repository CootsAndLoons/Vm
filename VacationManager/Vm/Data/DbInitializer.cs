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

            // Seed project
            if (!context.Projects.Any())
            {
                context.Projects.Add(new Project
                {
                    Name = "Vacation System",
                    Description = "Internal vacation tracking system"
                });
                await context.SaveChangesAsync();
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

            // Create Team Lead
            if (!await userManager.Users.AnyAsync(u => u.Email == "lead@company.com"))
            {
                var lead = new ApplicationUser
                {
                    UserName = "lead@company.com",
                    Email = "lead@company.com",
                    FirstName = "Team",
                    LastName = "Lead",
                    EmailConfirmed = true,
                    RoleId = context.Roles.First(r => r.Name == "Team Lead").Id
                };

                if ((await userManager.CreateAsync(lead, "LeadPassword123!")).Succeeded)
                    await userManager.AddToRoleAsync(lead, "Team Lead");
            }

            // Create Developer
            if (!await userManager.Users.AnyAsync(u => u.Email == "dev@company.com"))
            {
                var dev = new ApplicationUser
                {
                    UserName = "dev@company.com",
                    Email = "dev@company.com",
                    FirstName = "Dev",
                    LastName = "One",
                    EmailConfirmed = true,
                    RoleId = context.Roles.First(r => r.Name == "Developer").Id
                };

                if ((await userManager.CreateAsync(dev, "DevPassword123!")).Succeeded)
                    await userManager.AddToRoleAsync(dev, "Developer");
            }

            // Create Unassigned
            if (!await userManager.Users.AnyAsync(u => u.Email == "new@company.com"))
            {
                var unassigned = new ApplicationUser
                {
                    UserName = "new@company.com",
                    Email = "new@company.com",
                    FirstName = "New",
                    LastName = "User",
                    EmailConfirmed = true,
                    RoleId = context.Roles.First(r => r.Name == "Unassigned").Id
                };

                if ((await userManager.CreateAsync(unassigned, "NewPassword123!")).Succeeded)
                    await userManager.AddToRoleAsync(unassigned, "Unassigned");
            }
        }
    }
}
