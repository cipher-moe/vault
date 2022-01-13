using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OsuSharp;
using vault.Databases;
using vault.Services;

namespace vault
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
            var httpClient = new HttpClient();
            services.AddLogging();
            services.AddRazorPages();
            services.AddSession(session => session.IdleTimeout = TimeSpan.FromMinutes(30));
            services.AddSingleton(httpClient);
            services.AddSingleton(
                new OsuClient(new OsuSharpConfiguration{
                    ApiKey = Environment.GetEnvironmentVariable("OSU_API_KEY"),
                    HttpClient = httpClient
                })
            );
            services.AddDbContext<ReplayDbContext>(builder =>
            {
                var connectionString = Environment.GetEnvironmentVariable("MARIADB_CONNECTION_STRING")!;
                builder
                    .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors();
            });
            
            services.AddDbContext<BeatmapDbContext>(builder =>
            {
                var connectionString = Environment.GetEnvironmentVariable("MARIADB_CONNECTION_STRING")!;
                builder
                    .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors();
            });

            services.AddHostedService<MostPlayedMapsHostedService>();
            services.AddSingleton<MostPlayedMapsService>();
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
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSession();
            if (env.IsDevelopment()) app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
