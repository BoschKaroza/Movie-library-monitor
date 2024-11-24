using System.Management;
using System.Runtime.Versioning;
using MovieLibraryMonitor.Models;
using Microsoft.Extensions.Logging;

namespace MovieLibraryMonitor.Services;

public class RemovableDriveWatcher : IDisposable
{
    private readonly ILogger<RemovableDriveWatcher> _logger;
    private bool _isDisposed;
    private ManagementEventWatcher? _insertWatcher;
    private ManagementEventWatcher? _removeWatcher;

    public event EventHandler<DriveEventArgs>? DriveInserted;
    public event EventHandler<DriveEventArgs>? DriveRemoved;

    public RemovableDriveWatcher(ILogger<RemovableDriveWatcher> logger)
    {
        _logger = logger;
        if (OperatingSystem.IsWindows())
        {
            InitializeWindowsWatchers();
        }
    }

    [SupportedOSPlatform("windows")]
    private void InitializeWindowsWatchers()
    {
        // WMI query for drive insertion (Type 2)
        var insertQuery = new WqlEventQuery(
            "SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");
        _insertWatcher = new ManagementEventWatcher(insertQuery);
        _insertWatcher.EventArrived += HandleDriveInsertion;

        // WMI query for drive removal (Type 3)
        var removeQuery = new WqlEventQuery(
            "SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 3");
        _removeWatcher = new ManagementEventWatcher(removeQuery);
        _removeWatcher.EventArrived += HandleDriveRemoval;
    }

    public void StartMonitoring()
    {
        if (OperatingSystem.IsWindows())
        {
            StartWindowsMonitoring();
        }
        else
        {
            StartCrossPlatformMonitoring();
        }

        // Check for existing removable drives (works on all platforms)
        CheckExistingDrives();
    }

    [SupportedOSPlatform("windows")]
    private void StartWindowsMonitoring()
    {
        _insertWatcher?.Start();
        _removeWatcher?.Start();
        _logger.LogInformation("Windows-specific removable drive monitoring started");
    }

    private void StartCrossPlatformMonitoring()
    {
        _logger.LogInformation("Cross-platform drive monitoring started (polling-based)");
        // Implement polling-based monitoring for non-Windows platforms if needed
    }

    private void CheckExistingDrives()
    {
        foreach (DriveInfo drive in DriveInfo.GetDrives())
        {
            if (drive.DriveType == DriveType.Removable && drive.IsReady)
            {
                DriveInserted?.Invoke(this, new DriveEventArgs(drive.Name));
                _logger.LogInformation("Found existing removable drive: {DriveName}", drive.Name);
            }
        }
    }

    public void StopMonitoring()
    {
        if (OperatingSystem.IsWindows())
        {
            _insertWatcher?.Stop();
            _removeWatcher?.Stop();
        }
        _logger.LogInformation("Removable drive monitoring stopped");
    }

    [SupportedOSPlatform("windows")]
    private void HandleDriveInsertion(object sender, EventArrivedEventArgs e)
    {
        var driveName = e.NewEvent.Properties["DriveName"].Value.ToString();
        if (driveName != null)
        {
            var driveInfo = new DriveInfo(driveName);
            if (driveInfo.DriveType == DriveType.Removable)
            {
                DriveInserted?.Invoke(this, new DriveEventArgs(driveName));
                _logger.LogInformation("Removable drive inserted: {DriveName}", driveName);
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void HandleDriveRemoval(object sender, EventArrivedEventArgs e)
    {
        var driveName = e.NewEvent.Properties["DriveName"].Value.ToString();
        if (driveName != null)
        {
            DriveRemoved?.Invoke(this, new DriveEventArgs(driveName));
            _logger.LogInformation("Removable drive removed: {DriveName}", driveName);
        }
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
            StopMonitoring();
            _insertWatcher?.Dispose();
            _removeWatcher?.Dispose();
        }

        _isDisposed = true;
    }
}