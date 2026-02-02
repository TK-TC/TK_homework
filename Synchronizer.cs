namespace VeeamFolderSync;
using ItemTypeEnum = Utilities.ItemTypeEnum; 

public class Synchronizer
{
    public static async Task SynchronizeFoldersPeriodically(string sourceFolderPath, string replicaFolderPath, int syncIntervalSeconds, Logger logger, CancellationToken cancellationToken)
    {
        logger.Log($"***Sync initiated***{Environment.NewLine}Source: {sourceFolderPath}{Environment.NewLine}Replica: {replicaFolderPath}{Environment.NewLine}Interval: {syncIntervalSeconds} seconds{Environment.NewLine}", logTo: Logger.LogTo.LogFile);

        while(!cancellationToken.IsCancellationRequested) //check that cancellation has not already been requested by the cancellation monitoring task
        {
            int diffCounter = 0;

            //The main sync work
            //No cancel token, let it run to the end
            //Returns diff counter to check if any sync operations happened
            diffCounter = await Task.Run(() => SynchronizeFolders(sourceFolderPath, replicaFolderPath, diffCounter, logger), CancellationToken.None); 

            if(diffCounter == 0)
            {
                logger.Log("Folders up-to-date");
            }
            else
            {
                logger.LogBlankLine(); //to add blank line after block of sync records
            }

            try 
            {
                await Task.Delay(TimeSpan.FromSeconds(syncIntervalSeconds), cancellationToken);  //can react to cancellation during delay and cancels as well, catches an exception
            }
            catch(OperationCanceledException)
            {
                break;
            };
        }
    }

    private static int SynchronizeFolders(string sourceFolderPath, string replicaFolderPath, int diffCounter, Logger logger)
    {       
        //Copy from source to replica
        foreach(string sourceItemPath in Directory.GetFileSystemEntries(sourceFolderPath))
        {
            ItemTypeEnum itemType = Utilities.CheckIfFileOrFolder(sourceItemPath);

            string itemName = Path.GetFileName(sourceItemPath)!;
            string replicaItemPath = Path.Combine(replicaFolderPath, itemName);

            switch (itemType)
            {
                //case file - ensure the file exists or overwrite it if modified
                case ItemTypeEnum.File:

                        if(!File.Exists(replicaItemPath) || !Utilities.HashesAreEqual(sourceItemPath, replicaItemPath))
                        {   
                            diffCounter++; 

                            try
                            {
                                File.Copy(sourceItemPath, replicaItemPath, overwrite: true);
                                logger.Log($"Source file synchronized: {sourceItemPath}");            
                            }
                            catch (UnauthorizedAccessException)
                            {
                                logger.Log($"Source file not synced: {sourceItemPath} - Access denied, check permissions");
                                continue;
                            }
                        }
                        break;

                //case directory - check the directory exists recursively sync it's content
                case ItemTypeEnum.Directory:

                        if(!Directory.Exists(replicaItemPath))
                        {
                            diffCounter++; 

                            Directory.CreateDirectory(replicaItemPath);
                            logger.Log($"Replica folder created: {replicaItemPath}");  
                        }
                        
                        SynchronizeFolders(sourceItemPath, replicaItemPath, diffCounter, logger); //recursive sync for folders

                        break;           
            }
        }

        //Delete replica files if source removed
        foreach(string replicaItemPath in Directory.GetFileSystemEntries(replicaFolderPath))
        {
            ItemTypeEnum itemType = Utilities.CheckIfFileOrFolder(replicaItemPath);

            string filename = Path.GetFileName(replicaItemPath);
            string sourceItemPath = Path.Combine(sourceFolderPath, filename);

            switch (itemType)
            {
                //case file - if the file not in the source folder, delete it from the replica as well
                case ItemTypeEnum.File:

                        if(!File.Exists(sourceItemPath))
                        {
                            diffCounter++;

                            try
                            {
                                File.Delete(replicaItemPath);
                                logger.Log($"Replica file deleted: {replicaItemPath}");
                            }
                            catch (UnauthorizedAccessException)
                            {
                                logger.Log($"Replica file not deleted: {replicaItemPath} - Access denied, check permissions");
                                continue;
                            }       
                        }
                        break;

                //case directory - if the folder not in the source folder, recursively delete it from the replica as well
                case ItemTypeEnum.Directory:

                        if(!Directory.Exists(sourceItemPath))
                        {
                            diffCounter++;

                            try
                            {
                                Directory.Delete(replicaItemPath, recursive: true); //recursively deletes all files/folders from the deleted folder
                                logger.Log($"Replica folder deleted: {replicaItemPath}");
                                break;
                            }
                            catch (UnauthorizedAccessException)
                            {
                                logger.Log($"Replica folder not deleted: {replicaItemPath} - Access denied, check permissions");
                                continue;
                            } 
                        }
                        break;
            }
        }

        return diffCounter;
    }
}
