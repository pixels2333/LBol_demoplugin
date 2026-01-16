using LBoL.Core;

namespace NetworkPlugin.Network.NetworkPlayer;

/// <summary>
/// 定义网络玩家的基本属性和操作接口。
/// </summary>
/// <summary>
/// 网络玩家接口
/// 定义了网络游戏中玩家对象的标准行为和属性
/// 实现此接口的类需要处理玩家的网络同步、状态更新和通信功能
/// 
/// 该接口提供了完整的玩家状态管理功能，包括：
/// - 基础属性：用户名、角色标识、位置信息
/// - 战斗状态：生命值、格挡值、护盾值、金币
/// - 资源系统：法力值、终极能量
/// - 状态管理：姿态、展品、交易状态
/// - 网络同步：各种Update方法用于状态同步
/// - 操作方法：伤害处理、传送、复活等
/// 
/// 所有Update方法都包含updateServer参数，控制是否同步到服务器
/// 确保客户端状态与服务器保持一致性
/// </summary>
/// <remarks>
/// 说明：为与联机协议/序列化字段保持一致，本接口成员命名可能不遵循常规 C# 命名规范（例如 lowerCamelCase、下划线）。
/// 多数 Update* 方法的 updateServer 用于控制“本地状态更新后是否需要向服务器提交/广播”，具体行为由实现类决定。
/// </remarks>
public interface INetworkPlayer
{
    /// <summary>
    /// 玩家用户名
    /// 用于标识和显示玩家的名称，在网络游戏中需要保持同步
    /// </summary>
    string userName { get; set; }

    /// <summary>
    /// 玩家当前生命值
    /// 战斗中会动态变化，直接影响玩家存活状态
    /// </summary>
    int HP { get; set; }

    /// <summary>
    /// 玩家最大生命值
    /// 由角色决定，表示玩家可承受的最大伤害量
    /// </summary>
    int maxHP { get; set; }

    /// <summary>
    /// 玩家当前格挡值
    /// 可以抵消下一次受到的伤害，是重要的防御机制
    /// </summary>
    int block { get; set; }

    /// <summary>
    /// 玩家当前护盾值
    /// 另一种防御机制，与格挡值可能有不同的处理逻辑
    /// </summary>
    int shield { get; set; }

    /// <summary>
    /// 玩家金币数量
    /// 游戏内货币，用于购买物品、升级等消费行为
    /// </summary>
    int coins { get; set; }

    /// <summary>
    /// 玩家角色标识
    /// 表示玩家在游戏中选择的角色类型
    /// </summary>
    string chara { get; set; }

    /// <summary>
    /// 玩家终极能量
    /// 用于释放大招或特殊技能的高级资源
    /// </summary>

    /// <remarks>
    /// 注意：此段为历史遗留的占位描述。
    /// 当前接口内与终极相关的同步字段/方法以 ultimatePower 与 UpdateUltimatePower(bool) 为准。
    /// </remarks>

    /// <summary>
    /// 玩家当前位置描述
    /// 如村庄、商店、战斗房间等位置信息
    /// </summary>
    string location { get; set; }

    /// <summary>
    /// 是否已结束回合
    /// 回合制游戏中的重要状态，控制游戏流程
    /// </summary>
    bool endturn { get; set; }

    /// <summary>
    /// 玩家法力数组
    /// 支持多色法力系统，每个元素代表一种颜色的法力值
    /// </summary>
    /// <remarks>
    /// 当前联机同步通常使用固定长度数组（常见为 4 槽）表达“简化法力视图”；数组长度与颜色映射应以协议/DTO 实现为准。
    /// 原游戏内部存在更完整的法力表示（例如 ManaColor/ManaGroup），此处仅承载网络同步所需的最小信息。
    /// </remarks>
    int[] mana { get; set; }

    /// <summary>
    /// 玩家当前姿态
    /// TODO:stance名称可能要改
    /// 表示玩家的战斗姿态或状态
    /// </summary>
    /// <remarks>
    /// TODO 说明：原游戏侧更接近“mood（心境）”语义；此处的 stance 为历史遗留命名。
    /// 兼容策略：新增 mood 作为主字段，stance 保留为别名以减少破坏性；网络协议字段也应逐步迁移为 mood。
    /// </remarks>
    string stance { get; set; }

