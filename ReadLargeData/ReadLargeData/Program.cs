using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReadLargeData;
using ReadLargeData.Models;

// Build the host
var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        // Register DbContext with a connection string from appsettings.json
        services.AddDbContext<DataModelContext>(options =>
            options.UseSqlServer(context.Configuration["Data:DataConnection:ConnectionString"]));

        // Register the application entry point
        services.AddScoped<App>();
    })
    .Build();

// Run the application
using var scope = host.Services.CreateScope();
var app = scope.ServiceProvider.GetRequiredService<App>();
app.Run();

// Dispose of the host when done
await host.StopAsync();