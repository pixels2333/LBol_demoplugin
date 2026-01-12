
> 游戏所有模块代码文件完整清单
> 版本：1.0
> 总代码文件数：1255+

```
lbol/ (根目录)
├── LBoL.Core/                    # 核心游戏逻辑 (200+文件)
├── LBoL.Base/                    # 基础类型与扩展 (30+文件)
├── LBoL.ConfigData/              # 配置数据 (25+文件)
├── LBoL.EntityLib/               # 游戏实体库 (600+文件) ⭐
└── LBoL.Presentation/            # 表现层 (400+文件)
```

---

## 🎮 LBoL.Core - 核心游戏逻辑 (200+文件)

**核心框架、战斗系统、游戏循环**

<details>
<summary>点击查看完整目录</summary>

```
Battle/                           # 战斗系统核心
├── BattleController.cs           # 战斗控制器（核心）
├── ActionResolver.cs             # 行动解析器
├── BattleAction.cs               # 战斗行动基类
├── Phase.cs                      # 战斗阶段
├── BattleStatus.cs               # 战斗状态
├── BattleMessage.cs              # 战斗消息
├── Interaction.cs                # 交互基类
├── InteractionViewer.cs          # 交互查看器
├── BattleActionViewer.cs         # 战斗行动查看器
├── ActionViewer.cs               # 行动查看器
├── DieCause.cs                   # 死亡原因
├── ActionCause.cs                # 行动原因
├── ICustomCounter.cs             # 自定义计数器接口
├── CustomCounterResetTiming.cs   # 计数器重置时机
├── EventBattleAction.cs          # 事件战斗行动
├── EventSequencedReactor.cs      # 事件序列反应器
├── LazyActionReactor.cs          # 延迟行动反应器
├── LazySequencedReactor.cs       # 延迟序列反应器
├── Reactor.cs                    # 反应器
├── SimpleAction.cs               # 简单行动
├── SimpleEventBattleAction.cs    # 简单事件战斗行动
├── BattleActionRecord/           # 战斗行动记录
│   ├── ActionRecord.cs           # 行动记录
│   └── PhaseRecord.cs            # 阶段记录
└── Interactions/                 # 交互实现
    ├── SelectCardInteraction.cs  # 选择卡牌交互
    ├── UpgradeCardInteraction.cs # 升级卡牌交互
    ├── TransformCardInteraction.cs # 变换卡牌交互
    ├── RemoveCardInteraction.cs  # 移除卡牌交互
    ├── RewardInteraction.cs      # 奖励交互
    ├── SelectHandInteraction.cs  # 选择手牌交互
    └── MiniSelectCardInteraction.cs # 迷你选择卡牌

BattleActions/                    # 80+战斗行动
├── DamageAction.cs               # 伤害行动
├── HealAction.cs                 # 治疗行动
├── PlayCardAction.cs             # 打牌行动
├── DrawCardAction.cs             # 抽牌行动
├── GainManaAction.cs             # 获得法力
├── StartPlayerTurnAction.cs      # 开始玩家回合
├── EndPlayerTurnAction.cs        # 结束玩家回合
├── ApplyStatusEffectAction.cs    # 应用状态效果
├── AddCardsToHandAction.cs       # 添加卡牌到手牌
├── UpgradeCardAction.cs          # 升级卡牌
├── TransformCardAction.cs        # 变换卡牌
└── ...（70+更多行动）

Cards/                            # 卡牌系统
├── Card.cs                       # 卡牌基类（核心）
├── CardZone.cs                   # 卡牌区域
├── GunType.cs                    # 枪械类型
├── Guns.cs                       # 枪械
├── GunPair.cs                    # 枪对
├── WhiteLaser.cs                 # 白色激光
├── NightMana1-4.cs               # 夜之法力1-4
├── Zhukeling.cs                  # 竹雀灵
├── Xiaozhuo.cs                   # 小灼
├── CirnoFreeze.cs                # 琪露诺冰冻
├── ManaFreezer.cs                # 法力冻结
└── FriendToken.cs                # 好友标记

Units/                            # 单位系统
├── Unit.cs                       # 单位基类（核心）
├── PlayerUnit.cs                 # 玩家单位
├── EnemyUnit.cs                  # 敌人单位
├── EnemyUnit.2.cs                # 敌人单位扩展
├── Doll.cs                       # 人偶系统
├── BattleSet.cs                  # 战斗配置
├── EnemyGroup.cs                 # 敌人群组
├── Intention.cs                  # 敌人意图基类
├── IntentionType.cs              # 意图类型
├── IEnemyMove.cs                 # 敌人移动接口
├── SimpleEnemyMove.cs            # 简单敌人移动
├── IEnemyUnitView.cs             # 敌人单位视图接口
├── IPlayerUnitView.cs            # 玩家单位视图接口
├── IUnitView.cs                  # 单位视图接口
├── PlayerType.cs                 # 玩家类型
├── UnitStatus.cs                 # 单位状态
└── UltimateSkill.cs              # 终极技能

StatusEffects/                    # 40+状态效果
├── StatusEffect.cs               # 状态效果基类
├── Weak.cs                       # 虚弱
├── Vulnerable.cs                 # 易伤
├── Invincible.cs                 # 无敌
├── Grace.cs                      # 优雅（护盾）
├── Firepower.cs                  # 火力
├── Spirit.cs                     # 灵力
├── Control.cs                    # 控制
├── Charging.cs                   # 充能
├── Burst.cs                      # 爆发
├── Mood.cs                       # 情绪
├── ExtraTurn.cs                  # 额外回合
└── ...（25+更多效果）

Intentions/                       # 20+敌人意图
├── AttackIntention.cs            # 攻击意图
├── DefendIntention.cs            # 防御意图
├── SleepIntention.cs             # 睡眠意图
├── HealIntention.cs              # 治疗意图
├── EscapeIntention.cs            # 逃跑意图
├── ExplodeIntention.cs           # 爆炸意图
├── DoNothingIntention.cs         # 无行动意图
└── ...（15+更多意图）

Stations/                         # 15+地图节点
├── Station.cs                    # 节点基类
├── BattleStation.cs              # 战斗节点
├── EliteEnemyStation.cs          # 精英敌人节点
├── BossStation.cs                # Boss节点
├── ShopStation.cs                # 商店节点
├── GapStation.cs                 # 间隙节点
├── SupplyStation.cs              # 补给节点
├── TradeStation.cs               # 交易节点
└── ...（10+节点类型）

Adventures/                       # 冒险事件
├── Adventure.cs                  # 冒险事件基类
├── FakeAdventure.cs              # 虚假冒险
└── AdventureInfoAttribute.cs     # 冒险信息特性

GapOptions/                       # 间隙选项
├── GapOption.cs                  # 间隙选项基类
├── UpgradeCard.cs                # 升级卡牌
├── RemoveCard.cs                 # 移除卡牌
├── FindExhibit.cs                # 寻找宝物
├── GetRareCard.cs                # 获得稀有卡牌
└── ...

SaveData/                         # 20+存档数据
├── SaveDataHelper.cs             # 存档辅助
├── GameRunSaveData.cs            # 游戏运行存档
├── PlayerSaveData.cs             # 玩家存档
├── CardSaveData.cs               # 卡牌存档
├── ExhibitSaveData.cs            # 宝物存档
├── StageSaveData.cs              # 关卡存档
└── ...（15+更多存档类）

Stats/                            # 统计
├── GameRunStats.cs               # 游戏运行统计
└── BattleStats.cs                # 战斗统计

Randoms/                          # 随机系统
├── IRandomPool.cs                # 随机池接口
├── RepeatableRandomPool.cs       # 可重复随机池
├── UniqueRandomPool.cs           # 唯一随机池
├── CardWeightTable.cs            # 卡牌权重表
└── ExhibitWeightTable.cs         # 宝物权重表

Dialogs/                          # 对话系统
├── DialogRunner.cs               # 对话运行器
├── DialogProgram.cs              # 对话程序
├── DialogPhase.cs                # 对话阶段基类
├── DialogLinePhase.cs            # 对话行阶段
├── DialogOptionsPhase.cs         # 对话选项阶段
└── ...

核心类
├── GameRunController.cs          # 游戏运行控制器（核心入口）
├── GameEntity.cs                 # 游戏实体基类
├── GameMap.cs                    # 游戏地图
├── MapNode.cs                    # 地图节点
├── Stage.cs                      # 游戏关卡
├── Library.cs                    # 游戏内容库
├── GlobalConfig.cs               # 全局配置
├── GameMode.cs                   # 游戏模式
├── GameDifficulty.cs             # 游戏难度
├── GameResultType.cs             # 游戏结果类型
├── GameRunStatus.cs              # 游戏运行状态
├── MainMenuStatus.cs             # 主菜单状态
├── GameEvent.cs                  # 游戏事件
├── GameEventArgs.cs              # 游戏事件参数基类
├── GameEventHandler.cs           # 游戏事件处理器
├── GameEventHandlerHolder.cs     # 游戏事件处理器持有者
├── BlockInfo.cs                  # 格挡信息
├── DamageInfo.cs                 # 伤害信息
├── ShieldInfo.cs                 # 护盾信息
├── FriendCostInfo.cs             # 好友花费信息
├── CardEventArgs.cs              # 卡牌事件参数
├── StatusEffectEventArgs.cs      # 状态效果事件参数
├── UnitEventArgs.cs              # 单位事件参数
├── DieEventArgs.cs               # 死亡事件参数
├── HealEventArgs.cs              # 治疗事件参数
├── ManaEventArgs.cs              # 法力事件参数
├── PowerEventArgs.cs             # 能量事件参数
├── Exhibit.cs                    # 宝物基类
├── JadeBox.cs                    # 玉盒基类
└── ...

平台相关
├── PlatformHandler.cs            # 平台处理器
├── PlatformHandlers/
│   ├── StandalonePlatformHandler.cs # 独立平台
│   ├── SteamPlatformHandler.cs   # Steam平台
│   └── EditorPlatformHandler.cs  # 编辑器平台
└── CrossPlatformHelper.cs        # 跨平台辅助

本地化
├── Localization.cs               # 本地化
├── Locale.cs                     # 区域设置
├── EntityName.cs                 # 实体名称
├── UnitName.cs                   # 单位名称
├── Keywords.cs                   # 关键词
└── ...

其他
├── Utils.cs                      # 工具类
├── ExpHelper.cs                  # 经验辅助
├── RuntimeCommandAttribute.cs    # 运行时命令特性
├── RuntimeCommandHandler.cs      # 运行时命令处理器
├── Singleton.cs                  # 单例基类
└── Properties/
    └── AssemblyInfo.cs           # 程序集信息
```

