using System;
using HarmonyLib;

namespace NetworkPlugin;

public class PluginInfo
{
    public const string PLUGIN_GUID = "NetworkPlugin";
    public const string PLUGIN_NAME = "Network Plugin";
    public const string PLUGIN_VERSION = "1.0.1";
    public static readonly Harmony harmony = new("pixels.lbol.mods.networkmod");
}