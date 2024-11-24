namespace MovieLibraryMonitor.Models;

public class DriveEventArgs : EventArgs
{
    public string DriveName { get; }

    public DriveEventArgs(string driveName)
    {
        DriveName = driveName;
    }
}