</details>

---

## 📦 LBoL.Base - 基础类型与扩展 (30+文件)

**基础数据类型、枚举和扩展方法**

<details>
<summary>点击查看完整目录</summary>

```
基础数据类型
├── ManaColor.cs                    # 法力颜色
├── ManaColors.cs                   # 法力颜色集合
├── ManaGroup.cs                    # 法力组
├── BaseManaGroup.cs                # 基础法力组
├── CardType.cs                     # 卡牌类型
├── Rarity.cs                       # 稀有度
├── DamageType.cs                   # 伤害类型
├── StatusEffectType.cs             # 状态效果类型
├── EnemyType.cs                    # 敌人类型
├── TargetType.cs                   # 目标类型
├── StackType.cs                    # 堆叠类型
├── GapOptionType.cs                # 间隙选项类型
├── ExhibitLosableType.cs           # 宝物可失去类型
├── DurationDecreaseTiming.cs       # 持续时间减少时机
├── UsRepeatableType.cs             # Us可重复类型
├── AppearanceType.cs               # 出现类型
├── MinMax.cs                       # 最小最大值
└── Keyword.cs                      # 关键词

属性与特性
└── KeywordAttribute.cs             # 关键词特性

转换器
├── ManaColorConverter.cs           # 法力颜色转换器
├── ManaGroupConverter.cs           # 法力组转换器
├── BaseManaGroupConverter.cs       # 基础法力组转换器
└── MinMaxConverter.cs              # 最小最大值转换器

扩展方法
├── Extensions/
│   ├── BasicTypeExtensions.cs      # 基础类型扩展
│   ├── CollectionsExtensions.cs    # 集合扩展
│   ├── MathExtensions.cs           # 数学扩展
│   ├── TypeExtensions.cs           # 类型扩展
│   └── TransformExtensions.cs      # 变换扩展
├── ManaColorExtensions.cs          # 法力颜色扩展
└── ManaGroupExtensions.cs          # 法力组扩展

数据结构
├── AssociationList.cs              # 关联列表
└── PriorityQueue.cs                # 优先队列

随机数生成
└── RandomGen.cs                    # 随机数生成器

Properties/
└── AssemblyInfo.cs                 # 程序集信息
```

