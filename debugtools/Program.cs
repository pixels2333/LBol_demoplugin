// See https://aka.ms/new-console-template for more information
using System.Reflection;
using System.Text.Json.Nodes;

// Minimal repo invariants check for inrun-map-progress-sync.
// Keep it opt-in to avoid interfering with existing debugtools usage.
if (args.Length > 0 && string.Equals(args[0], "verify-inrun", StringComparison.OrdinalIgnoreCase))
{
    string root = args.Length > 1 ? args[1] : Environment.CurrentDirectory;
    return debugtools.VerifyInrunMapProgressSync.Run(root);
}

String apath = @"F:\thunderbolt mods\TouhouLostBranchOfLegend\profiles\Default\BepInEx\plugins\koishi514\MyFirstPlugin\Resource\reimu\reimu.json";
String rpath = "../MyFirstPlugin/Resource/reimu/reimu.json";

Console.WriteLine($"当前工作目录: {Environment.CurrentDirectory}\n");

if (JsonFileExists(rpath))
{
    Console.WriteLine("JSON文件存在");
}
else
{
    Console.WriteLine("JSON文件不存在");
}

static bool JsonFileExists(string jsonPath)
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

static bool AtlasFileExists(string atlasPath)
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

return 0;