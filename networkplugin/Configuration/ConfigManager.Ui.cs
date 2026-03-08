using BepInEx.Configuration;

namespace NetworkPlugin.Configuration;

public partial class ConfigManager
{
    /// <summary>
    /// 其他玩家 overlay 右上停靠的 X 偏移。
    /// 负值越大越靠左，负值越小越靠右。
    /// </summary>
    public ConfigEntry<float> OtherPlayersOverlayRightOffsetX { get; private set; }

    /// <summary>
    /// 其他玩家 overlay 右上停靠的 Y 偏移。
    /// 负值越大越靠下，负值越小越靠上。
    /// </summary>
    public ConfigEntry<float> OtherPlayersOverlayRightOffsetY { get; private set; }

    /// <summary>
    /// 绑定 UI 配置。
    /// </summary>
    private void BindUiSettings(ConfigFile configFile)
    {
        OtherPlayersOverlayRightOffsetX = configFile.Bind(
            "UI.OtherPlayersOverlay",
            "RightOffsetX",
            -24f,
            "其他玩家 Overlay 停靠在屏幕右上角时的 X 偏移；负值越小越靠右"
        );

        OtherPlayersOverlayRightOffsetY = configFile.Bind(
            "UI.OtherPlayersOverlay",
            "RightOffsetY",
            -144f,
            "其他玩家 Overlay 停靠在屏幕右上角时的 Y 偏移；负值越小越靠上"
        );
    }
}