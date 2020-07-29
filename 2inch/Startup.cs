using System;
using _2inch.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace _2inch
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("STC SHORTENER");
                });

                endpoints.MapControllerRoute(
                    name: "NotFoundRoute",
                    pattern: "/404",
                    defaults: new { controller = "NotFound", action = "Index" });

                endpoints.MapControllerRoute(
                    name: "Admin",
                    pattern: "/Admin",
                    defaults: new { controller = "Admin", action = "Login" });

                endpoints.MapGet("/{name:alpha}", async context =>
                {
                    var name = context.Request.RouteValues["name"];
                    string url = Convert.ToString(name);
                    string final = Database.getLongLink(url);
                    if(final == null) {
                        context.Response.Redirect("/404");
                        return;
                    }
                    context.Response.Redirect(final);
                });
            });
        }
    }
}
