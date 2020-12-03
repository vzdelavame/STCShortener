using System;
using _2inch.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace _2inch
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(options => {
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                
            }).AddCookie(options => { 
                options.LoginPath = "/Admin/Index";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
            });

            services.AddControllersWithViews().AddRazorPagesOptions(options => {
                options.Conventions.AuthorizeFolder("/Admin/"); 
                options.Conventions.AllowAnonymousToPage("/Admin/Index");
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "NotFoundRoute",
                    pattern: "/404",
                    defaults: new { controller = "NotFound", action = "Index" });

                endpoints.MapControllerRoute(
                    name: "Index",
                    pattern: "/Index",
                    defaults: new { controller = "MainIndex", action = "Index" });

                endpoints.MapControllerRoute(
                    name: "Admin",
                    pattern: "{controller=Admin}/{action=Login}");

                endpoints.MapGet("/{name:required}", async context =>
                {
                    var name = context.Request.RouteValues["name"];
                    string url = Convert.ToString(name);
                    string final = await Database.GetLongString(url);
                    if (final == null)
                    {
                        context.Response.Redirect("/404");
                        return;
                    }
                    context.Response.Redirect(final);
                });

#pragma warning disable CS1998
                endpoints.MapGet("/admin", async context =>
                {
                    context.Response.Redirect("/Admin/Login");
                });      

                endpoints.MapGet("/Admin/{name:required}", async context =>
                {
                    context.Response.Redirect("/admin/NotFoundPage");
                });

                endpoints.MapGet("/", async context =>
                {
                    context.Response.Redirect("/Index");
                });
#pragma warning restore CS1998
            });
        }
    }
}
