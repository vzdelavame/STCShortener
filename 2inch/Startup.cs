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


            app.UseEndpoints(endpoints =>
            {                      

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });

                endpoints.MapGet("/admin", async context =>
                {
                    await context.Response.WriteAsync("ADMIN PAGE!");
                });

                endpoints.MapGet("/{name:alpha}", async context =>
                {
                    var name = context.Request.RouteValues["name"];
                    string url = Convert.ToString(name);
                    string final = Database.getLongLink(url);
                    if(final == null) {
                        await context.Response.WriteAsync("404");
                        return;
                    }
                    context.Response.Redirect(final);
                });
            });
        }
    }
}
