using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace _2inch
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