</details>

---

## ⚙️ LBoL.ConfigData - 配置数据 (25+文件)

**游戏配置文件和数据管理**

<details>
<summary>点击查看完整目录</summary>

```
配置管理器
└── ConfigDataManager.cs            # 配置数据管理器

角色配置
├── PlayerUnitConfig.cs             # 玩家单位配置
└── EnemyUnitConfig.cs              # 敌人单位配置

卡牌配置
├── CardConfig.cs                   # 卡牌配置
└── UltimateSkillConfig.cs          # 终极技能配置

敌人配置
└── EnemyGroupConfig.cs             # 敌人群组配置

宝物与道具
├── ExhibitConfig.cs                # 宝物配置
└── JadeBoxConfig.cs                # 玉盒配置

关卡与冒险
├── StageConfig.cs                  # 关卡配置
└── AdventureConfig.cs              # 冒险配置

系统配置
├── RuleConfig.cs                   # 规则配置
├── SpellConfig.cs                  # 符卡配置
└── PuzzleConfig.cs                 # 谜题配置

音效配置
├── BgmConfig.cs                    # 背景音乐配置
├── SfxConfig.cs                    # 音效配置
└── UiSoundConfig.cs                # UI音效配置

视觉效果
├── EffectConfig.cs                 # 效果配置
├── BulletConfig.cs                 # 子弹配置
├── GunConfig.cs                    # 枪械配置
├── LaserConfig.cs                  # 激光配置
└── PieceConfig.cs                  # 碎片配置

模型配置
└── UnitModelConfig.cs              # 单位模型配置

其他配置
├── ExpConfig.cs                    # 经验配置
├── SequenceConfig.cs               # 序列配置
└── SpineEventConfig.cs             # Spine事件配置

Properties/
└── AssemblyInfo.cs                 # 程序集信息
```

</details>

---

## 🎴 LBoL.EntityLib - 游戏实体库 (600+文件) ⭐

**具体的游戏内容实体（卡牌、敌人、冒险等）**

<details>
<summary>点击查看完整目录</summary>

### 卡牌 - 角色专属 (300+文件)

