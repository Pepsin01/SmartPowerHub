using IoTControllerContracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Services;
using SmartPowerHub.Data;
using Serilog;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SmartPowerHub.Database;
using SmartPowerHub.Database.Contexts;
using System.Net;
using System.Net.Security;

namespace SmartPowerHub
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                //.MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddSingleton<DeviceService<IAppliance>>();
            builder.Services.AddSingleton<DeviceService<IBattery>>();
            builder.Services.AddSingleton<DeviceService<IEnergySource>>();
            builder.Services.AddSingleton<PlanningService>();
            builder.Services.AddSingleton<ControllerService>();
            builder.Services.AddMudServices();
            builder.Services.AddSingleton(Log.Logger);

            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7019/")
            });
            builder.WebHost.UseUrls("http://+:7019", "https://+:443");


            builder.Host.UseSerilog();

            // Add DbContext with SQLite
            var connectionString = builder.Configuration.GetConnectionString("DevicesDatabase");
            builder.Services.AddDbContext<DeviceContext>(options =>
                options.UseSqlite(connectionString));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            if (app.Environment.IsDevelopment())
            {
                using var scope = app.Services.CreateScope();
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<DeviceContext>();
                context.Database.Migrate();
                SeedData.SeedMockAppliances(context);
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.Run();
        }
    }
}
