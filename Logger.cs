namespace VeeamFolderSync;

public class Logger(string logPath) //Primary constructor
{
    private readonly string logfileFullPath = Path.Combine(logPath, "VeeamFolderSync_Log.txt");
    private static readonly Lock _lock = new(); //Lock prevents two tasks to write at the same time - folder sync task and cancellation monitoring task

    public void Log(string message, LogTo logTo = LogTo.ConsoleAndLogFile)
    {
        string logEntry = $"{DateTime.Now}  {message}";

        lock (_lock)
        {
            if(logTo is LogTo.LogFile or LogTo.ConsoleAndLogFile) 
                    File.AppendAllText(logfileFullPath, logEntry + Environment.NewLine); //Good enough for infrequent writing
            if(logTo is LogTo.Console or LogTo.ConsoleAndLogFile) 
                    Console.WriteLine(message);
        }
    }

    public void LogBlankLine()
    {
        lock (_lock)
        {
            File.AppendAllText(logfileFullPath, Environment.NewLine); //Adds blank line between blocks
        }
    }

    public enum LogTo
    {
        Console,
        LogFile,
        ConsoleAndLogFile
    }
}