```
Cards/Character/
├── Alice/ (28 files)              # 爱丽丝 - 人偶使
│   ├── AliceAttackB.cs           # 攻击·蓝
│   ├── AliceAttackU.cs           # 攻击·紫
│   ├── AliceBlockB.cs            # 防御·蓝
│   ├── AliceBlockU.cs            # 防御·紫
│   ├── DeployShanghai.cs         # 配置上海
│   ├── DeployPenglai.cs          # 配置蓬莱
│   ├── DollFactory.cs            # 人偶工厂
│   ├── DollFormation.cs          # 人偶阵型
│   ├── DollFire.cs               # 人偶之火
│   ├── DollBlock.cs              # 人偶防御
│   ├── TriggerAllPassive.cs      # 触发所有被动
│   └── ...
│
├── Cirno/ (45 files)              # 琪露诺 - 冰之妖精  ⭐最多
│   ├── CirnoAttackG.cs           # 攻击·绿
│   ├── CirnoAttackU.cs           # 攻击·紫
│   ├── CirnoBlockG.cs            # 防御·绿
│   ├── CirnoBlockU.cs            # 防御·紫
│   ├── FreezeBullet.cs           # 冰冻弹幕
│   ├── IceBarrier.cs             # 冰之壁垒
│   ├── ColdSnap.cs               # 寒冷爆发
│   ├── FairyTeam.cs              # 妖精小队
│   ├── CallFriends.cs            # 呼叫朋友
│   ├── Friend/ (9 files)         # 朋友系列
│   │   ├── LilyFriend.cs
│   │   ├── LunaFriend.cs
│   │   ├── StarFriend.cs
│   │   └── ...
│   └── ...
│
├── Reimu/ (35 files)              # 博丽灵梦 - 巫女
│   ├── ReimuAttackR.cs           # 攻击·红
│   ├── ReimuAttackW.cs           # 攻击·白
│   ├── ReimuBlockR.cs            # 防御·红
│   ├── ReimuBlockW.cs            # 防御·白
│   ├── YinYangCard.cs            # 阴阳玉
│   ├── EvilTerminator.cs         # 恶灵退散
│   ├── SpiritSign.cs             # 灵符
│   └── ...
│
├── Marisa/ (42 files)             # 雾雨魔理沙 - 魔法使
│   ├── MarisaAttackB.cs          # 攻击·蓝
│   ├── MarisaAttackR.cs          # 攻击·红
│   ├── MarisaBlockB.cs           # 防御·蓝
│   ├── MarisaBlockR.cs           # 防御·红
│   ├── MasterSpark.cs            # 极限火花
│   ├── FinalSpark.cs             # 究极火花
│   ├── Potion.cs                 # 药水
│   └── ...
│
├── Sakuya/ (32 files)             # 十六夜咲夜 - 女仆长
│   ├── SakuyaAttackU.cs          # 攻击·紫
│   ├── SakuyaAttackW.cs          # 攻击·白
│   ├── SakuyaBlockU.cs           # 防御·紫
│   ├── SakuyaBlockW.cs           # 防御·白
│   ├── Knife.cs                  # 飞刀
│   ├── TimeStop.cs               # 时间停止
│   ├── LunaDial.cs               # 月时计
│   └── ...
│
└── Koishi/ (38 files)             # 古明地恋 - 觉妖
    ├── KoishiAttackB.cs          # 攻击·蓝
    ├── KoishiAttackG.cs          # 攻击·绿
    ├── KoishiBlockB.cs           # 防御·蓝
    ├── KoishiBlockG.cs           # 防御·绿
    ├── CloseEye.cs               # 闭眼
    ├── InspirationCard.cs        # 灵感卡牌
    ├── Follower.cs               # 使魔
    └── ...
```

### 卡牌 - 中立 (200+文件)

```
Cards/Neutral/
├── NoColor/ (15 files)            # 无色卡牌
│   ├── ManaCard.cs               # 法力卡
│   ├── BManaCard.cs              # 蓝法力卡
│   ├── RManaCard.cs              # 红法力卡
│   ├── Shoot.cs                  # 射击
│   └── ...
│
├── Red/ (15 files)                # 红色（攻击/力量）
│   ├── RedGiantStar.cs           # 红巨星
│   ├── HuoliQuankai.cs           # 活力全开
│   └── ...
│
├── Blue/ (15 files)               # 蓝色（防御/冰）
│   ├── IceBlock.cs               # 冰块
│   ├── FakeMoon.cs               # 幻月
│   └── ...
│
├── Green/ (12 files)              # 绿色（自然/生命）
│   ├── GreenLotus.cs             # 绿莲
│   ├── SunflowerDefense.cs       # 向日葵防御
│   └── ...
│
├── White/ (8 files)               # 白色（神圣/治疗）
│   ├── Guangyu.cs                # 光玉
│   ├── Invincible.cs             # 无敌
│   └── ...
│
├── Black/ (12 files)              # 黑色（暗影/诅咒）
│   ├── Shadow.cs                 # 暗影
│   ├── Curse.cs                  # 诅咒
│   └── ...
│
├── TwoColor/ (25 files)           # 双色卡牌
│   ├── FengleiCard.cs            # 风雷卡
│   ├── ShuihuoCard.cs            # 水火卡
│   └── ...
│
└── MultiColor/ (5 files)          # 多色卡牌
    ├── AnimalSpirit.cs           # 动物灵
    └── ...
```

### 卡牌 - 特殊类型

```
Cards/
├── Tool/ (13 files)               # 道具卡牌
│   ├── ToolAttack.cs             # 攻击道具
│   ├── ToolBlock.cs              # 防御道具
│   ├── ToolHeal.cs               # 治疗道具
│   └── ...
│
├── Enemy/ (8 files)               # 敌人卡牌
│   ├── AyaNews.cs                # 文文新闻
│   ├── Lunatic.cs                # 狂气
│   └── ...
│
├── Misfortune/ (11 files)         # 灾厄卡牌
│   ├── Drunk.cs                  # 醉酒
│   ├── Pressure.cs               # 压力
│   └── ...
│
├── Adventure/ (9 files)           # 冒险事件卡牌
│   ├── GainTreasure.cs           # 获得宝藏
│   ├── NewsEntertainment.cs      # 娱乐新闻
│   └── ...
│
├── DebugCards/ (12 files)         # 调试用卡牌
│   ├── DebugAddHandCards.cs      # 调试添加手牌
│   ├── DebugUpgradeAllZone.cs    # 调试升级全区域
│   └── ...
│
└── Others/ (3 files)              # 其他特殊卡牌
    ├── FakeCard.cs               # 虚假卡牌
    ├── LimiteStopTimeCard.cs     # 限时停止卡牌
    └── HistoryCard.cs            # 历史卡牌
```

