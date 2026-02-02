## VeeamFolderSync
VeeamFolderSync is a simple **one-way folder** synchronization console application for Windows.

It periodically synchronizes a source folder to a replica folder.
New or modified file and folders in the source are copied to the replica.
Files and folders deleted from the source are removed from the replica.

## Features
- One-way synchronization (source → replica)
- Synchronizes files and folders
- Periodic sync using a configurable interval
- MD5-based comparison to detect changes in files
- Logging to text file
- Graceful exit using "exit" or Ctrl+C

## Running the app:
Run the application from Command Prompt
```
Usage: VeeamFolderSync [source] [replica] [interval] [log]

  -s, --source      (REQUIRED)  Source folder path
  -r, --replica     (REQUIRED)  Replica folder path
  -i, --interval    (REQUIRED)  Sync interval in seconds
  -l, --log         (REQUIRED)  Folder path for the log file
  -?, -h, --help                Show help and usage information

Example: VeeamFolderSync "C:\Source" "C:\Replica" 10 "C:\Log"
```
Type "exit" to stop the application. Alternatively use Ctrl+C.

## Logging
Sync activity is logged to *VeeamFolderSync_Log.txt* (path to be provided by user).

## Published
Provided as a source code or a single exe file release.

Requirements:
- For building from source code: .NET SDK 10 (probably compatible with 8+)
- For .exe: Win 10/11 64-bit

## Author
TK
