using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Student_Management.Data;
using Student_Management.Models;
using System.Runtime;

namespace Student_Management
{

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container.
            builder.Services.AddControllersWithViews();
            //Configure Entity Framework Core
            var connectionString = builder.Configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Connection string 'SQLServerIdentityConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            //Configuration Identity Services
            //Configurre the Custom User and Role Class
            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(
                options =>
                {
                    // Password settings
                    options.Password.RequireDigit = true;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequiredUniqueChars = 4;
                    // Other settings can be configured here
                })
                .AddEntityFrameworkStores<ApplicationDbContext>();
            // Configure the Application Cookie settings
            builder.Services.ConfigureApplicationCookie(options =>
            {
                // If the LoginPath isn't set, ASP.NET Core defaults the path to /Account/Login.
                options.LoginPath = "/Account/Login"; // Set your login path here
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
            //Configuring Authentication Middleware to the Request Pipeline
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.Run();

            // Configure the Application Cookie settings
            builder.Services.ConfigureApplicationCookie(options =>
            {
                // If the LoginPath isn't set, ASP.NET Core defaults the path to /Account/Login.
                options.LoginPath = "/Account/Login"; // Set your login path here

                //// If the AccessDenied isn't set, ASP.NET Core defaults the path to /Account/AccessDenied
                //options.AccessDeniedPath = "/Account/AccessDenied"; // Set your access denied path here
            });
        }
    }
}