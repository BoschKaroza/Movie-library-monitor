using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MovieLibraryMonitor.Data;
using MovieLibraryMonitor.Repositories;

using MovieLibraryMonitor.Services;
using Serilog;

namespace MovieLibraryMonitor;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.Title = "Movie Library Monitor";
        
        try
        {
            ShowWelcomeMessage();
            
            var host = CreateHostBuilder(args).Build();
            
            // Get the required services
            var driveWatcher = host.Services.GetRequiredService<RemovableDriveWatcher>();
            var movieWatcher = host.Services.GetRequiredService<MovieLibraryWatcher>();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            
            // Setup drive monitoring
            driveWatcher.DriveInserted += (sender, e) => 
            {
                Console.WriteLine($"\nDrive inserted: {e.DriveName}");
                movieWatcher.StartWatching(e.DriveName);
            };
            
            driveWatcher.DriveRemoved += (sender, e) => 
            {
                Console.WriteLine($"\nDrive removed: {e.DriveName}");
                movieWatcher.StopWatching(e.DriveName);
            };
            
            // Start monitoring
            driveWatcher.StartMonitoring();
            
            logger.LogInformation("Application started successfully");
            
            // Keep the application running until user exits
            await RunApplicationLoop(host);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Fatal error: {ex.Message}");
            Console.ResetColor();
            Environment.Exit(1);
        }
    }
    
    private static void ShowWelcomeMessage()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"
╔══════════════════════════════════════════╗
║        Movie Library Monitor             ║
║        Version 1.0.0                     ║
╚══════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine("\nMonitoring removable drives for movie files...");
        ShowHelp();
    }
    
    private static void ShowHelp()
    {
        Console.WriteLine("\nAvailable commands:");
        Console.WriteLine("  help    - Show this help message");
        Console.WriteLine("  clear   - Clear the console");
        Console.WriteLine("  drives  - List currently monitored drives");
        Console.WriteLine("  exit    - Exit the application");
        Console.WriteLine("\nPress 'H' at any time to show this help message.");
    }
    
    private static async Task RunApplicationLoop(IHost host)
    {
        bool exitRequested = false;
        
        while (!exitRequested)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                switch (char.ToUpper(key.KeyChar))
                {
                    case 'H':
                        ShowHelp();
                        break;
                        
                    case 'C':
                        Console.Clear();
                        ShowWelcomeMessage();
                        break;
                        
                    case 'D':
                        ShowCurrentDrives();
                        break;
                        
                    case 'Q':
                    case 'X':
                        exitRequested = true;
                        break;
                }
            }
            
            await Task.Delay(100); // Reduce CPU usage
        }
        
        Console.WriteLine("\nShutting down...");
        await host.StopAsync();
    }
    
    private static void ShowCurrentDrives()
    {
        Console.WriteLine("\nCurrently connected removable drives:");
        var drives = DriveInfo.GetDrives()
            .Where(d => d.DriveType == DriveType.Removable && d.IsReady);
            
        if (!drives.Any())
        {
            Console.WriteLine("  No removable drives connected.");
            return;
        }
        
        foreach (var drive in drives)
        {
            Console.WriteLine($"  {drive.Name} - {drive.VolumeLabel} ({FormatBytes(drive.TotalFreeSpace)} free of {FormatBytes(drive.TotalSize)})");
        }
    }
    
    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;
        
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        
        return $"{size:0.##} {sizes[order]}";
    }

     private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog((context, configuration) =>
            {
                configuration
                    .WriteTo.Console()
                    .WriteTo.File("logs/movielibrarymonitor.log", 
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
            })
            .ConfigureServices((context, services) =>
            {
                // Updated DbContext registration
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(context.Configuration.GetConnectionString("DefaultConnection") ?? 
                        "Data Source=MovieLibraryMonitor.db"));
                
                // Keep your existing registrations
                services.AddScoped<IEventRepository, EventRepository>();
                services.AddSingleton<RemovableDriveWatcher>();
                services.AddSingleton<MovieLibraryWatcher>();
            });

}