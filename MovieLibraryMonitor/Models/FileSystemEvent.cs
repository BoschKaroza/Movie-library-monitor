using System;

namespace MovieLibraryMonitor.Models;

public class FileSystemEvent
{
    public int Id { get; set; }                
    public required string EventType { get; set; }  
    public required string FilePath { get; set; }   
    public string? OldFilePath { get; set; }        
    public DateTime Timestamp { get; set; }         
}
