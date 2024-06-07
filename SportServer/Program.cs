using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SportServer.Data;
using SportServer.Helpers;
using SportServer.Models;
using SportServer.Predictors;
using System.Text.Json;

namespace SportServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration["SPORT_CONNECTION"] ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            var issuer = builder.Configuration["ISSUER"] ?? throw new InvalidOperationException("Issuer not found.");
            var secretKey = builder.Configuration["SECRET_KEY"] ?? throw new InvalidOperationException("Secret key not found.");
            var audience = builder.Configuration["AUDIENCE"] ?? throw new InvalidOperationException("Audience not found.");
            var jwtOptions = new JwtOptions(secretKey, issuer, audience);
            builder.Services.AddSingleton(options => jwtOptions);
            builder.Services.AddScoped<WeightPredictor>();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = jwtOptions.SymmetricSecurityKey,
                    ValidateIssuerSigningKey = true,
                };
            });

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<AppUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 2;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddControllersWithViews();

            builder.Services.Configure<SecurityStampValidatorOptions>(options =>
            {
                // enables immediate logout, after updating the user's security stamp.
                options.ValidationInterval = TimeSpan.Zero;
            });
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.LoginPath = "/users/login";
                options.AccessDeniedPath = "/";
            });
            var app = builder.Build();

            // Initialize data
            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Database.EnsureCreatedAsync();
                var plangroups = await db.PlanGroups
                    .Include(x => x.Plans)
                    .ThenInclude(x => x.ExerciseParts)
                    .ThenInclude(x => x.Exercise)
                    .ToListAsync();
                db.PlanGroups.RemoveRange(plangroups);
                var exercices = await db.ExerciseParts.Include(x => x.Plans)
                    .Include(x => x.Exercise)
                    .ToListAsync();
                db.ExerciseParts.RemoveRange(exercices);
                var plans = await JsonSerializer.DeserializeAsync<IEnumerable<PlanGroup>>(File.OpenRead("plans.json"));
                var exs = plans.SelectMany(x => x.Plans.SelectMany(x => x.ExerciseParts)).GroupBy(x => x.Exercise.Title).Select(x => x.First()).ToList();
                foreach (var plangroup in plans)
                {
                    foreach (var plan in plangroup.Plans)
                    {
                        var exParts = new List<ExercisePart>();
                        foreach (var ex in plan.ExerciseParts)
                        {
                            var sExs = exs.First(x => x.Exercise.Title == ex.Exercise.Title);
                            exParts.Add(sExs);
                        }
                        plan.ExerciseParts = exParts;
                    }
                }
                await db.PlanGroups.AddRangeAsync(plans);
                await db.SaveChangesAsync();
                await IdentityDataInitializer.SeedRoles(roleManager);
                await IdentityDataInitializer.SeedUsers(userManager);
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            
            app.MapControllers();

            app.Run();
        }
    }
}
