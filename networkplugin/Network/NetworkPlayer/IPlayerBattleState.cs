namespace NetworkPlugin.Network.NetworkPlayer;

/// <summary>
/// 玩家战斗状态接口——生命、格挡、护盾、心境、位置等
/// </summary>
public interface IPlayerBattleState
{
    /// <summary>当前生命值</summary>
    int HP { get; set; }

    /// <summary>最大生命值</summary>
    int maxHP { get; set; }

    /// <summary>当前格挡值</summary>
    int block { get; set; }

    /// <summary>当前护盾值</summary>
    int shield { get; set; }

    /// <summary>是否已结束回合</summary>
    bool endturn { get; set; }

    /// <summary>当前位置描述</summary>
    string location { get; set; }

    /// <summary>当前章节索引</summary>
    int stage { get; set; }

    /// <summary>心境标识</summary>
    string mood { get; set; }

    /// <summary>节点 X 坐标</summary>
    int location_X { get; set; }

    /// <summary>节点 Y 坐标</summary>
    int location_Y { get; set; }

    /// <summary>判断玩家是否在同一房间</summary>
    bool IsPlayerInSameRoom();

    /// <summary>判断玩家是否在同一章节</summary>
    bool IsPlayerOnSameAct();

    /// <summary>是否应该渲染角色</summary>
    bool ShouldRenderCharacter();

    /// <summary>是否应该渲染角色信息框</summary>
    bool ShouldRenderCharacterInfoBox();

    /// <summary>更新濒死状态</summary>
    void IsNearDeath(bool updateServer);

    /// <summary>受到伤害</summary>
    void Takedamage(int damage);

    /// <summary>造成伤害</summary>
    void DealDamage(int damage);

    /// <summary>传送玩家</summary>
    void Teleport(int x, int y);

    /// <summary>复活玩家</summary>
    void Resurrect(string username, int newhp);
}
