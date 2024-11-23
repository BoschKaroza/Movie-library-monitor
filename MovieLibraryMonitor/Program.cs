using MovieLibraryMonitor.Services;

namespace MovieLibraryMonitor;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Starting the Movie Library Monitor...");

        // Path to monitor (Movie-library)
        string folderPath = @"C:\Users\HP\Videos\Movies";

        // Instantiate the MovieLibraryWatcher
        var watcher = new MovieLibraryWatcher(folderPath);
        watcher.Start();

        Console.WriteLine("Press enter to exit.");
        Console.ReadLine();
    }
}
