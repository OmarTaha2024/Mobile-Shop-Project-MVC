using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace MobileShopSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
           .AddEntityFrameworkStores<ApplicationDbContext>()
           .AddDefaultTokenProviders();
            builder.Services.AddScoped<CartCountFilter>();

            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.AddService<CartCountFilter>();
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Mobiles}/{action=Index}/{id?}");

         /*   using (var scope = app.Services.CreateScope())
           {
               var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
               var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

               string[] roles = { "Admin", "User" };

               foreach (var role in roles)
               {
                   if (!await roleManager.RoleExistsAsync(role))
                   {
                       await roleManager.CreateAsync(new IdentityRole(role));
                   }
               }


               string adminEmail = "admin@MobileShop.com";
               string adminPassword = "Admin@123";

               if (await userManager.FindByEmailAsync(adminEmail) == null)
               {
                   var admin = new ApplicationUser
                   {
                       UserName = adminEmail,
                       Email = adminEmail,
                       FirstName = "Main",
                       LastName = "Admin",
                       Address = "Cairo",
                       CreatedAt = DateTime.Now
                   };

                   var result = await userManager.CreateAsync(admin, adminPassword);
                   if (result.Succeeded)
                   {
                       await userManager.AddToRoleAsync(admin, "Admin");
                   }
               }
           }
         */
            app.Run();
        }
    }
}