### 敌人单位 (100+文件)

```
EnemyUnits/
├── Character/ (30 files)          # 角色Boss/自机
│   ├── Aya.cs                   # 射命丸文
│   ├── Clownpiece.cs            # 克劳恩皮丝
│   ├── Doremy.cs                # 哆来咪·苏伊特
│   ├── Junko.cs                 # 纯狐
│   ├── Remilia.cs               # 蕾米莉亚·斯卡雷特
│   ├── Flandre.cs               # 芙兰朵露·斯卡雷特
│   ├── Kokoro.cs                # 秦心
│   ├── Sanae.cs                 # 东风谷早苗
│   ├── Suika.cs                 # 伊吹萃香
│   ├── Yuyuko.cs                # 西行寺幽幽子
│   ├── Youmu.cs                 # 魂魄妖梦
│   ├── Patchouli.cs             # 帕秋莉·诺蕾姬
│   ├── Marisa.cs                # 雾雨魔理沙
│   ├── Reimu.cs                 # 博丽灵梦
│   ├── Yuji.cs                  # 伊季
│   ├── Nitori.cs                # 河城荷取
│   ├── Rin.cs                   # 火焰猫燐
│   ├── Siji.cs                  # 四季映姬
│   ├── Tianzi.cs                # 比那名居天子
│   ├── Seija.cs                 # 鬼人正邪
│   ├── Sumireko.cs              # 宇佐见堇子
│   ├── Yukari.cs                # 八云紫
│   ├── Koishi.cs                # 古明地恋
│   ├── DreamServants/ (4 files)  # 梦之从者
│   │   ├── DreamAya.cs
│   │   ├── DreamJunko.cs
│   │   └── ...
│   └── ...
│
├── Lore/ (50 files)               # 东方Project角色
│   ├── Patchouli.cs             # 帕秋莉·诺蕾姬
│   ├── Reisen.cs                # 铃仙·优昙华院·因幡
│   ├── Kaguya.cs                # 蓬莱山辉夜
│   ├── Mokou.cs                 # 藤原妹红
│   ├── Keine.cs                 # 上白泽慧音
│   ├── Mystia.cs                # 米斯蒂娅·萝蕾拉
│   ├── Tewi.cs                  # 因幡帝
│   ├── Satori.cs                # 古明地觉
│   ├── Parsee.cs                # 桥姬
│   ├── Nazrin.cs                # 纳兹琳
│   ├── Medicine.cs              # 梅蒂欣·梅兰可莉
│   ├── Hina.cs                  # 键山雏
│   ├── Kogasa.cs                # 多多良小伞
│   ├── Kokoro.cs                # 秦心
│   ├── Shinmyoumaru.cs          # 少名针妙丸
│   ├── Seija.cs                 # 鬼人正邪
│   ├── Yamame.cs                # 黑谷山女
│   ├── Yuugi.cs                 # 星熊勇仪
│   ├── Suika.cs                 # 伊吹萃香
│   ├── Kasen.cs                 # 茨木华扇
│   ├── Miko.cs                  # 丰聪耳神子
│   ├── Futo.cs                  # 物部布都
│   ├── Tojiko.cs                # 苏我屠自古
│   ├── Mamizou.cs               # 二岩猯藏
│   ├── Akyuu.cpp                # 稗田阿求
│   ├── Hatate.cpp               # 姬海棠果
│   ├── Momiji.cpp               # 犬走椛
│   └── ...
│
└── Normal/ (20 files)             # 普通敌人
    ├── Bats/ (2 files)            # 蝙蝠系
    │   ├── Bat.cs
    │   └── BatLord.cs
    │
    ├── Fairies/ (8 files)         # 妖精系
    │   ├── Fairy.cs
    │   ├── FireFairy.cs
    │   ├── IceFairy.cs
    │   └── ...
    │
    ├── Ghosts/ (3 files)          # 幽灵系
    │   ├── Ghost.cs
    │   └── Poltergeist.cs
    │
    └── Dolls/ (1 file)            # 人偶系
        └── ShanghaiDoll.cs
```

### 战斗配置 & 人偶

```
├── BattleSets/ (1 file)          # 战斗配置
│   └── Ravens2.cs
│
└── Dolls/ (5 files)              # 人偶
    ├── Shanghai.cs               # 上海人偶
    ├── Penglai.cs                # 蓬莱人偶
    ├── ChargeDoll.cs             # 充能人偶
    ├── DefenseDoll.cs            # 防御人偶
    └── ManaDoll.cs               # 法力人偶
```

### 冒险事件 (40+文件)

