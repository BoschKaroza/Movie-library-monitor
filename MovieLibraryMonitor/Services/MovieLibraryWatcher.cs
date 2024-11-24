using Microsoft.Extensions.Logging;
using MovieLibraryMonitor.Models;
using MovieLibraryMonitor.Repositories;

namespace MovieLibraryMonitor.Services;

public class MovieLibraryWatcher : IDisposable
{
    private readonly Dictionary<string, FileSystemWatcher> _watchers = new();
    private readonly IEventRepository _eventRepository;
    private readonly ILogger<MovieLibraryWatcher> _logger;
    private readonly string[] _allowedExtensions = { ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".txt" };
    private bool _isDisposed;

    public MovieLibraryWatcher(
        IEventRepository eventRepository,
        ILogger<MovieLibraryWatcher> logger)
    {
        _eventRepository = eventRepository;;
        _logger = logger;
    }

    public void StartWatching(string path)
    {
        if (_watchers.ContainsKey(path))
        {
            _logger.LogWarning("Already watching path: {Path}", path);
            return;
        }

        try
        {
            var watcher = new FileSystemWatcher(path)
            {
                NotifyFilter = NotifyFilters.Attributes
                    | NotifyFilters.CreationTime
                    | NotifyFilters.DirectoryName
                    | NotifyFilters.FileName
                    | NotifyFilters.LastWrite
                    | NotifyFilters.Security
                    | NotifyFilters.Size,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            watcher.Changed += async (s, e) => await HandleEventAsync("Changed", e);
            watcher.Created += async (s, e) => await HandleEventAsync("Created", e);
            watcher.Deleted += async (s, e) => await HandleEventAsync("Deleted", e);
            watcher.Renamed += async (s, e) => await HandleRenamedEventAsync(e);
            watcher.Error += OnError;

            _watchers.Add(path, watcher);
            _logger.LogInformation("Started monitoring folder: {Path}", path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start watching path: {Path}", path);
        }
    }

    public void StopWatching(string path)
    {
        if (_watchers.TryGetValue(path, out var watcher))
        {
            watcher.Dispose();
            _watchers.Remove(path);
            _logger.LogInformation("Stopped monitoring folder: {Path}", path);
        }
    }

    private async Task HandleEventAsync(string eventType, FileSystemEventArgs e)
    {
        if (!_allowedExtensions.Contains(Path.GetExtension(e.FullPath).ToLower()))
            return;

        _logger.LogInformation("{EventType} event detected: {Path}", eventType, e.FullPath);
        
        var fsEvent = new FileSystemEvent
        {
            EventType = eventType,
            FilePath = e.FullPath,
            Timestamp = DateTime.UtcNow
        };
        
        await _eventRepository.LogEventAsync(fsEvent);
    }

    private async Task HandleRenamedEventAsync(RenamedEventArgs e)
    {
        if (!_allowedExtensions.Contains(Path.GetExtension(e.FullPath).ToLower()))
            return;

        _logger.LogInformation("Rename event detected: {OldPath} -> {NewPath}", 
            e.OldFullPath, e.FullPath);
        
        var fsEvent = new FileSystemEvent
        {
            EventType = "Renamed",
            FilePath = e.FullPath,
            OldFilePath = e.OldFullPath,
            Timestamp = DateTime.UtcNow
        };
        
        await _eventRepository.LogEventAsync(fsEvent);
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        _logger.LogError(e.GetException(), "Watcher error occurred");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            foreach (var watcher in _watchers.Values)
            {
                watcher.Dispose();
            }
            _watchers.Clear();
        }

        _isDisposed = true;
    }
}