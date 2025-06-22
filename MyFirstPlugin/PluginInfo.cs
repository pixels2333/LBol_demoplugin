using System;
using HarmonyLib;

namespace MyFirstPlugin;

public class PluginInfo
{
    public const string PLUGIN_GUID = "SkinPlugin";
    public const string PLUGIN_NAME = "koishi skin plugin";
    public const string PLUGIN_VERSION = "1.0.1";

    public static readonly Harmony harmony = new("pixels.lbol.mods.skinmod");
}
