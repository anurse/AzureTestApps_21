﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mvc21
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
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.Use(async (context, next) =>
            {
                string GetSharedFxVersion(Type type)
                {
                    var asmPath = type.Assembly.Location;
                    var versionFile = Path.Combine(Path.GetDirectoryName(asmPath), ".version");

                    var simpleVersion = File.Exists(versionFile) ?
                        File.ReadAllLines(versionFile).Last() :
                        "<unknown>";

                    var infoVersion = type.Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "<unknown>";

                    return $"{simpleVersion} ({infoVersion})";
                }

                if (context.Request.Path.StartsWithSegments("/.runtime-info"))
                {
                    context.Response.ContentType = "text/plain";
                    var aspnetCoreVersion = GetSharedFxVersion(typeof(IApplicationBuilder));
                    var netCoreVersion = GetSharedFxVersion(typeof(string));
                    await context.Response.WriteAsync($"ASP.NET Core Runtime version: {aspnetCoreVersion}{Environment.NewLine}");
                    await context.Response.WriteAsync($".NET Core Runtime version: {netCoreVersion}{Environment.NewLine}");
                }
                else
                {
                    await next();
                }
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
