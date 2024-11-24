using MovieLibraryMonitor.Models;

namespace MovieLibraryMonitor.Repositories;

public interface IEventRepository
{
    Task LogEventAsync(FileSystemEvent fileSystemEvent);
}