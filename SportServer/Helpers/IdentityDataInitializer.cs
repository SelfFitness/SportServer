using Microsoft.AspNetCore.Identity;
using SportServer.Data;

namespace SportServer.Helpers
{
    public static class IdentityDataInitializer
    {
        public static async Task SeedData(ApplicationDbContext db, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            await SeedRoles(roleManager);
            await SeedUsers(userManager);
        }

        public static async Task SeedUsers(UserManager<AppUser> userManager)
        {
            var requiredUser = await userManager.FindByNameAsync("admin");
            if (requiredUser is null)
            {
                var user = new AppUser()
                {
                    Email = "admin@admin.com",
                    UserName = "admin@admin.com"
                };
                var res = await userManager.CreateAsync(user, "Admin_1234");
                var result = await userManager.AddToRoleAsync(user, "admin");
            }
        }

        public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("user"))
            {
                IdentityResult result = await roleManager.CreateAsync(new IdentityRole("user"));
            }
            if (!await roleManager.RoleExistsAsync("admin"))
            {
                IdentityResult result = await roleManager.CreateAsync(new IdentityRole("admin"));
            }
        }
    }
}