    /// <summary>
    /// 玩家当前心境（Mood）标识。
    /// </summary>
    /// <remarks>
    /// 该字段为联机侧的“心境/架势”主语义；与原游戏中通过 StatusEffect.UnitEffectName 驱动的心境特效概念保持一致。
    /// </remarks>
    string mood { get; set; }

    /// <summary>
    /// 玩家持有的展品列表
    /// 记录玩家收集的特殊物品或成就
    /// </summary>
    string[] exhibits { get; set; }

    /// <summary>
    /// 玩家交易状态
    /// 标识玩家是否正在进行交易操作
    /// </summary>
    bool tradingStatus { get; set; }

    /// <summary>
    /// 玩家终极能量（备用属性）
    /// 与UltimatePower功能重复，可能需要重构
    /// </summary>
    /// <remarks>
    /// 该字段当前以 bool 表示“终极技能是否可用/是否处于充能完成状态”。
    /// 若未来需要同步数值型“终极能量/进度”，应通过扩展协议与接口成员实现。
    /// </remarks>
    bool ultimatePower { get; set; }

    /// <summary>
    /// 玩家所在节点X坐标
    /// 在地图系统中的横向位置
    /// </summary>
    int location_X { get; set; }

    /// <summary>
    /// 玩家所在节点Y坐标
    /// 在地图系统中的纵向位置
    /// </summary>
    int location_Y { get; set; }

    /// <summary>
    /// 发送玩家数据到网络
    /// 将当前玩家的所有状态信息同步到其他客户端
    /// </summary>
    void SendData();

    /// <summary>
    /// 判断玩家是否为房主
    /// 房主拥有特殊权限，如控制游戏开始、踢出玩家等
    /// </summary>
    /// <returns>如果是房主返回true，否则返回false</returns>
    bool IsLobbyOwner();

    /// <summary>
    /// 存档加载后执行的操作
    /// 在玩家加载存档后进行必要的初始化和网络同步
    /// </summary>
    void PostSaveLoad();

    /// <summary>
    /// 判断玩家是否在同一房间
    /// 用于检查玩家间的交互和可见性
    /// </summary>
    /// <returns>如果在同一房间返回true，否则返回false</returns>
    bool IsPlayerInSameRoom();

    /// <summary>
    /// 判断玩家是否在同一章节
    /// 确保玩家在同一个游戏进度区域
    /// </summary>
    /// <returns>如果在同一章节返回true，否则返回false</returns>
    bool IsPlayerOnSameAct();

    /// <summary>
    /// 更新玩家濒死状态
    /// 检查并更新玩家的生命状态，必要时通知服务器
    /// </summary>
    /// <param name="updateServer">是否同步到服务器</param>
    void IsNearDeath(bool updateServer);

    /// <summary>
    /// 判断是否应该渲染角色
    /// 根据游戏状态决定是否显示角色模型
    /// </summary>
    /// <returns>应该渲染返回true，否则返回false</returns>
    bool ShouldRenderCharacter();

    /// <summary>
    /// 判断是否应该渲染角色信息框
    /// 控制角色信息的UI显示
    /// </summary>
    /// <returns>应该渲染返回true，否则返回false</returns>
    bool ShouldRenderCharacterInfoBox();

    /// <summary>
    /// 更新生命值信息
    /// 同步生命值变化到网络和其他客户端
    /// </summary>
    /// <param name="updateServer">是否同步到服务器</param>
    void UpdateHealth(bool updateServer);

    /// <summary>
    /// 更新格挡值信息
    /// 同步格挡值变化到网络和其他客户端
    /// </summary>
    /// <param name="updateServer">是否同步到服务器</param>
    void UpdateBlock(bool updateServer);

    /// <summary>
    /// 更新最大生命值信息
    /// 同步最大生命值变化到网络和其他客户端
    /// </summary>
    /// <param name="updateServer">是否同步到服务器</param>
    void UpdateMaxHP(bool updateServer);

    /// <summary>
    /// 更新金币数量
    /// 同步金币变化到网络和其他客户端
    /// </summary>
    /// <param name="updateServer">是否同步到服务器</param>
    void UpdateCoins(bool updateServer);

    /// <summary>
    /// 更新玩家基本信息
    /// 同步玩家的综合信息到网络和其他客户端
    /// </summary>
    /// <param name="updateServer">是否同步到服务器</param>
    void UpdatePlayerInfo(bool updateServer);

