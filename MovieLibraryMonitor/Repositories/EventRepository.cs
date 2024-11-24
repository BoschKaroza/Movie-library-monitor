// EventRepository.cs with enhanced logging
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MovieLibraryMonitor.Data;
using MovieLibraryMonitor.Models;

namespace MovieLibraryMonitor.Repositories;

public class EventRepository : IEventRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<EventRepository> _logger;

    public EventRepository(
        ApplicationDbContext dbContext,
        ILogger<EventRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        
        try
        {
            _dbContext.Database.EnsureCreated();
            _logger.LogInformation("Database created/verified successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure database was created");
            throw;
        }
    }

    public async Task LogEventAsync(FileSystemEvent fileSystemEvent)
    {
        try
        {
            _logger.LogDebug("Attempting to add event to context: {EventType}", 
                fileSystemEvent.EventType);
            
            await _dbContext.FileSystemEvents.AddAsync(fileSystemEvent);
            
            _logger.LogDebug("Attempting to save changes to database");
            var changesCount = await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation(
                "Event logged successfully: {EventType} - {FilePath}. Changes saved: {Count}", 
                fileSystemEvent.EventType, 
                fileSystemEvent.FilePath,
                changesCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log event: {EventType} - {FilePath}", 
                fileSystemEvent.EventType, fileSystemEvent.FilePath);
            throw;
        }
    }
}