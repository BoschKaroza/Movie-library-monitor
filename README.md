# Movie Library Monitor

**Author:** Bosch Karoza (Dar es Salaam, Tanzania)  
**Version:** 1.0.0

## Description

Movie Library Monitor is a lightweight console application designed to track file activity on connected external or removable drives. It helps monitor unauthorized changes to your files, such as additions, deletions, or modifications, making it ideal for safeguarding sensitive data like movie collections.

## Features

- **Real-Time Monitoring:** Continuously scans connected drives for file changes.
- **Detailed Logs:** Records activity with timestamps, file names, and sizes.
- **Simple & Effective:** Lightweight and easy to use, with no unnecessary overhead.

## Planned Features

- **Background Service:** Enable uninterrupted monitoring in the background.
- **Graphical Interface:** Visualize logs and generate detailed activity reports.
- **Enhanced Security:** Protect sensitive data with advanced security options.

## Requirements

- **.NET Core 8:** [Download here](https://dotnet.microsoft.com/download)
- **Entity Framework Core (EF Core):** Included with .NET Core.
- **SQLite Database:** For storing tracked file activity.

## Getting Started

1. **Clone the Repository:**

   ```bash
   git clone <repository-url>
   cd <repository-folder>
   ```

2. **Install Dependencies:**

   ```bash
   dotnet restore
   ```

3. **Run the Application:**
   ```bash
   dotnet run
   ```

### On First Run

The application will create the following:

- `/logs`: Stores logs of detected changes.
- `MovieLibraryMonitor.db`: SQLite database for file activity records.
- `obj` and `bin`: Build and output directories.

## Feedback and Contributions

While Movie Library Monitor is not open-source yet, feedback and suggestions are welcome! Please feel free to share ideas or report any issues.

## License

This software is proprietary and not currently open-source.

```

```
