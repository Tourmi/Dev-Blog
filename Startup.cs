using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dev_Blog.Config;
using Dev_Blog.Data;
using Dev_Blog.Models;
using Dev_Blog.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Dev_Blog
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
            });
            services.AddAuthentication("Cookies");

            services.Configure<IdentityOptions>(options =>
            {
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            });
            services.Configure<PasswordHasherOptions>(options =>
            {
                options.IterationCount = 150000;
            });

            services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = long.MaxValue;
            });

            services.AddDbContext<BlogDBContext>();

            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<BlogDBContext>()
                .AddUserManager<UserManager<User>>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(5);
                options.SlidingExpiration = true;
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
            });

            services.AddHttpClient();

            services.AddTransient<EmailSenderService>();
            services.AddSingleton<PostViewCountUpdaterService>();
            services.AddSingleton<EmailSchedulerService>();
            services.Configure<EmailSenderConfig>(Configuration);
            services.Configure<DefaultUserConfig>(Configuration);
            services.Configure<ReCaptchaConfig>(Configuration);

            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            })
                .AddRazorRuntimeCompilation();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Post}/{action=Index}");
            });

            var _ = app.ApplicationServices.GetService<EmailSchedulerService>();

            using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
            using var context = serviceScope.ServiceProvider.GetRequiredService<BlogDBContext>();
            context.Database.Migrate();

            await checkAndCreateDefaultRoles(serviceScope);
            var optionsAccessor = app.ApplicationServices.GetService<IOptions<DefaultUserConfig>>();
            await checkAndCreateDefaultUser(serviceScope, optionsAccessor.Value);
        }

        private async Task checkAndCreateDefaultRoles(IServiceScope serviceScope)
        {
            var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var adminRole = await roleManager.FindByNameAsync("Admin");
            if (adminRole == null)
            {
                var role = new IdentityRole
                {
                    Name = "Admin"
                };
                await roleManager.CreateAsync(role);
            }

            var authorRole = await roleManager.FindByNameAsync("Author");
            if (authorRole == null)
            {
                var role = new IdentityRole
                {
                    Name = "Author"
                };
                await roleManager.CreateAsync(role);
            }
        }

        private async Task checkAndCreateDefaultUser(IServiceScope serviceScope, DefaultUserConfig options)
        {

            var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var defaultAdminUser = await userManager.FindByNameAsync(options.DefaultUserUsername);
            if (defaultAdminUser == null)
            {
                User user = new User()
                {
                    UserName = options.DefaultUserUsername,
                    Email = options.DefaultUserEmail,
                    DisplayName = options.DefaultUserDisplayName
                };

                var createResult = await userManager.CreateAsync(user, options.DefaultUserPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                    await userManager.AddToRoleAsync(user, "Author");
                }
            }
        }
    }
}
