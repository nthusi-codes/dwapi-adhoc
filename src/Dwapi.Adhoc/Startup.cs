using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActiveQueryBuilder.Web.Core;
using ActiveQueryBuilder.Web.Server.Infrastructure.Providers;
using Dwapi.Adhoc.Providers;
using Flexmonster.DataServer.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dwapi.Adhoc
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }
        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            Environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Active Query Builder requires support for Session HttpContext.
            services.AddSession();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Register providers
            services.AddScoped<IQueryBuilderProvider, QueryBuilderMsSqlStoreProvider>();
            services.AddScoped<IQueryTransformerProvider, QueryTransformerMsSqlStoreProvider>();
            services.AddScoped<IAdhocManager, AdhocManager>();

            services.AddActiveQueryBuilder();
           // services.ConfigureFlexmonsterOptions(Configuration);
            // services.AddFlexmonsterApi();
            services.AddControllersWithViews();
            services.AddCors();
            services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = long.MaxValue;
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            // Active Query Builder requires support for Session HttpContext.
            app.UseSession();

            // Active Query Builder server requests handler.
            app.UseActiveQueryBuilder();

            app.UseStaticFiles();

            app.UseRouting();

            // app.UseAuthorization();

            // other configurations
            app.UseCors(builder => {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });



            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