    //TODO:stance名称可能要改
    /// <summary>
    /// 更新玩家姿态信息
    /// 同步姿态变化到网络和其他客户端
    /// </summary>
    /// <param name="updateServer">是否同步到服务器</param>
    /// <remarks>
    /// 兼容入口：与历史字段 stance 对应；实现建议内部转调 UpdateMood。
    /// </remarks>
    void UpdateStance(bool updateServer);

    /// <summary>
    /// 更新玩家心境（Mood）信息。
    /// </summary>
    /// <param name="updateServer">是否同步到服务器</param>
    void UpdateMood(bool updateServer);

    // void ClearPowers(bool updateServer);

    // void UpdatePowers(bool updateServer);

    // void UpdateTempPowers(bool updateServer);

    /// <summary>
    /// 更新状态效果信息
    /// 同步玩家的状态效果变化到网络和其他客户端
    /// </summary>
    /// <param name="updateServer">是否同步到服务器</param>
    void UpdateStatusEffects(bool updateServer);

    /// <summary>
    /// 更新终极能量信息
    /// 同步终极能量变化到网络和其他客户端
    /// </summary>
    /// <param name="updateServer">是否同步到服务器</param>
    /// <remarks>
    /// 当前实现通常同步的是“终极状态（bool）”，与 ultimatePower 字段语义保持一致。
    /// </remarks>
    void UpdateUltimatePower(bool updateServer);

    /// <summary>
    /// 更新展品信息
    /// 同步玩家获得的展物变化到网络和其他客户端
    /// </summary>
    /// <param name="updateServer">是否同步到服务器</param>
    void UpdateExhibits(bool updateServer);

    /// <summary>
    /// 更新法力值信息
    /// 同步法力值变化到网络和其他客户端
    /// </summary>
    /// <param name="updateServer">是否同步到服务器</param>
    void UpdateMana(bool updateServer);

    /// <summary>
    /// 更新结束回合状态
    /// 同步回合结束信息到网络和其他客户端
    /// </summary>
    /// <param name="updateServer">是否同步到服务器</param>
    void UpdateEndTurn(bool updateServer);

    /// <summary>
    /// 更新位置信息
    /// 同步玩家在地图上的位置变化到网络和其他客户端
    /// </summary>
    /// <param name="visitingnode">访问的地图节点</param>
    /// <param name="updateServer">是否同步到服务器，默认为true</param>
    /// <remarks>
    /// MapNode 来自 LBoL.Core，除 X/Y 外还包含 Act、StationType 等关键信息；此处用于同步“正在访问的节点”。
    /// </remarks>
    void UpdateLocation(MapNode visitingnode, bool updateServer = true);

    /// <summary>
    /// 更新存活状态
    /// 同步玩家的存活状态到网络和其他客户端
    /// </summary>
    /// <param name="updateServer">是否同步到服务器</param>
    void UpdateLiveStatus(bool updateServer);

    //TODO:预计弃用
    // NOTE: 若后续引入统一的玩家管理器/上下文，本方法可被替代为从上下文获取本地玩家实例。
    /// <summary>
    /// 获取玩家自身实例
    /// 返回当前玩家的网络对象引用
    /// </summary>
    /// <returns>当前玩家的INetworkPlayer实例</returns>
    /// <remarks>
    /// TODO 说明：GetMyself 可能导致接口职责不清（接口同时承担“行为契约”和“全局访问点”）。
    /// 建议后续由上层维护本地玩家引用（例如 PlayerManager/NetworkContext），并逐步迁移调用点。
    /// </remarks>
    INetworkPlayer GetMyself();

    /// <summary>
    /// 受到伤害
    /// 处理玩家受到伤害的逻辑，包括生命值扣除和状态更新
    /// </summary>
    /// <param name="damage">受到的伤害值</param>
    void Takedamage(int damage);

    /// <summary>
    /// 造成伤害
    /// 使玩家对其他目标造成伤害
    /// </summary>
    /// <param name="damage">造成的伤害值</param>
    void DealDamage(int damage);

    /// <summary>
    /// 复活玩家
    /// 将死亡玩家复活并设置新的生命值
    /// </summary>
    /// <param name="username">复活玩家的用户名</param>
    /// <param name="newhp">复活后的生命值</param>
    void Resurrect(string username, int newhp);

    /// <summary>
    /// 传送玩家
    /// 将玩家传送到指定的坐标位置
    /// </summary>
    /// <param name="x">目标X坐标</param>
    /// <param name="y">目标Y坐标</param>
    void Teleport(int x, int y);
}
