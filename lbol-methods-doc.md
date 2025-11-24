# LBoL.Core 核心代码文件目录结构

> 游戏主要功能核心代码文件清单（按目录结构排列）
> 总代码文件数：200+

```
lbol/LBoL.Core/
├── 核心框架与游戏循环
│   ├── GameRunController.cs          # 游戏运行控制器（核心游戏循环）
│   ├── GameEntity.cs                 # 游戏实体基类
│   ├── GameMap.cs                    # 游戏地图管理
│   ├── MapNode.cs                    # 地图节点
│   ├── Stage.cs                      # 游戏关卡
│   ├── Library.cs                    # 游戏内容库
│   ├── GlobalConfig.cs               # 全局配置
│   ├── GameMode.cs                   # 游戏模式
│   ├── GameDifficulty.cs             # 游戏难度
│   ├── GameResultType.cs             # 游戏结果类型
│   ├── GameRunStatus.cs              # 游戏运行状态
│   ├── GameRunMapMode.cs             # 游戏运行地图模式
│   ├── GameRunStartupParameters.cs   # 游戏启动参数
│   └── MainMenuStatus.cs             # 主菜单状态
│
├── 战斗系统 (Battle/)
│   ├── BattleController.cs           # 战斗控制器（核心）
│   ├── ActionResolver.cs             # 行动解析器
│   ├── BattleAction.cs               # 战斗行动基类
│   ├── Phase.cs                      # 战斗阶段
│   ├── BattleStatus.cs               # 战斗状态
│   ├── BattleMessage.cs              # 战斗消息
│   ├── DieCause.cs                   # 死亡原因
│   ├── ActionCause.cs                # 行动原因
│   ├── ICustomCounter.cs             # 自定义计数器接口
│   ├── CustomCounterResetTiming.cs   # 计数器重置时机
│   ├── EventBattleAction.cs          # 事件战斗行动
│   ├── EventSequencedReactor.cs      # 事件序列反应器
│   ├── LazyActionReactor.cs          # 延迟行动反应器
│   ├── LazySequencedReactor.cs       # 延迟序列反应器
│   ├── Reactor.cs                    # 反应器
│   ├── SimpleAction.cs               # 简单行动
│   ├── SimpleEventBattleAction.cs    # 简单事件战斗行动
│   ├── BattleActionRecord/           # 战斗行动记录
│   │   ├── ActionRecord.cs           # 行动记录
│   │   └── PhaseRecord.cs            # 阶段记录
│   ├── BattleActions/                # 战斗行动实现（80+ 文件）
│   │   ├── DamageAction.cs           # 伤害行动
│   │   ├── HealAction.cs             # 治疗行动
│   │   ├── PlayCardAction.cs         # 打牌行动
│   │   ├── UseCardAction.cs          # 使用卡牌行动
│   │   ├── DrawCardAction.cs         # 抽牌行动
│   │   ├── DrawManyCardAction.cs     # 抽取多张牌
│   │   ├── GainManaAction.cs         # 获得法力
│   │   ├── GainPowerAction.cs        # 获得能量
│   │   ├── GainMoneyAction.cs        # 获得金钱
│   │   ├── ConsumeManaAction.cs      # 消耗法力
│   │   ├── ConsumePowerAction.cs     # 消耗能量
│   │   ├── ConsumeMoneyAction.cs     # 消耗金钱
│   │   ├── ConvertManaAction.cs      # 转换法力
│   │   ├── CastBlockShieldAction.cs  # 施放护盾
│   ├── LoseBlockShieldAction.cs      # 失去护盾
│   │   ├── StartPlayerTurnAction.cs  # 开始玩家回合
│   │   ├── EndPlayerTurnAction.cs    # 结束玩家回合
│   │   ├── StartEnemyTurnAction.cs   # 开始敌人回合
│   │   ├── EndEnemyTurnAction.cs     # 结束敌人回合
│   │   ├── StartAllEnemyTurnAction.cs # 开始所有敌人回合
│   │   ├── EndAllEnemyTurnAction.cs  # 结束所有敌人回合
│   │   ├── StartRoundAction.cs       # 开始回合
│   │   ├── EndRoundAction.cs         # 结束回合
│   │   ├── StartBattleAction.cs      # 开始战斗
│   │   ├── EndBattleAction.cs        # 结束战斗
│   │   ├── EscapeAction.cs           # 逃跑行动
│   │   ├── InstantWinAction.cs       # 立即胜利
│   │   ├── DieAction.cs              # 死亡行动
│   │   ├── ForceKillAction.cs        # 强制击杀
│   │   ├── ExplodeAction.cs          # 爆炸行动
│   │   ├── ApplyStatusEffectAction.cs # 应用状态效果
│   │   ├── RemoveStatusEffectAction.cs # 移除状态效果
│   │   ├── RemoveAllNegativeStatusEffectAction.cs # 移除所有负面状态
│   │   ├── AddCardsToHandAction.cs   # 添加卡牌到手牌
│   │   ├── AddCardsToDeckAction.cs   # 添加卡牌到牌库
│   │   ├── AddCardsToDiscardAction.cs # 添加卡牌到弃牌堆
│   │   ├── AddCardsToExileAction.cs  # 添加卡牌到放逐区
│   │   ├── RemoveCardAction.cs       # 移除卡牌
│   │   ├── ExileCardAction.cs        # 放逐卡牌
│   │   ├── ExileManyCardAction.cs    # 放逐多张卡牌
│   │   ├── MoveCardAction.cs         # 移动卡牌
│   │   ├── ReshuffleAction.cs        # 重洗牌库
│   │   ├── UpgradeCardAction.cs      # 升级卡牌
│   │   ├── UpgradeCardsAction.cs     # 升级多张卡牌
│   │   ├── TransformCardAction.cs    # 变换卡牌
│   │   ├── DreamCardsAction.cs       # 梦境卡牌
│   │   ├── DreamCardsToHandAction.cs # 梦境卡牌到手牌
│   │   ├── ScryAction.cs             # 侦察
│   │   ├── AddDollAction.cs          # 添加人偶
│   │   ├── RemoveDollAction.cs       # 移除人偶
│   │   ├── TriggerDollActiveAction.cs # 触发人偶主动技能
│   │   ├── TriggerDollPassiveAction.cs # 触发人偶被动技能
│   │   ├── TriggerAllDollsPassiveAction.cs # 触发所有人偶被动
│   │   ├── UseDollAction.cs          # 使用人偶
│   │   ├── LoseManaAction.cs         # 失去法力
│   │   ├── LosePowerAction.cs        # 失去能量
│   │   ├── LoseMoneyAction.cs        # 失去金钱
│   │   ├── LockTurnManaAction.cs     # 锁定回合法力
│   │   ├── LockRandomTurnManaAction.cs # 锁定随机回合法力
│   │   ├── UnlockTurnManaAction.cs   # 解锁回合法力
│   │   ├── UnlockAllTurnManaAction.cs # 解锁所有回合法力
│   │   ├── LoseTurnManaAction.cs     # 失去回合法力
│   │   ├── GainTurnManaAction.cs     # 获得回合法力
│   │   ├── MoodChangeAction.cs       # 情绪变化
│   │   ├── EnemyMoveAction.cs        # 敌人移动
│   │   ├── SpawnEnemyAction.cs       # 生成敌人
│   │   ├── AddDollSlotAction.cs      # 添加人偶槽位
│   │   ├── RemoveDollSlotAction.cs   # 移除人偶槽位
│   │   ├── FollowAttackAction.cs     # 跟随攻击
│   │   ├── EndShootAction.cs         # 结束射击
│   │   ├── RetainAction.cs           # 保留
│   │   ├── WaitForCoroutineAction.cs # 等待协程
│   │   └── WaitForYieldInstructionAction.cs # 等待yield指令
│   ├── Interactions/                 # 战斗交互
│   │   ├── Interaction.cs            # 交互基类
│   │   ├── SelectCardInteraction.cs  # 选择卡牌交互
│   │   ├── UpgradeCardInteraction.cs # 升级卡牌交互
│   │   ├── TransformCardInteraction.cs # 变换卡牌交互
│   │   ├── RemoveCardInteraction.cs  # 移除卡牌交互
│   │   ├── RewardInteraction.cs      # 奖励交互
│   │   ├── SelectHandInteraction.cs  # 选择手牌交互
│   │   └── MiniSelectCardInteraction.cs # 迷你选择卡牌
│   └── InteractionViewer.cs          # 交互查看器
│   └── BattleActionViewer.cs         # 战斗行动查看器
│   └── ActionViewer.cs               # 行动查看器
│
├── 卡牌系统 (Cards/)
│   ├── Card.cs                       # 卡牌基类（核心）
│   ├── CardZone.cs                   # 卡牌区域
│   ├── IXCostFilter.cs               # X费用过滤器接口
│   ├── GunType.cs                    # 枪械类型
│   ├── Guns.cs                       # 枪械
│   ├── GunPair.cs                    # 枪对
│   ├── WhiteLaser.cs                 # 白色激光
│   ├── NightMana1.cs                 # 夜之法力1
│   ├── NightMana2.cs                 # 夜之法力2
│   ├── NightMana3.cs                 # 夜之法力3
│   ├── NightMana4.cs                 # 夜之法力4
│   ├── Zhukeling.cs                  # 竹雀灵
│   ├── Xiaozhuo.cs                   # 小灼
│   ├── CirnoFreeze.cs                # 琪露诺冰冻
│   ├── ManaFreezer.cs                # 法力冻结
│   ├── FriendToken.cs                # 好友标记
│   └── FollowAttackFiller.cs         # 跟随攻击填充
│
├── 单位系统 (Units/)
│   ├── Unit.cs                       # 单位基类（核心）
│   ├── PlayerUnit.cs                 # 玩家单位
│   ├── EnemyUnit.cs                  # 敌人单位
│   ├── EnemyUnit.2.cs                # 敌人单位扩展
│   ├── Doll.cs                       # 人偶系统
│   ├── BattleSet.cs                  # 战斗配置
│   ├── EnemyGroup.cs                 # 敌人群组
│   ├── EnemyGroupEntry.cs            # 敌人群组条目
│   ├── Intention.cs                  # 敌人意图基类
│   ├── IntentionType.cs              # 意图类型
│   ├── IEnemyMove.cs                 # 敌人移动接口
│   ├── SimpleEnemyMove.cs            # 简单敌人移动
│   ├── IEnemyUnitView.cs             # 敌人单位视图接口
│   ├── IPlayerUnitView.cs            # 玩家单位视图接口
│   ├── IUnitView.cs                  # 单位视图接口
│   ├── PlayerType.cs                 # 玩家类型
│   ├── UnitStatus.cs                 # 单位状态
│   └── UltimateSkill.cs              # 终极技能
│
├── 状态效果系统 (StatusEffects/)
│   ├── StatusEffect.cs               # 状态效果基类
│   ├── IOpposing.cs                  # 对立接口
│   ├── OpposeResult.cs               # 对立结果
│   ├── StatusEffectAddResult.cs      # 状态添加结果
│   ├── TurnStatus.cs                 # 回合状态
│   ├── Burst.cs                      # 爆发
│   ├── BurstDrawSe.cs                # 爆发抽牌SE
│   ├── BurstUpgrade.cs               # 爆发升级
│   ├── Charging.cs                   # 充能
│   ├── Concentration.cs              # 专注
│   ├── Control.cs                    # 控制（正面）
│   ├── ControlNegative.cs            # 控制（负面）
│   ├── TempControl.cs                # 临时控制（正面）
│   ├── TempControlNegative.cs        # 临时控制（负面）
│   ├── Firepower.cs                  # 火力（正面）
│   ├── FirepowerNegative.cs          # 火力（负面）
│   ├── TempFirepower.cs              # 临时火力（正面）
│   ├── TempFirepowerNegative.cs      # 临时火力（负面）
│   ├── Spirit.cs                     # 灵力（正面）
│   ├── SpiritNegative.cs             # 灵力（负面）
│   ├── TempSpirit.cs                 # 临时灵力（正面）
│   ├── TempSpiritNegative.cs         # 临时灵力（负面）
│   ├── Weak.cs                       # 虚弱
│   ├── Vulnerable.cs                 # 易伤
│   ├── LockedOn.cs                   # 锁定
│   ├── EnemyLockedOn.cs              # 敌人锁定
│   ├── Invincible.cs                 # 无敌
│   ├── InvincibleEternal.cs          # 永恒无敌
│   ├── Grace.cs                      # 优雅（护盾）
│   ├── Graze.cs                      # 擦弹
│   ├── Fragil.cs                     # 脆弱
│   ├── LimitedDamage.cs              # 伤害限制
│   ├── DeepFreezeSe.cs               # 深度冻结SE
│   ├── MirrorImage.cs                # 镜像
│   ├── Servant.cs                    # 从者
│   ├── WindGirl.cs                   # 风之少女
│   ├── Mood.cs                       # 情绪
│   ├── GuangxueMicai.cs              # 光学迷彩
│   ├── BossAct.cs                    # Boss行动
│   ├── ExtraTurn.cs                  # 额外回合
│   ├── SuperExtraTurn.cs             # 超级额外回合
│   └── TurnStartDontLoseBlock.cs     # 回合开始不失护盾
│
├── 意图系统 (Intentions/)
│   ├── AttackIntention.cs            # 攻击意图
│   ├── DefendIntention.cs            # 防御意图
│   ├── SleepIntention.cs             # 睡眠意图
│   ├── HealIntention.cs              # 治疗意图
│   ├── EscapeIntention.cs            # 逃跑意图
│   ├── ExplodeIntention.cs           # 爆炸意图
│   ├── ExplodeAllyIntention.cs       # 友军爆炸意图
│   ├── DoNothingIntention.cs         # 无行动意图
│   ├── StunIntention.cs              # 眩晕意图
│   ├── GrazeIntention.cs             # 擦弹意图
│   ├── ClearIntention.cs             # 清除意图
│   ├── ChargeIntention.cs            # 充能意图
│   ├── SpawnIntention.cs             # 生成意图
│   ├── SpawnDroneIntention.cs        # 生成无人机意图
│   ├── RepairIntention.cs            # 修复意图
│   ├── HexIntention.cs               # 诅咒意图
│   ├── KokoroDarkIntention.cs        # 暗心意图
│   ├── NegativeEffectIntention.cs    # 负面效果意图
│   ├── PositiveEffectIntention.cs    # 正面效果意图
│   ├── SpellCardIntention.cs         # 符卡意图
│   ├── CountDownIntention.cs         # 倒计时意图
│   ├── AddCardIntention.cs           # 添加卡牌意图
│   └── UnknownIntention.cs           # 未知意图
│
├── 地图节点与事件 (Stations/)
│   ├── Station.cs                    # 节点基类
│   ├── BattleStation.cs              # 战斗节点
│   ├── EliteEnemyStation.cs          # 精英敌人节点
│   ├── BossStation.cs                # Boss节点
│   ├── EnemyStation.cs               # 普通敌人节点
│   ├── ShopStation.cs                # 商店节点
│   ├── SupplyStation.cs              # 补给节点
│   ├── TradeStation.cs               # 交易节点
│   ├── GapStation.cs                 # 间隙节点
│   ├── AdventureStation.cs           # 冒险节点
│   ├── BattleAdvTestStation.cs       # 战斗冒险测试节点
│   ├── SelectStation.cs              # 选择节点
│   ├── EntryStation.cs               # 入口节点
│   ├── IAdventureStation.cs          # 冒险节点接口
│   ├── StationType.cs                # 节点类型
│   ├── StationStatus.cs              # 节点状态
│   ├── StationReward.cs              # 节点奖励
│   ├── StationRewardType.cs          # 节点奖励类型
│   ├── ShopItem.cs                   # 商店物品
│   └── StationDialogSource.cs        # 节点对话源
│
├── 冒险系统 (Adventures/)
│   ├── Adventure.cs                  # 冒险事件基类
│   ├── FakeAdventure.cs              # 虚假冒险
│   ├── AdventureInfoAttribute.cs     # 冒险信息特性
│   └── IAdventureWeighter.cs         # 冒险权重接口
│
├── 宝物系统
│   ├── Exhibit.cs                    # 宝物基类
│   ├── ExhibitInfoAttribute.cs       # 宝物信息特性
│   └── Exhibits/
│       ├── ZhinengYinxiang.cs        # 智能音响
│       └── YichuiPiao.cs             # 一吹瓢
│
├── 对话系统 (Dialogs/)
│   ├── DialogRunner.cs               # 对话运行器
│   ├── DialogProgram.cs              # 对话程序
│   ├── DialogPhase.cs                # 对话阶段基类
│   ├── DialogLinePhase.cs            # 对话行阶段
│   ├── DialogOptionsPhase.cs         # 对话选项阶段
│   ├── DialogOption.cs               # 对话选项
│   ├── DialogOptionData.cs           # 对话选项数据
│   ├── DialogCommandPhase.cs         # 对话命令阶段
│   ├── DialogFunctionAttribute.cs    # 对话函数特性
│   ├── DialogFunctions.cs            # 对话函数
│   ├── LineArgumentHandler.cs        # 行参数处理器
│   └── DialogStorage.cs              # 对话存储
│
├── 玉盒系统 (JadeBoxes/)
│   ├── JadeBox.cs                    # 玉盒基类
│   ├── TwoColorStart.cs              # 双色开局
│   └── StartWithMythic.cs            # 神话开局
│
├── 间隙选项 (GapOptions/)
│   ├── GapOption.cs                  # 间隙选项基类
│   ├── UpgradeCard.cs                # 升级卡牌
│   ├── RemoveCard.cs                 # 移除卡牌
│   ├── FindExhibit.cs                # 寻找宝物
│   ├── GetRareCard.cs                # 获得稀有卡牌
│   ├── GetMoney.cs                   # 获得金钱
│   ├── DrinkTea.cs                   # 喝茶
│   └── UpgradeBaota.cs               # 升级宝塔
│
├── 数据保存系统 (SaveData/)
│   ├── SaveDataHelper.cs             # 存档辅助
│   ├── GameRunSaveData.cs            # 游戏运行存档
│   ├── PlayerSaveData.cs             # 玩家存档
│   ├── AdventureSaveData.cs          # 冒险存档
│   ├── CardSaveData.cs               # 卡牌存档
│   ├── CardRecordSaveData.cs         # 卡牌记录存档
│   ├── ExhibitSaveData.cs            # 宝物存档
│   ├── StageSaveData.cs              # 关卡存档
│   ├── MapNodeSaveData.cs            # 地图节点存档
│   ├── GameSettingsSaveData.cs       # 游戏设置存档
│   ├── StageRecord.cs                # 关卡记录
│   ├── StationRecord.cs              # 节点记录
│   ├── GameRunRecordSaveData.cs      # 游戏运行记录存档
│   ├── CharacterStatsSaveData.cs     # 角色统计存档
│   ├── ProfileSaveData.cs            # 配置文件存档
│   ├── SysSaveData.cs                # 系统存档
│   ├── HintLevel.cs                  # 提示等级
│   ├── HintStatusSaveData.cs         # 提示状态存档
│   ├── QuickPlayLevel.cs             # 快速游戏等级
│   ├── JadeBoxSaveData.cs            # 玉盒存档
│   ├── RandomPoolEntrySaveData.cs    # 随机池条目存档
│   ├── RepeatableRandomPoolSaveData.cs # 可重复随机池存档
│   ├── UniqueRandomPoolSaveData.cs   # 唯一随机池存档
│   ├── CardWeightFactorSaveData.cs   # 卡牌权重因子存档
│   └── Lzss.cs                       # LZSS压缩
│   └── SaveTiming.cs                 # 存档时机
│
├── 随机系统 (Randoms/)
│   ├── IRandomPool.cs                # 随机池接口
│   ├── RandomPoolEntry.cs            # 随机池条目
│   ├── RepeatableRandomPool.cs       # 可重复随机池
│   ├── UniqueRandomPool.cs           # 唯一随机池
│   ├── CardWeightTable.cs            # 卡牌权重表
│   ├── CardWeightTableExtensions.cs  # 卡牌权重表扩展
│   ├── CardTypeWeightTable.cs        # 卡牌类型权重表
│   ├── ExhibitWeightTable.cs         # 宝物权重表
│   ├── OwnerWeightTable.cs           # 拥有者权重表
│   ├── AppearanceWeightTable.cs      # 出现权重表
│   ├── RarityWeightTable.cs          # 稀有度权重表
│   └── RarityWeightTableExtensions.cs # 稀有度权重表扩展
│
├── 统计与成就 (Stats/)
│   ├── GameRunStats.cs               # 游戏运行统计
│   └── BattleStats.cs                # 战斗统计
│
├── 事件参数类
│   ├── DamageInfo.cs                 # 伤害信息
│   ├── DamageEventArgs.cs            # 伤害事件参数
│   ├── DamageDealingEventArgs.cs     # 造成伤害事件参数
│   ├── StatisticalDamageEventArgs.cs # 统计伤害事件参数
│   ├── HealEventArgs.cs              # 治疗事件参数
│   ├── HealType.cs                   # 治疗类型
│   ├── ShieldInfo.cs                 # 护盾信息
│   ├── BlockInfo.cs                  # 格挡信息
│   ├── BlockShieldEventArgs.cs       # 格挡护盾事件参数
│   ├── BlockShieldType.cs            # 格挡护盾类型
│   ├── FollowAttackEventArgs.cs      # 跟随攻击事件参数
│   ├── ManaEventArgs.cs              # 法力事件参数
│   ├── ManaConvertingEventArgs.cs    # 法力转换事件参数
│   ├── PowerEventArgs.cs             # 能量事件参数
│   ├── MoodChangeEventArgs.cs        # 情绪变化事件参数
│   ├── CardEventArgs.cs              # 卡牌事件参数
│   ├── CardUsingEventArgs.cs         # 使用卡牌事件参数
│   ├── CardTransformEventArgs.cs     # 卡牌变换事件参数
│   ├── CardMovingEventArgs.cs        # 卡牌移动事件参数
│   ├── CardMovingToDrawZoneEventArgs.cs # 卡牌移动到抽牌区参数
│   ├── CardsEventArgs.cs             # 卡牌事件参数
│   ├── CardsAddingToDrawZoneEventArgs.cs # 卡牌添加到抽牌区参数
│   ├── DollEventArgs.cs              # 人偶事件参数
│   ├── DollUsingEventArgs.cs         # 使用人偶事件参数
│   ├── DollTriggeredEventArgs.cs     # 人偶触发事件参数
│   ├── DollMagicArgs.cs              # 人偶魔法参数
│   ├── DollMagicEventArgs.cs         # 人偶魔法事件参数
│   ├── DollValueArgs.cs              # 人偶值参数
│   ├── StatusEffectEventArgs.cs      # 状态效果事件参数
│   ├── StatusEffectApplyEventArgs.cs # 状态效果应用事件参数
│   ├── UnitEventArgs.cs              # 单位事件参数
│   ├── DieEventArgs.cs               # 死亡事件参数
│   ├── ForceKillEventArgs.cs         # 强制击杀事件参数
│   ├── CancelCause.cs                # 取消原因
│   ├── StationEventArgs.cs           # 节点事件参数
│   ├── ScryEventArgs.cs              # 侦察事件参数
│   ├── ScryInfo.cs                   # 侦察信息
│   ├── UsUsingEventArgs.cs           # 使用Us事件参数
│   ├── GameEventArgs.cs              # 游戏事件参数
│   └── FriendCostInfo.cs             # 好友花费信息
│   └── FriendCostType.cs             # 好友花费类型
│
├── 游戏事件系统
│   ├── GameEvent.cs                  # 游戏事件
│   ├── GameEventArgs.cs              # 游戏事件参数基类
│   ├── GameEventHandler.cs           # 游戏事件处理器
│   ├── GameEventHandlerHolder.cs     # 游戏事件处理器持有者
│   └── GameEventPriority.cs          # 游戏事件优先级
│
├── 平台相关
│   ├── PlatformHandler.cs            # 平台处理器
│   ├── PlatformHandlers/
│   │   ├── StandalonePlatformHandler.cs # 独立平台处理器
│   │   ├── SteamPlatformHandler.cs   # Steam平台处理器
│   │   └── EditorPlatformHandler.cs  # 编辑器平台处理器
│   └── CrossPlatformHelper.cs        # 跨平台辅助
│
├── 属性系统
│   ├── Attributes/
│   │   └── LocalizableAttribute.cs   # 可本地化特性
│   ├── IInitializable.cs             # 可初始化接口
│   ├── INotifyChanged.cs             # 通知改变接口
│   ├── INotifyActivating.cs          # 通知激活接口
│   ├── IVerifiable.cs                # 可验证接口
│   ├── IGameRunAchievementHandler.cs # 游戏成就处理器接口
│   ├── IGameRunVisualTrigger.cs      # 游戏视觉触发器接口
│   ├── IMapModeOverrider.cs          # 地图模式覆盖接口
│   ├── IExhibitWeighter.cs           # 宝物权重接口
│   ├── IDisplayWord.cs               # 显示词接口
│   └── RuntimeCommandAttribute.cs    # 运行时命令特性
│   └── RuntimeCommandHandler.cs      # 运行时命令处理器
│
├── 名称与本地化
│   ├── Localization.cs               # 本地化
│   ├── LocalizationExtensions.cs     # 本地化扩展
│   ├── Locale.cs                     # 区域设置
│   ├── LocaleExtensions.cs           # 区域设置扩展
│   ├── EntityName.cs                 # 实体名称
│   ├── EntityNameTable.cs            # 实体名称表
│   ├── UnitName.cs                   # 单位名称
│   ├── UnitNameTable.cs              # 单位名称表
│   ├── UnitNameStyle.cs              # 单位名称样式
│   ├── Keywords.cs                   # 关键词
│   ├── KeywordDisplayWord.cs         # 关键词显示词
│   ├── PuzzleFlags.cs                # 谜题标志
│   ├── PuzzleFlag.cs                 # 谜题标志
│   ├── PuzzleFlagDisplayWord.cs      # 谜题标志显示词
│   ├── NounCase.cs                   # 名词格
│   └── StringDecorator.cs            # 字符串装饰器
│
├── 杂项工具与扩展
│   ├── Utils.cs                      # 工具类
│   ├── ExpHelper.cs                  # 经验辅助
│   ├── MiscExtensions.cs             # 杂项扩展
│   ├── SemVer.cs                     # 语义化版本
│   ├── Singleton.cs                  # 单例基类
│   ├── OrderedList.cs                # 有序列表
│   ├── FaultTolerantArray.cs         # 容错数组
│   ├── TypeFactory.cs                # 类型工厂
│   ├── RuntimeFormatter.cs           # 运行时格式化器
│   ├── RuntimeFormatterArgmentHandler.cs # 运行时格式化参数处理器
│   ├── RuntimeFormatterExtensions.cs # 运行时格式化器扩展
│   ├── DrawZoneTarget.cs             # 抽牌区目标
│   ├── AddCardsType.cs               # 添加卡牌类型
│   ├── VersionInfo.cs                # 版本信息
│   ├── VisualSourceData.cs           # 视觉源数据
│   ├── VisualSourceType.cs           # 视觉源类型
│   ├── UnitySourceGeneratedAssemblyMonoScriptTypes_v1.cs # Unity生成代码
│   ├── System/                       # 系统级扩展
│   │   └── Runtime/CompilerServices/
│   │       └── NullableAttribute.cs  # 可空特性
│   └── Microsoft/
│       └── CodeAnalysis/
│           └── EmbeddedAttribute.cs  # 嵌入式特性
│
└── 项目文件
    ├── LBoL.Core.csproj              # 项目文件
    └── Properties/
        └── AssemblyInfo.cs           # 程序集信息

```

