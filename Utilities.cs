using System.Security.Cryptography;

namespace VeeamFolderSync;

public static class Utilities
{
    public static ItemTypeEnum CheckIfFileOrFolder(string itemPath)
    {
        return File.Exists(itemPath) ? ItemTypeEnum.File : Directory.Exists(itemPath) ? ItemTypeEnum.Directory : ItemTypeEnum.NotFound;         
    }

    //MD5 - compromised, but still good for change detection
    public static bool HashesAreEqual(string sourceFilePath, string replicaFilePath)
    {
        using MD5 md5Hash = MD5.Create();
        using var sourceFileContent = File.OpenRead(sourceFilePath);
        using var replicaFileContent = File.OpenRead(replicaFilePath);
        var sourceMD5hash = md5Hash.ComputeHash(sourceFileContent);
        var replicaMD5hash = md5Hash.ComputeHash(replicaFileContent);

        return sourceMD5hash.SequenceEqual(replicaMD5hash);
    }

    public enum ItemTypeEnum
    {
        File,
        Directory,
        NotFound
    }
    
}