```
Adventures/
├── Common/ (1 file)
│   └── YorigamiSisters.cs
│
├── Debut.cs
│
├── FirstPlace/ (6 files)        # 第一梯队角色
│   ├── DoremyPortal.cs
│   ├── JunkoColorless.cs
│   ├── MiyoiBartender.cs
│   ├── PatchouliPhilosophy.cs
│   ├── ShinmyoumaruForge.cs
│   └── WatatsukiPurify.cs
│
├── RinnosukeTrade.cs
│
├── Shared12/ (4 files)          # 1-2层共享
│   ├── HecatiaTshirt.cs
│   ├── KeineSales.cs
│   ├── MikeInvest.cs
│   └── YoumuDelivery.cs
│
├── Shared23/ (7 files)          # 2-3层共享
│   ├── HatateInterview.cs
│   ├── HinaCollect.cs
│   ├── KogasaSpook.cs
│   ├── KosuzuBookstore.cs
│   ├── NarumiOfferCard.cs
│   └── ...
│
├── Stage1/ (6 files)            # 第一层
│   ├── AssistKagerou.cs
│   ├── EternityAscension.cs
│   ├── KaguyaVersusMokou.cs
│   ├── MystiaBbq.cs
│   ├── ParseeJealousy.cs
│   └── ...
│
├── Stage2/ (4 files)            # 第二层
│   ├── BuduSuanming.cs
│   ├── RemiliaMeet.cs
│   ├── RingoEmp.cs
│   └── YachieOppression.cs
│
├── Stage3/ (4 files)            # 第三层
│   ├── BackgroundDancers.cs
│   ├── MedicinePoison.cs
│   ├── MikoDonation.cs
│   └── SatoriCounseling.cs
│
├── SumirekoGathering.cs
└── Supply.cs
```

</details>

---

## 🎮 LBoL.Presentation - 表现层 (400+文件)

**UI界面、视觉效果、音频管理、输入处理**

<details>
<summary>点击查看完整目录</summary>