## 📊 核心系统统计

| 系统类别 | 文件数量 | 核心文件 |
|---------|---------|---------|
| 战斗系统 | 80+ | BattleController.cs, BattleAction.cs |
| 卡牌系统 | 10+ | Card.cs, CardZone.cs |
| 单位系统 | 15+ | Unit.cs, PlayerUnit.cs, EnemyUnit.cs |
| 状态效果 | 40+ | StatusEffect.cs, 各种具体效果 |
| 意图系统 | 20+ | Intention.cs, AttackIntention.cs |
| 地图节点 | 15+ | Station.cs, 各种节点类型 |
| 保存系统 | 20+ | SaveDataHelper.cs, 各种存档类 |
| 事件系统 | 30+ | GameEvent.cs, 各种事件参数 |
| 其他系统 | 50+ | 杂项工具和基础类 |

**总计：约 200+ 个核心代码文件**

## 🎯 关键入口点

1. **游戏启动**：`GameRunController.cs` - 控制整个游戏流程
2. **战斗核心**：`BattleController.cs` - 管理所有战斗逻辑
3. **卡牌核心**：`Card.cs` - 卡牌系统的基础
4. **单位核心**：`Unit.cs` - 所有单位的基类
5. **行动核心**：`BattleAction.cs` - 所有战斗行动的基础

## 🔧 插件开发关注重点

对于 Mod 开发，重点关注：
- `Card.cs` - 自定义卡牌
- `StatusEffect.cs` - 自定义状态效果
- `Exhibit.cs` - 自定义宝物
- `Adventure.cs` - 自定义冒险事件
- `Station.cs` - 自定义地图节点
- `Intention.cs` - 自定义敌人意图
- `BattleAction.cs` - 自定义战斗行动
