using System;

namespace debugtools;

//检测json火atlas文件是否存在
public static class FileExists
{
    public static bool JsonFileExists(string jsonPath)
    {
        try
        {
            return System.IO.File.Exists(jsonPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking JSON file existence: {ex.Message}");
            return false;
        }
    }

    public static bool AtlasFileExists(string atlasPath)
    {
        try
        {
            return System.IO.File.Exists(atlasPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking Atlas file existence: {ex.Message}");
            return false;
        }
    }
    
}
// The class is already complete, no further changes are necessary.