```
游戏入口与管理
├── GameEntry.cs                    # 游戏入口
├── GameMaster.cs                   # 游戏主控
├── PlatformHandlerRunner.cs        # 平台处理器运行器
└── FrameSetting.cs                 # 帧设置

环境
└── Environments/
    ├── Environment.cs              # 环境基类
    ├── FinalStageEnvironment.cs  # 最终关卡环境
    └── IntoFinalEffect.cs          # 进入最终关卡效果

子弹与投射物
└── Bullet/
    ├── Bullet.cs                   # 子弹
    ├── Projectile.cs               # 投射物
    ├── Gun.cs                      # 枪械
    ├── GunManager.cs               # 枪械管理器
    ├── Launcher.cs                 # 发射器
    ├── Laser.cs                    # 激光
    ├── Piece.cs                    # 碎片
    ├── HitType.cs                  # 命中类型
    ├── BulletEvent.cs              # 子弹事件
    └── ParticalRotator.cs          # 粒子旋转器

特效
└── Effect/
    ├── EffectManager.cs            # 特效管理器
    ├── EffectWidget.cs             # 特效控件
    ├── EffectBullet.cs             # 特效子弹
    ├── EffectBulletView.cs         # 特效子弹视图
    ├── EffectUIBulletView.cs       # UI特效子弹视图
    ├── ManaFlyEffect.cs            # 法力飞行特效
    ├── ExhibitActivating.cs        # 宝物激活特效
    ├── ExileCoverEffect.cs         # 放逐覆盖特效
    ├── RemoveCoverEffect.cs        # 移除覆盖特效
    └── ...

UI系统
└── UI/
    ├── UiManager.cs                # UI管理器
    ├── UiPanel.cs                  # UI面板
    ├── UiPanel.2.cs                # UI面板扩展
    ├── UiPanelBase.cs              # UI面板基类
    ├── UiBase.cs                   # UI基类
    ├── UiDialog.cs                 # UI对话框
    ├── UiDialogBase.cs             # UI对话框基类
    ├── UiAdventurePanel.cs         # UI冒险面板
    ├── IAdventureHandler.cs        # 冒险处理器接口
    ├── IInputActionHandler.cs      # 输入动作处理器接口
    └── ...

UI面板 (80+文件)
└── UI/Panels/
    ├── MainMenuPanel.cs            # 主菜单面板
    ├── StartGamePanel.cs           # 开始游戏面板
    ├── GameRunVisualPanel.cs       # 游戏运行视觉面板
    ├── SettingPanel.cs             # 设置面板
    ├── ProfilePanel.cs             # 配置文件面板
    ├── MapPanel.cs                 # 地图面板
    ├── BattleNotifier.cs           # 战斗通知器
    ├── PlayBoard.cs                # 游戏板
    ├── CardDetailPanel.cs          # 卡牌详情面板
    ├── ExhibitInfoPanel.cs         # 宝物信息面板
    ├── UltimateSkillPanel.cs       # 终极技能面板
    ├── GapOptionsPanel.cs          # 间隙选项面板
    ├── ShopPanel.cs                # 商店面板
    ├── RewardPanel.cs              # 奖励面板
    ├── HistoryPanel.cs             # 历史面板
    ├── MuseumPanel.cs              # 博物馆面板
    ├── MusicRoomPanel.cs           # 音乐室面板
    ├── CreditsPanel.cs             # 制作组名单面板
    ├── SpellPanel.cs               # 符卡面板
    ├── BossExhibitPanel.cs         # Boss宝物面板
    ├── BattleManaPanel.cs          # 战斗法力面板
    └── ...

UI对话框
└── UI/Dialogs/
    ├── MessageDialog.cs            # 消息对话框
    ├── MessageContent.cs           # 消息内容
    ├── UpgradeCardDialog.cs        # 升级卡牌对话框
    ├── TransformCardDialog.cs      # 变换卡牌对话框
    ├── RemoveCardDialog.cs         # 移除卡牌对话框
    └── ...

UI控件 (120+文件)
└── UI/Widgets/
    ├── CardWidget.cs               # 卡牌控件
    ├── HandCard.cs                 # 手牌控件
    ├── ExhibitWidget.cs            # 宝物控件
    ├── StatusEffectWidget.cs       # 状态效果控件
    ├── UnitInfoWidget.cs           # 单位信息控件
    ├── HealthBar.cs                # 生命条
    ├── DamagePopup.cs              # 伤害弹出
    ├── BaseManaWidget.cs           # 基础法力控件
    ├── BattleManaWidget.cs         # 战斗法力控件
    ├── EndTurnButtonWidget.cs      # 结束回合按钮控件
    ├── MapNodeWidget.cs            # 地图节点控件
    ├── TooltipWidget.cs            # 提示控件
    ├── RewardWidget.cs             # 奖励控件
    ├── ShopCard.cs                 # 商店卡牌
    └── ...

单位显示
└── Units/
    ├── UnitView.cs                 # 单位视图
    ├── DollView.cs                 # 人偶视图
    ├── DollSlotView.cs             # 人偶槽位视图
    ├── EnemyFormation.cs           # 敌人群组
    ├── GameDirector.cs             # 游戏导演
    └── SpecialUnits/
        └── KokoroUnitController.cs # 秦心单位控制器

UI过渡
└── UI/Transitions/
    ├── UiTransition.cs             # UI过渡基类
    ├── SimpleTransition.cs         # 简单过渡
    ├── AnimationTransition.cs      # 动画过渡
    ├── MapTransition.cs            # 地图过渡
    ├── GameResultTransition.cs     # 游戏结果过渡
    └── ...

UI额外控件
└── UI/ExtraWidgets/
    ├── AchievementWidget.cs        # 成就控件
    ├── DollTooltipSource.cs        # 人偶提示源
    ├── ExhibitTooltipSource.cs     # 宝物提示源
    ├── IntentionTooltipSource.cs   # 意图提示源
    ├── HandCard.cs                 # 手牌
    ├── ShopCard.cs                 # 商店卡牌
    └── ...

输入系统
└── InputSystemExtend/
    ├── InputDeviceManager.cs       # 输入设备管理器
    ├── InputDeviceType.cs          # 输入设备类型
    ├── GamepadNavigationManager.cs # 手柄导航管理器
    ├── GamepadBehaviour.cs         # 手柄行为
    ├── GamepadButton.cs            # 手柄按钮
    └── ...

国际化
└── I10N/
    ├── L10nManager.cs              # 本地化管理器
    ├── L10nInfo.cs                 # 本地化信息
    ├── LocalizedText.cs            # 本地化文本
    └── LocalizedGameObject.cs      # 本地化游戏对象

其他
├── AudioManager.cs                 # 音频管理器
├── CameraController.cs             # 摄像机控制器
├── OutlineCameraController.cs    # 轮廓摄像机控制器
├── HighlightManager.cs             # 高亮管理器
├── ResolutionHelper.cs             # 分辨率助手
├── ResourcesHelper.cs              # 资源助手
├── Touchable.cs                    # 可触摸接口
├── FrameSetting.cs                 # 帧设置
├── Animations/
│   └── SingleAnimationClipPlayer.cs
├── YukariRoom.cs                   # 紫室
└── Properties/
    └── AssemblyInfo.cs             # 程序集信息
```

</details>

---

## 📊 完整项目统计

### 模块概览

| 模块 | 文件数 | 占比 | 核心内容 |
|------|--------|------|---------|
| **LBoL.Core** | ~200 | 16% | 游戏循环、战斗逻辑、状态机 |
| **LBoL.Base** | ~30 | 2% | 基础类型、枚举、扩展方法 |
| **LBoL.ConfigData** | ~25 | 2% | 游戏配置、平衡参数 |
| **LBoL.EntityLib** | **~600** | **48%** | **卡牌、敌人、冒险事件** ⭐ |
| **LBoL.Presentation** | **~400** | **32%** | **UI、特效、音频、输入** ⭐ |
| **总计** | **1255+** | **100%** | 完整游戏代码库 |

### 详细统计

#### 📋 LBoL.Core (200+ 文件)
- 战斗系统：80+ 行动
- 状态效果：40+ 种
- 敌人意图：20+ 种
- 地图节点：15+ 种
- 存档数据：20+ 类
- 事件系统：30+ 参数类

#### 🎴 LBoL.EntityLib (600+ 文件) - 主要内容

**卡牌 (500+ 张)**
- 角色专属卡：300+ 张
  - 琪露诺：45张 ⭐
  - 魔理沙：42张
  - 恋恋：38张
  - 灵梦：35张
  - 咲夜：32张
  - 爱丽丝：28张
- 中立卡牌：200+ 张
  - 无色：15张
  - 红色：15张
  - 蓝色：15张
  - 绿色：12张
  - 白色：8张
  - 黑色：12张
  - 双色：25张
  - 多色：5张
