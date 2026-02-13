using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SmartEvent.Models;

namespace SmartEvent.Data
{
    public static class IdentitySeeder
    {
      
        private const string AdminEmail = "admin@smartevent.com";
        private const string AdminPassword = "Admin@12345";

        public static async Task SeedAsync(IServiceProvider services)
        {
            // Get managers from DI
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // 1) Ensure roles exist
            string[] roles = { "Admin", "Member" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
                    if (!roleResult.Succeeded)
                    {
                        var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                        throw new Exception($"Failed to create role '{role}': {errors}");
                    }
                }
            }

            // 2) Ensure admin user exists
            var adminUser = await userManager.FindByEmailAsync(AdminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = AdminEmail,
                    Email = AdminEmail,
                    EmailConfirmed = true,
                    FullName = "System Admin",
                    EventPreferences = ""
                };


                var userResult = await userManager.CreateAsync(adminUser, AdminPassword);
                if (!userResult.Succeeded)
                {
                    var errors = string.Join("; ", userResult.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to create admin user: {errors}");
                }
            }

            // 3) Ensure admin user is in Admin role
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                var addRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                if (!addRoleResult.Succeeded)
                {
                    var errors = string.Join("; ", addRoleResult.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to add admin user to Admin role: {errors}");
                }
            }
        }
    }
}
