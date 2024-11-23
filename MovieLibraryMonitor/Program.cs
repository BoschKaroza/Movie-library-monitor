namespace MovieLibraryMonitor;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Starting the Movie Library Monitor...");

        // Path to monitor (Movie-library)
        string folderPath = @"C:\Users\HP\Videos\Movies";

        using var watcher = new FileSystemWatcher(folderPath);

        // Configure the watcher
        watcher.NotifyFilter = NotifyFilters.Attributes
                             | NotifyFilters.CreationTime
                             | NotifyFilters.DirectoryName
                             | NotifyFilters.FileName
                             | NotifyFilters.LastAccess
                             | NotifyFilters.LastWrite
                             | NotifyFilters.Security
                             | NotifyFilters.Size;

        // Add event handlers
        watcher.Changed += OnChanged;
        watcher.Created += OnCreated;
        watcher.Deleted += OnDeleted;
        watcher.Renamed += OnRenamed;
        watcher.Error += OnError;

        // Set a filter to monitor specific file types (e.g., ".mp4" for movies)
        watcher.Filter = "*.*";
        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;

        Console.WriteLine($"Monitoring changes in: {folderPath}");
        Console.WriteLine("Press enter to exit.");
        Console.ReadLine();
    }

    private static void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Changed)
        {
            return;
        }
        Console.WriteLine($"File changed: {e.FullPath}");
    }

    private static void OnCreated(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine($"File created: {e.FullPath}");
    }

    private static void OnDeleted(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine($"File deleted: {e.FullPath}");
    }

    private static void OnRenamed(object sender, RenamedEventArgs e)
    {
        Console.WriteLine($"File renamed:");
        Console.WriteLine($"    Old: {e.OldFullPath}");
        Console.WriteLine($"    New: {e.FullPath}");
    }

    private static void OnError(object sender, ErrorEventArgs e)
    {
        PrintException(e.GetException());
    }

    private static void PrintException(Exception? ex)
    {
        if (ex != null)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Stack trace:");
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine();
            PrintException(ex.InnerException);
        }
    }
}
