using System.CommandLine;
using VeeamFolderSync;

class Program
{
    static void Main(string[] args)
    {
        var cancelTokenSource = new CancellationTokenSource();
        string sourcePath;
        string replicaPath;
        int syncIntervalSeconds;
        string logPath;

        //Defines required input options for the root command
        var sourcePathOption = new Option<string>("--source", "-s") { Description = "Source folder path", Required = true };
        var replicaPathOption = new Option<string>("--replica", "-r") { Description = "Replica folder path", Required = true };
        var syncIntervalOption = new Option<int>("--interval", "-i") { Description = "Sync interval in seconds", Required = true };
        var logPathOption = new Option<string>("--log", "-l") { Description = "Folder path for the log file", Required = true };

        //Creates a new root command with the defined options
        var rootCommand = new RootCommand ("Veeam Sync App")
        {
            sourcePathOption,
            replicaPathOption,
            syncIntervalOption,
            logPathOption
        };       

        //Defines actions to take when the root command is invoked
        rootCommand.SetAction(async(parseResult,cancelToken)  =>
        {
            //Get paths from the parsed options. No null reference since the options are required.
            sourcePath = Path.GetFullPath(parseResult.GetValue(sourcePathOption)!);
            replicaPath = Path.GetFullPath(parseResult.GetValue(replicaPathOption)!)!;
            logPath = Path.GetFullPath(parseResult.GetValue(logPathOption)!);
            syncIntervalSeconds = parseResult.GetValue(syncIntervalOption);

            //Check existence of the source folder
            if (!Directory.Exists(sourcePath))
            {
                Console.WriteLine(Environment.NewLine + "The source folder not found.");
                return;
            }

            //Check existence of the replica folder. If negative create a new one.
            if (!Directory.Exists(replicaPath))
            {
                Console.WriteLine(Environment.NewLine + $"The replica folder not found. Creating a new one at {replicaPath}");
                Directory.CreateDirectory(replicaPath);
            }   

            //Check existence of the log folder. If negative create a new one.
            if (!Directory.Exists(logPath))
            {
                Console.WriteLine(Environment.NewLine + $"The log folder not found. Creating a new one at {logPath}");
                Directory.CreateDirectory(logPath);
            }

            //Create a Logger instance
            Logger logger = new(logPath);

            //Intro to be displayed
            Console.WriteLine(
                Environment.NewLine
                + "================================================="
                + Environment.NewLine 
                + "Veeam Sync App" 
                + Environment.NewLine 
                + "=================================================" 
                + Environment.NewLine
                + "Setup:" 
                + Environment.NewLine);
            Console.WriteLine($"Source: {sourcePath}");
            Console.WriteLine($"Replica: {replicaPath}");
            Console.WriteLine($"Interval: {syncIntervalSeconds} seconds");
            Console.WriteLine($"Log: {logPath}");

            //run the task that monitors for cancellation, returned task can be discarded
            _ = Task.Run(() => MonitorCancellationTask(cancelTokenSource, logger), cancelTokenSource.Token);

            //the main synchronization work
            await Synchronizer.SynchronizeFoldersPeriodically(sourcePath, replicaPath, syncIntervalSeconds, logger, cancelTokenSource.Token);
        });

        //Parses the options/arguments and invokes the actions set in the rootCommand's SetAction()
        rootCommand.Parse(args).Invoke();

    }

    private static void MonitorCancellationTask(CancellationTokenSource cancelTokenSource, Logger logger)
    {
        Console.WriteLine(Environment.NewLine + "Type 'exit' to quit the program." + Environment.NewLine);
        
        //Waits for the user to type 'exit', than it initiates cancellation and writes it to the log file.
        while (true)
        {
            var input = Console.ReadLine()?.Trim();
            if (input?.ToLowerInvariant().Equals("exit") == true)
            {
                cancelTokenSource.Cancel();
                logger.Log("Sync stopped by user" + Environment.NewLine);
                break;
            }
        }
    }
}