- 道具：13张
- 调试用：12张

**敌人单位 (100+ 个)**
- 角色Boss/自机：30个
- 东方Project角色：50+个
- 普通敌人：20+个（蝙蝠、妖精、幽灵、人偶）

**冒险事件**：40+个

**人偶**：5个（上海、蓬莱、充能、防御、法力）

#### 🎨 LBoL.Presentation (400+ 文件)
- UI面板：80+个
- UI控件：120+个
- 特效：30+个
- 输入处理：15+个
- 过渡动画：10+个

---

## 🗺️ 项目架构图

```
┌─────────────────────────────────────────────────────────────┐
│                    LBoL.Presentation                        │
│  (UI/特效/音频/输入 - 400文件)                               │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐    │
│  │  UI面板(80+) │  │  UI控件(120+)│  │  特效(30+)  │    │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘    │
│         │                 │                 │             │
└─────────┼─────────────────┼─────────────────┼─────────────┘
          │                 │                 │
          ▼                 ▼                 ▼
┌─────────────────────────────────────────────────────────────┐
│                      LBoL.EntityLib                         │
│  (游戏内容实体 - 600文件) ⭐                                  │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐    │
│  │  卡牌(500+)  │  │  敌人(100+)  │  │  事件(40+)  │    │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘    │
│         │                 │                 │             │
└─────────┼─────────────────┼─────────────────┼─────────────┘
          │                 │                 │
          ▼                 ▼                 ▼
┌─────────────────────────────────────────────────────────────┐
│                       LBoL.Core                             │
│  (游戏核心逻辑 - 200文件)                                    │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐    │
│  │  战斗系统    │  │  状态效果    │  │  游戏循环    │    │
│  │  (80+行动)  │  │  (40+效果)  │  │  (控制器)    │    │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘    │
│         │                 │                 │             │
└─────────┼─────────────────┼─────────────────┼─────────────┘
          │                 │                 │
┌─────────▼─────────────────▼─────────────────▼─────────────┐
│         LBoL.Base          │     LBoL.ConfigData          │
│  (基础类型-30文件)         │  (配置数据-25文件)           │
│  枚举、扩展、数据结构      │  游戏参数、平衡设置          │
└─────────────────────────────────────────────────────────────┘
```

---

## 🎯 开发指南

### MOD开发重点关注区域

#### 1. **新增角色卡牌** → LBoL.EntityLib/Cards/Character/YourCharacter/
```csharp
// 示例：创建新角色卡牌
public class YourCard : Card {
    // 实现卡牌逻辑
}
```

#### 2. **新增敌人** → LBoL.EntityLib/EnemyUnits/
```csharp
// 示例：创建新敌人
public class YourEnemy : EnemyUnit {
    // 实现敌人逻辑
}
```

#### 3. **新增冒险事件** → LBoL.EntityLib/Adventures/
```csharp
// 示例：创建新冒险
public class YourAdventure : Adventure {
    // 实现冒险逻辑
}
```

#### 4. **修改游戏平衡** → LBoL.ConfigData/*.cs
```csharp
// 修改卡牌配置、掉落率等
CardConfig.cs
ExpConfig.cs
```

#### 5. **自定义UI** → LBoL.Presentation/UI/
```csharp
// 自定义界面
UiPanels/YourPanel.cs
UiWidgets/YourWidget.cs
```

#### 6. **核心机制修改** → LBoL.Core/
```csharp
// 修改核心战斗逻辑
BattleController.cs
BattleAction.cs
StatusEffect.cs
```

---

## 📈 代码行数估算

基于文件数量和平均大小估算：

| 模块 | 文件数 | 估算行数 | 占比 |
|------|--------|---------|------|
| LBoL.Core | 200 | 40,000 | 22% |
| LBoL.Base | 30 | 5,000 | 3% |
| LBoL.ConfigData | 25 | 4,000 | 2% |
| LBoL.EntityLib | 600 | 80,000 | 45% |
| LBoL.Presentation | 400 | 50,000 | 28% |
| **总计** | **1255** | **~179,000** | **100%** |

**估算总代码量：约18万行C#代码**

---

## 🔍 文件引用统计

最常见的基础类引用：

1. **Card.cs** - 被500+个卡牌文件引用
2. **EnemyUnit.cs** - 被100+个敌人文件引用
3. **StatusEffect.cs** - 被40+个状态效果文件引用
4. **BattleAction.cs** - 被80+个行动文件引用
5. **Unit.cs** - 被所有单位相关文件引用

---

## 🚀 快速导航

- **核心**：`lbol/LBoL.Core/Battle/BattleController.cs`
- **卡牌**：`lbol/LBoL.EntityLib/Cards/Character/`
- **敌人**：`lbol/LBoL.EntityLib/EnemyUnits/`
- **配置**：`lbol/LBoL.ConfigData/`
- **UI**：`lbol/LBoL.Presentation/UI/Panels/`
- **特效**：`lbol/LBoL.Presentation/Effect/`

---

## 📚 相关文档

- [核心代码文档](lbol-methods-doc.md) - LBoL.Core详细说明
- [非核心代码文档](lbol-noncore-methods-doc.md) - 其他模块详细说明

---

**文档版本**：1.0
**最后更新**：2025-11-24
**总代码文件**：1255+
**估算代码行数**：~179,000行
