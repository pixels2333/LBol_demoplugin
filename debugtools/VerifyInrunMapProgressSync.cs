using System;
using System.IO;
using System.Linq;

namespace debugtools;

internal static class VerifyInrunMapProgressSync
{
    public static int Run(string repoRoot)
    {
        if (string.IsNullOrWhiteSpace(repoRoot))
        {
            repoRoot = Directory.GetCurrentDirectory();
        }

        // When running from debugtools/bin/..., walk up to the repo root.
        repoRoot = FindRepoRoot(repoRoot) ?? repoRoot;

        int failed = 0;

        failed += AssertFileMissing(repoRoot, Path.Combine("networkplugin", "Patch", "Network", "SaveLoadSyncPatch.cs"),
            "SaveLoadSyncPatch 已废弃：联机不应传输存档 bytes。")
            ? 0
            : 1;

        failed += AssertNoHostSaveTransferConstants(repoRoot)
            ? 0
            : 1;

        Console.WriteLine(failed == 0
            ? "[verify] OK"
            : "[verify] FAILED");

        return failed == 0 ? 0 : 2;
    }

    private static bool AssertFileMissing(string repoRoot, string relPath, string reason)
    {
        string full = Path.Combine(repoRoot, relPath);
        if (File.Exists(full))
        {
            Console.WriteLine("[verify] FAIL: unexpected file exists: " + full);
            Console.WriteLine("         " + reason);
            return false;
        }

        Console.WriteLine("[verify] PASS: missing as expected: " + relPath);
        return true;
    }

    private static bool AssertNoHostSaveTransferConstants(string repoRoot)
    {
        string path = Path.Combine(repoRoot, "networkplugin", "Network", "Messages", "NetworkMessageTypes.cs");
        if (!File.Exists(path))
        {
            Console.WriteLine("[verify] FAIL: missing file: " + path);
            return false;
        }

        string text = File.ReadAllText(path);
        string[] forbidden =
        {
            "OnHostSaveTransferStart",
            "OnHostSaveTransferChunk",
            "OnHostSaveTransferEnd",
        };

        string? hit = forbidden.FirstOrDefault(f => text.Contains(f, StringComparison.Ordinal));
        if (hit != null)
        {
            Console.WriteLine("[verify] FAIL: NetworkMessageTypes.cs still contains: " + hit);
            return false;
        }

        Console.WriteLine("[verify] PASS: NetworkMessageTypes has no HostSaveTransfer constants");
        return true;
    }

    private static string? FindRepoRoot(string start)
    {
        try
        {
            DirectoryInfo? dir = new DirectoryInfo(start);
            for (int i = 0; i < 8 && dir != null; i++)
            {
                if (Directory.Exists(Path.Combine(dir.FullName, "networkplugin")) &&
                    Directory.Exists(Path.Combine(dir.FullName, "helloagents")))
                {
                    return dir.FullName;
                }

                dir = dir.Parent;
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }
}
