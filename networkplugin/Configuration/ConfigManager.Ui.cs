using BepInEx.Configuration;

namespace NetworkPlugin.Configuration;

public partial class ConfigManager
{
    /// <summary>
    /// 其他玩家 Overlay 停靠在屏幕右上角时的 X 偏移。
    /// </summary>
    public ConfigEntry<float> OtherPlayersOverlayRightOffsetX { get; private set; }

    /// <summary>
    /// 其他玩家 Overlay 停靠在屏幕右上角时的 Y 偏移。
    /// </summary>
    public ConfigEntry<float> OtherPlayersOverlayRightOffsetY { get; private set; }

    /// <summary>
    /// 调试：为所有 UI 控件添加红色 5px 边界，用于查看控件边界。
    /// </summary>
    public ConfigEntry<bool> DebugShowControlBounds { get; private set; }

    private void BindUiSettings(ConfigFile configFile)
    {
        OtherPlayersOverlayRightOffsetX = configFile.Bind(
            "UI.OtherPlayersOverlay",
            "RightOffsetX",
            -24f,
            "其他玩家 Overlay 停靠在屏幕右上角时的 X 偏移；负值越小越靠左"
        );

        OtherPlayersOverlayRightOffsetY = configFile.Bind(
            "UI.OtherPlayersOverlay",
            "RightOffsetY",
            -144f,
            "其他玩家 Overlay 停靠在屏幕右上角时的 Y 偏移；负值越小越靠上"
        );

        DebugShowControlBounds = configFile.Bind(
            "UI.Debug",
            "ShowControlBounds",
            false,
            "调试：为所有 UI 控件添加红色 5px 边界，用于查看控件边界"
        );
    }
}
