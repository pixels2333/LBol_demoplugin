# LBoL 非核心模块代码文件目录结构

> 游戏非核心功能模块代码文件清单（按目录结构排列）
> 包含：LBoL.Base、LBoL.ConfigData、LBoL.EntityLib、LBoL.Presentation

## 📦 LBoL.Base - 基础类型与扩展

基础数据类型、枚举和扩展方法库

```
lbol/LBoL.Base/
├── 基础数据类型
│   ├── ManaColor.cs                  # 法力颜色
│   ├── ManaColors.cs                 # 法力颜色集合
│   ├── ManaGroup.cs                  # 法力组
│   ├── BaseManaGroup.cs              # 基础法力组
│   ├── CardType.cs                   # 卡牌类型
│   ├── Rarity.cs                     # 稀有度
│   ├── DamageType.cs                 # 伤害类型
│   ├── StatusEffectType.cs           # 状态效果类型
│   ├── EnemyType.cs                  # 敌人类型
│   ├── TargetType.cs                 # 目标类型
│   ├── StackType.cs                  # 堆叠类型
│   ├── GapOptionType.cs              # 间隙选项类型
│   ├── ExhibitLosableType.cs         # 宝物可失去类型
│   ├── DurationDecreaseTiming.cs     # 持续时间减少时机
│   ├── UsRepeatableType.cs           # Us可重复类型
│   ├── AppearanceType.cs             # 出现类型
│   ├── MinMax.cs                     # 最小最大值
│   └── Keyword.cs                    # 关键词
│
├── 属性与特性
│   ├── KeywordAttribute.cs           # 关键词特性
│
├── 转换器
│   ├── ManaColorConverter.cs         # 法力颜色转换器
│   ├── ManaGroupConverter.cs         # 法力组转换器
│   ├── BaseManaGroupConverter.cs     # 基础法力组转换器
│   └── MinMaxConverter.cs            # 最小最大值转换器
│
├── 扩展方法
│   ├── Extensions/
│   │   ├── BasicTypeExtensions.cs    # 基础类型扩展
│   │   ├── CollectionsExtensions.cs  # 集合扩展
│   │   ├── MathExtensions.cs         # 数学扩展
│   │   ├── TypeExtensions.cs         # 类型扩展
│   │   └── TransformExtensions.cs    # 变换扩展
│   ├── ManaColorExtensions.cs        # 法力颜色扩展
│   └── ManaGroupExtensions.cs        # 法力组扩展
│
├── 数据结构
│   ├── AssociationList.cs            # 关联列表
│   └── PriorityQueue.cs              # 优先队列
│
├── 随机数生成
│   └── RandomGen.cs                  # 随机数生成器
│
└── Properties/
    └── AssemblyInfo.cs               # 程序集信息
```

---

## ⚙️ LBoL.ConfigData - 配置数据

游戏配置文件和数据管理

```
lbol/LBoL.ConfigData/
├── 配置管理器
│   └── ConfigDataManager.cs          # 配置数据管理器
│
├── 角色配置
│   ├── PlayerUnitConfig.cs           # 玩家单位配置
│   └── EnemyUnitConfig.cs            # 敌人单位配置
│
├── 卡牌配置
│   ├── CardConfig.cs                 # 卡牌配置
│   └── UltimateSkillConfig.cs        # 终极技能配置
│
├── 敌人配置
│   └── EnemyGroupConfig.cs           # 敌人群组配置
│
├── 宝物与道具
│   ├── ExhibitConfig.cs              # 宝物配置
│   └── JadeBoxConfig.cs              # 玉盒配置
│
├── 关卡与冒险
│   ├── StageConfig.cs                # 关卡配置
│   └── AdventureConfig.cs            # 冒险配置
│
├── 系统配置
│   ├── RuleConfig.cs                 # 规则配置
│   ├── SpellConfig.cs                # 符卡配置
│   └── PuzzleConfig.cs               # 谜题配置
│
├── 音效配置
│   ├── BgmConfig.cs                  # 背景音乐配置
│   ├── SfxConfig.cs                  # 音效配置
│   └── UiSoundConfig.cs              # UI音效配置
│
├── 视觉效果
│   ├── EffectConfig.cs               # 效果配置
│   ├── BulletConfig.cs               # 子弹配置
│   ├── GunConfig.cs                  # 枪械配置
│   ├── LaserConfig.cs                # 激光配置
│   └── PieceConfig.cs                # 碎片配置
│
├── 模型配置
│   └── UnitModelConfig.cs            # 单位模型配置
│
├── 其他配置
│   ├── ExpConfig.cs                  # 经验配置
│   ├── SequenceConfig.cs             # 序列配置
│   └── SpineEventConfig.cs           # Spine事件配置
│
└── Properties/
    └── AssemblyInfo.cs               # 程序集信息
```

---

## 🎴 LBoL.EntityLib - 游戏实体库

具体的游戏内容实体（卡牌、敌人、冒险等）

```
lbol/LBoL.EntityLib/
├── 卡牌 - 角色专属 (Cards/Character/)
│   ├── Alice/                        # 爱丽丝（人偶使）
│   │   ├── AliceAttackB.cs          # 爱丽丝攻击·蓝
│   │   ├── AliceAttackU.cs          # 爱丽丝攻击·紫
│   │   ├── AliceBlockB.cs           # 爱丽丝防御·蓝
│   │   ├── AliceBlockU.cs           # 爱丽丝防御·紫
│   │   ├── DeployShanghai.cs        # 配置上海
│   │   ├── DeployPenglai.cs         # 配置蓬莱
│   │   ├── DollFactory.cs           # 人偶工厂
│   │   ├── DollFormation.cs         # 人偶阵型
│   │   ├── DollFire.cs              # 人偶之火
│   │   ├── DollBlock.cs             # 人偶防御
│   │   ├── TriggerAllPassive.cs     # 触发所有被动
│   │   └── ...（20+张人偶相关卡牌）
│   │
│   ├── Cirno/                        # 琪露诺（冰之妖精）
│   │   ├── CirnoAttackG.cs          # 琪露诺攻击·绿
│   │   ├── CirnoAttackU.cs          # 琪露诺攻击·紫
│   │   ├── CirnoBlockG.cs           # 琪露诺防御·绿
│   │   ├── CirnoBlockU.cs           # 琪露诺防御·紫
│   │   ├── FreezeBullet.cs          # 冰冻弹幕
│   │   ├── IceBarrier.cs            # 冰之壁垒
│   │   ├── ColdSnap.cs              # 寒冷爆发
│   │   ├── FairyTeam.cs             # 妖精小队
│   │   ├── CallFriends.cs           # 呼叫朋友
│   │   └── ...（40+张冰系/妖精卡牌）
│   │
│   ├── Reimu/                        # 博丽灵梦（巫女）
│   │   ├── ReimuAttackR.cs          # 灵梦攻击·红
│   │   ├── ReimuAttackW.cs          # 灵梦攻击·白
│   │   ├── ReimuBlockR.cs           # 灵梦防御·红
│   │   ├── ReimuBlockW.cs           # 灵梦防御·白
│   │   ├── YinYangCard.cs           # 阴阳玉
│   │   ├── EvilTerminator.cs        # 恶灵退散
│   │   ├── SpiritSign.cs            # 灵符
│   │   └── ...（30+张符卡/灵力卡牌）
│   │
│   ├── Marisa/                       # 雾雨魔理沙（魔法使）
│   │   ├── MarisaAttackB.cs         # 魔理沙攻击·蓝
│   │   ├── MarisaAttackR.cs         # 魔理沙攻击·红
│   │   ├── MarisaBlockB.cs          # 魔理沙防御·蓝
│   │   ├── MarisaBlockR.cs          # 魔理沙防御·红
│   │   ├── MasterSpark.cs           # 极限火花
│   │   ├── FinalSpark.cs            # 究极火花
│   │   ├── Potion.cs                # 药水
│   │   └── ...（40+张魔法/星幻卡牌）
│   │
│   ├── Sakuya/                       # 十六夜咲夜（女仆长）
│   │   ├── SakuyaAttackU.cs         # 咲夜攻击·紫
│   │   ├── SakuyaAttackW.cs         # 咲夜攻击·白
│   │   ├── SakuyaBlockU.cs          # 咲夜防御·紫
│   │   ├── SakuyaBlockW.cs          # 咲夜防御·白
│   │   ├── Knife.cs                 # 飞刀
│   │   ├── TimeStop.cs              # 时间停止
│   │   ├── LunaDial.cs              # 月时计
│   │   └── ...（30+张时间/飞刀卡牌）
│   │
│   └── Koishi/                       # 古明地恋（觉妖）
│       ├── KoishiAttackB.cs         # 恋恋攻击·蓝
│       ├── KoishiAttackG.cs         # 恋恋攻击·绿
│       ├── KoishiBlockB.cs          # 恋恋防御·蓝
│       ├── KoishiBlockG.cs          # 恋恋防御·绿
│       ├── CloseEye.cs              # 闭眼
│       ├── InspirationCard.cs       # 灵感卡牌
│       ├── Follower.cs              # 使魔
│       └── ...（35+张意识/使魔卡牌）
│
├── 卡牌 - 中立 (Cards/Neutral/)
│   ├── NoColor/                      # 无色卡牌
│   │   ├── ManaCard.cs              # 法力卡
│   │   ├── BManaCard.cs             # 蓝法力卡
│   │   ├── RManaCard.cs             # 红法力卡
│   │   ├── GManaCard.cs             # 绿法力卡
│   │   ├── WManaCard.cs             # 白法力卡
│   │   ├── PManaCard.cs             # 紫法力卡
│   │   ├── UManaCard.cs             # 紫法力卡
│   │   ├── CManaCard.cs             # 青法力卡
│   │   └── Shoot.cs                 # 射击
│   │
│   ├── Red/                          # 红色卡牌（攻击/力量）
│   │   ├── RedGiantStar.cs          # 红巨星
│   │   ├── HuoliQuankai.cs          # 活力全开
│   │   ├── MogongAttack.cs          # 魔炮攻击
│   │   └── ...（15+张火力卡牌）
│   │
│   ├── Blue/                         # 蓝色卡牌（防御/冰）
│   │   ├── IceBlock.cs              # 冰块
│   │   ├── FakeMoon.cs              # 幻月
│   │   ├── Underwater.cs            # 水中
│   │   └── ...（15+张冰雪卡牌）
│   │
│   ├── Green/                        # 绿色卡牌（自然/生命）
│   │   ├── GreenLotus.cs            # 绿莲
│   │   ├── Sunflower.cs             # 向日葵
│   │   ├── LilyChun.cs              # 莉莉春
│   │   └── ...（10+张自然卡牌）
│   │
│   ├── White/                        # 白色卡牌（神圣/治疗）
│   │   ├── Guangyu.cs               # 光玉
│   │   ├── Invincible.cs            # 无敌
│   │   └── ...（8+张神圣卡牌）
│   │
│   ├── Black/                        # 黑色卡牌（暗影/诅咒）
│   │   ├── Shadow.cs                # 暗影
│   │   ├── Curse.cs                 # 诅咒
│   │   └── ...（12+张暗影卡牌）
│   │
│   ├── TwoColor/                     # 双色卡牌（20+张）
│   │   ├── FengleiCard.cs           # 风雷卡
│   │   ├── ShuihuoCard.cs           # 水火卡
│   │   ├── AyaWindGirl.cs           # 文文·风之少女
│   │   └── ...（角色混合卡牌）
│   │
│   └── MultiColor/                   # 多色卡牌（5+张）
│       ├── AnimalSpirit.cs          # 动物灵
│       └── ...（彩虹/多属性卡牌）
│
├── 卡牌 - 特殊类型
│   ├── Tool/                         # 道具卡牌（13张）
│   │   ├── ToolAttack.cs            # 攻击道具
│   │   ├── ToolBlock.cs             # 防御道具
│   │   ├── ToolHeal.cs              # 治疗道具
│   │   ├── ToolInvincible.cs        # 无敌道具
│   │   └── ...
│   │
│   ├── Enemy/                        # 敌人卡牌
│   │   ├── AyaNews.cs               # 文文新闻
│   │   ├── Lunatic.cs               # 狂气
│   │   └── ...
│   │
│   ├── Misfortune/                   # 灾厄卡牌
│   │   ├── Drunk.cs                 # 醉酒
│   │   ├── Pressure.cs              # 压力
│   │   └── ...
│   │
│   ├── Adventure/                    # 冒险事件卡牌
│   │   ├── GainTreasure.cs          # 获得宝藏
│   │   ├── NewsEntertainment.cs     # 娱乐新闻
│   │   └── ...
│   │
│   ├── DebugCards/                   # 调试用卡牌（12张）
│   │   ├── DebugAddHandCards.cs     # 调试添加手牌
│   │   ├── DebugUpgradeAllZone.cs   # 调试升级全区域
│   │   └── ...
│   │
│   └── Others/                       # 其他特殊卡牌
│       ├── FakeCard.cs              # 虚假卡牌
│       ├── HistoryCard.cs           # 历史卡牌
│       └── LimitedStopTimeCard.cs   # 限时停止卡牌
│
├── 敌人单位 - 角色 (EnemyUnits/Character/)
│   ├── Aya.cs                        # 射命丸文
│   ├── Clownpiece.cs                 # 克劳恩皮丝
│   ├── Doremy.cs                     # 哆来咪·苏伊特
│   ├── Junko.cs                      # 纯狐
│   ├── Remilia.cs                    # 蕾米莉亚·斯卡雷特
│   ├── Kokoro.cs                     # 秦心
│   ├── Luna.cs                       # 露娜
│   ├── Star.cs                       # 斯塔
│   ├── Sunny.cs                      # 桑尼
│   ├── Sanae.cs                      # 东风谷早苗
│   ├── Seija.cs                      # 鬼人正邪
│   ├── Youmu.cs                      # 魂魄妖梦
│   ├── Yuyuko.cs                     # 西行寺幽幽子
│   ├── Yiji.cs                       # 伊季
│   ├── Nitori.cs                     # 河城荷取
│   ├── Rin.cs                        # 火焰猫燐
│   ├── Siji.cs                       # 四季映姬
│   ├── Tianzi.cs                     # 比那名居天子
│   ├── Suika.cs                      # 伊吹萃香
│   └── DreamServants/                # 梦之从者
│       ├── DreamAya.cs              # 梦之文
│       ├── DreamJunko.cs            # 梦之纯狐
│       └── ...
│
├── 敌人单位 - 自机 (EnemyUnits/Lore/)
│   ├── Patchouli.cs                  # 帕秋莉·诺蕾姬
│   ├── Reisen.cs                     # 铃仙·优昙华院·因幡
│   ├── Suwako.cs                     # 洩矢诹访子
│   ├── Kanako.cs                     # 八坂神奈子
│   ├── Kaguya.cs                     # 蓬莱山辉夜
│   ├── Mokou.cs                      # 藤原妹红
│   ├── Keine.cs                      # 上白泽慧音
│   ├── Mystia.cs                     # 米斯蒂娅·萝蕾拉
│   ├── Tewi.cs                       # 因幡帝
│   ├── Satori.cs                     # 古明地觉
│   ├── Parsee.cs                     # 桥姬
│   ├── Hina.cs                       # 键山雏
│   ├── Kogasa.cs                     # 多多良小伞
│   ├── Nazrin.cs                     # 纳兹琳
│   ├── Koishi.cs                     # 古明地恋
│   ├── Medicine.cs                   # 梅蒂欣·梅兰可莉
│   ├── Sumireko.cs                   # 宇佐见堇子
│   ├── Yukari.cs                     # 八云紫
│   ├── Suika.cs                      # 伊吹萃香
│   ├── Yuugi.cs                      # 星熊勇仪
│   ├── Kasen.cs                      # 茨木华扇
│   ├── Miko.cs                       # 丰聪耳神子
│   ├── Futo.cs                       # 物部布都
│   ├── Tojiko.cs                     # 苏我屠自古
│   ├── Mamizou.cs                    # 二岩猯藏
│   ├── Kokoro.cs                     # 秦心
│   ├── Shinmyoumaru.cs               # 少名针妙丸
│   ├── Seija.cs                      # 鬼人正邪
│   ├── Kagerou.cs                    # 今泉影狼
│   ├── Wakasagihime.cs               # 若鹭姬
│   ├── Sekibanki.cs                  # 赤蛮奇
│   ├── Kyouko.cs                     # 幽谷响子
│   ├── Yoshika.cs                    # 宫古芳香
│   ├── Seiga.cs                      # 霍青娥
│   ├── Flandre.cs                    # 芙兰朵露·斯卡雷特
│   ├── Koishi.cs                     # 古明地恋
│   ├── Orin.cs                       # 阿燐
│   ├── Okuu.cs                       # 阿空
│   ├── Satori.cs                     # 古明地觉
│   ├── Rin.cs                        # 铃仙二号
│   ├── Utsuho.cs                     # 灵乌路空
│   ├── Hatate.cs                     # 姬海棠果
│   ├── Aya.cs                        # 射命丸文
│   ├── Momiji.cs                     # 犬走椛
│   ├── Nitori.cs                     # 河城荷取
│   ├── Hina.cs                       # 键山雏
│   ├── Yamame.cs                     # 黑谷山女
│   ├── Parsee.cs                     # 桥姬
│   ├── Yuugi.cs                      # 星熊勇仪
│   ├── Satori.cs                     # 古明地觉
│   ├── Rin.cs                        # 火焰猫燐
│   ├── Zombiel.cs                    # 僵尸
│   ├── Yuyuko.cs                     # 西行寺幽幽子
│   ├── Youmu.cs                      # 魂魄妖梦
│   ├── Ran.cpp                       # 八云蓝
│   ├── Chen.cpp                      # 橙
│   └── ...（50+角色）
│
├── 敌人单位 - 普通 (EnemyUnits/Normal/)
│   ├── Bats/                         # 蝙蝠系
│   │   ├── Bat.cs                   # 蝙蝠
│   │   └── BatLord.cs               # 蝙蝠领主
│   │
│   ├── Fairies/                      # 妖精系
│   │   ├── Fairy.cs                 # 妖精
│   │   ├── FireFairy.cs             # 火妖精
│   │   ├── IceFairy.cs              # 冰妖精
│   │   └── ...
│   │
│   ├── Ghosts/                       # 幽灵系
│   │   ├── Ghost.cs                 # 幽灵
│   │   └── Poltergeist.cs           # 骚灵
│   │
│   └── Dolls/                        # 人偶系
│       └── ShanghaiDoll.cs          # 上海人偶
│
├── 战斗配置 (BattleSets/)
│   ├── Ravens2.cs                    #  Ravens2战斗配置
│   └── ...（预设战斗配置）
│
├── 人偶 (Dolls/)
│   ├── Shanghai.cs                   # 上海人偶
│   ├── Penglai.cs                    # 蓬莱人偶
│   ├── ChargeDoll.cs                 # 充能人偶
│   ├── DefenseDoll.cs                # 防御人偶
│   └── ManaDoll.cs                   # 法力人偶
│
└── Properties/
    └── AssemblyInfo.cs               # 程序集信息
```

---

## 🎮 LBoL.Presentation - 表现层

UI界面、视觉效果、音频管理、输入处理等

```
lbol/LBoL.Presentation/
├── 游戏入口与管理
│   ├── GameEntry.cs                  # 游戏入口
│   ├── GameMaster.cs                 # 游戏主控
│   ├── PlatformHandlerRunner.cs      # 平台处理器运行器
│   └── FrameSetting.cs               # 帧设置
│
├── 环境 (Environments/)
│   ├── Environment.cs               # 环境基类
│   ├── FinalStageEnvironment.cs     # 最终关卡环境
│   └── IntoFinalEffect.cs           # 进入最终关卡效果
│
├── 子弹与投射物 (Bullet/)
│   ├── Bullet.cs                    # 子弹
│   ├── Projectile.cs                # 投射物
│   ├── Gun.cs                       # 枪械
│   ├── GunManager.cs                # 枪械管理器
│   ├── Launcher.cs                  # 发射器
│   ├── Laser.cs                     # 激光
│   ├── Piece.cs                     # 碎片
│   ├── HitType.cs                   # 命中类型
│   ├── BulletEvent.cs               # 子弹事件
│   └── ParticalRotator.cs           # 粒子旋转器
│
├── 特效 (Effect/)
│   ├── EffectManager.cs             # 特效管理器
│   ├── EffectWidget.cs              # 特效控件
│   ├── EffectBullet.cs              # 特效子弹
│   ├── EffectBulletView.cs          # 特效子弹视图
│   ├── EffectUIBulletView.cs        # UI特效子弹视图
│   ├── ManaFlyEffect.cs             # 法力飞行特效
│   ├── ExhibitActivating.cs         # 宝物激活特效
│   ├── ExileCoverEffect.cs          # 放逐覆盖特效
│   ├── RemoveCoverEffect.cs         # 移除覆盖特效
│   ├── Point.cs                     # 点
│   ├── ReimiChain.cs                # 灵梦链
│   ├── RinOrb.cs                    # 燐球
│   ├── SanaeSuck.cs                 # 早苗吸取
│   ├── VampireBlood.cs              # 吸血鬼之血
│   └── ...（各种特效实现）
│
├── UI系统 (UI/)
│   ├── UiManager.cs                 # UI管理器
│   ├── UiPanel.cs                   # UI面板
│   ├── UiPanel.2.cs                 # UI面板扩展
│   ├── UiPanelBase.cs               # UI面板基类
│   ├── UiBase.cs                    # UI基类
│   ├── UiDialog.cs                  # UI对话框
│   ├── UiDialogBase.cs              # UI对话框基类
│   ├── UiAdventurePanel.cs          # UI冒险面板
│   ├── IAdventureHandler.cs         # 冒险处理器接口
│   ├── IInputActionHandler.cs       # 输入动作处理器接口
│   ├── NavigateDirection.cs         # 导航方向
│   ├── PanelLayer.cs                # 面板层级
│   ├── ActionMapping.cs             # 动作映射
│   ├── DialogResult.cs              # 对话框结果
│   ├── CharNumTransf.cs             # 字符数字转换
│   ├── RawImageUvTweaker.cs         # RawImage UV调整器
│   ├── RectPositionTier.cs          # 矩形位置层级
│   ├── ScenePositionTier.cs         # 场景位置层级
│   ├── TooltipAlignment.cs          # 提示对齐
│   ├── TooltipDirection.cs          # 提示方向
│   ├── TooltipPosition.cs           # 提示位置
│   ├── TooltipPositioner.cs         # 提示定位器
│   └── ...
│
├── UI面板 (UI/Panels/)
│   ├── MainMenuPanel.cs             # 主菜单面板
│   ├── StartGamePanel.cs            # 开始游戏面板
│   ├── GameRunVisualPanel.cs        # 游戏运行视觉面板
│   ├── SettingPanel.cs              # 设置面板
│   ├── ProfilePanel.cs              # 配置文件面板
│   ├── MapPanel.cs                  # 地图面板
│   ├── BattleNotifier.cs            # 战斗通知器
│   ├── PlayBoard.cs                 # 游戏板
│   ├── CardDetailPanel.cs           # 卡牌详情面板
│   ├── CardDetailPayload.cs         # 卡牌详情数据
│   ├── ExhibitInfoPanel.cs          # 宝物信息面板
│   ├── UltimateSkillPanel.cs        # 终极技能面板
│   ├── GapOptionsPanel.cs           # 间隙选项面板
│   ├── ShopPanel.cs                 # 商店面板
│   ├── RewardPanel.cs               # 奖励面板
│   ├── HistoryPanel.cs              # 历史面板
│   ├── MuseumPanel.cs               # 博物馆面板
│   ├── MusicRoomPanel.cs            # 音乐室面板
│   ├── CreditsPanel.cs              # 制作组名单面板
│   ├── LicensesPanel.cs             # 许可证面板
│   ├── ComplexRulesPanel.cs         # 复杂规则面板
│   ├── ChangeLogPanel.cs            # 更新日志面板
│   ├── SpellPanel.cs                # 符卡面板
│   ├── TopMessagePanel.cs           # 顶部消息面板
│   ├── BossExhibitPanel.cs          # Boss宝物面板
│   ├── BattleManaPanel.cs           # 战斗法力面板
│   ├── BattleHintPanel.cs           # 战斗提示面板
│   ├── DebugBattleLogPanel.cs       # 调试战斗日志面板
│   ├── InteractionType.cs           # 交互类型
│   ├── ShowCardsPanel.cs            # 展示卡牌面板
│   ├── ShowCardsPayload.cs          # 展示卡牌数据
│   ├── ShowCardZone.cs              # 展示卡牌区域
│   ├── SelectCardPanel.cs           # 选择卡牌面板
│   ├── SelectCardPayload.cs         # 选择卡牌数据
│   ├── SelectBaseManaPanel.cs       # 选择基础法力面板
│   ├── SelectBaseManaPayload.cs     # 选择基础法力数据
│   ├── VnPanel.cs                   # 视觉小说面板
│   ├── VnExtraSettings.cs           # VN额外设置
│   ├── SystemBoard.cs               # 系统板
│   ├── EntryPanel.cs                # 入口面板
│   ├── HintPanel.cs                 # 提示面板
│   ├── HintPayload.cs               # 提示数据
│   ├── HintKeys.cs                  # 提示键
│   ├── PopupHud.cs                  # 弹出HUD
│   ├── GameResultPanel.cs           # 游戏结果面板
│   ├── GameResultData.cs            # 游戏结果数据
│   ├── NazrinDetectPanel.cs         # 纳兹琳探测面板
│   ├── SelectDebugPanel.cs          # 选择调试面板
│   ├── MultipleCardTooltip.cs       # 多卡牌提示
│   ├── StartSetupWidget.cs          # 开始设置控件
│   ├── StartStatusWidget.cs         # 开始状态控件
│   ├── BeginningStatusWidget.cs     # 开始状态控件
│   ├── SettingsPanelType.cs         # 设置面板类型
│   ├── ScoreData.cs                 # 分数数据
│   ├── ScoreDataId.cs               # 分数数据ID
│   ├── RewardType.cs                # 奖励类型
│   ├── RewardWidget.cs              # 奖励控件
│   ├── ShowRewardContent.cs         # 显示奖励内容
│   ├── RecordRow.cs                 # 记录行
│   └── ...
│
├── UI对话框 (UI/Dialogs/)
│   ├── MessageDialog.cs             # 消息对话框
│   ├── MessageContent.cs            # 消息内容
│   ├── MessageIcon.cs               # 消息图标
│   ├── UpgradeCardDialog.cs         # 升级卡牌对话框
│   ├── UpgradeCardContent.cs        # 升级卡牌内容
│   ├── TransformCardDialog.cs       # 变换卡牌对话框
│   ├── TransformCardContent.cs      # 变换卡牌内容
│   ├── RemoveCardDialog.cs          # 移除卡牌对话框
│   ├── RemoveCardContent.cs         # 移除卡牌内容
│   └── DialogButtons.cs             # 对话框按钮
│
├── UI控件 (UI/Widgets/)
│   ├── CardWidget.cs                # 卡牌控件
│   ├── HandCard.cs                  # 手牌控件
│   ├── CardInFastView.cs            # 快速视图中的卡牌
│   ├── CardPackWidget.cs            # 卡牌包控件
│   ├── FastDeckViewer.cs            # 快速牌库查看器
│   ├── FastDeckViewButton.cs        # 快速牌库查看按钮
│   ├── CardZoneUpperCountWidget.cs  # 卡牌区域上部计数控件
│   ├── DeckHolder.cs                # 牌库持有者
│   ├── CardFlyBrief.cs              # 卡牌飞掠简述
│   ├── ExhibitWidget.cs             # 宝物控件
│   ├── DollInfoWidget.cs            # 人偶信息控件
│   ├── IntentionWidget.cs           # 意图控件
│   ├── StatusEffectWidget.cs        # 状态效果控件
│   ├── UnitInfoWidget.cs            # 单位信息控件
│   ├── UnitStatusWidget.cs          # 单位状态控件
│   ├── UnitStatusHud.cs             # 单位状态HUD
│   ├── HealthBar.cs                 # 生命条
│   ├── DamagePopup.cs               # 伤害弹出
│   ├── BaseManaWidget.cs            # 基础法力控件
│   ├── BattleManaWidget.cs          # 战斗法力控件
│   ├── BattleManaStatus.cs          # 战斗法力状态
│   ├── EndTurnButtonWidget.cs       # 结束回合按钮控件
│   ├── StartTurnButtonWidget.cs     # 开始回合按钮控件
│   ├── GapOptionWidget.cs           # 间隙选项控件
│   ├── JadeBoxWidget.cs             # 玉盒控件
│   ├── JadeBoxToggle.cs             # 玉盒切换
│   ├── RewardWidget.cs              # 奖励控件
│   ├── ShopCard.cs                  # 商店卡牌
│   ├── ShopExhibit.cs               # 商店宝物
│   ├── ShowCardsWidget.cs           # 展示卡牌控件
│   ├── SelectCardWidget.cs          # 选择卡牌控件
│   ├── SelectBaseManaWidget.cs      # 选择基础法力控件
│   ├── TargetSelector.cs            # 目标选择器
│   ├── TargetSelectorStatus.cs      # 目标选择器状态
│   ├── TooltipWidget.cs             # 提示控件
│   ├── TooltipSource.cs             # 提示源
│   ├── EntityTooltipWidget.cs       # 实体提示控件
│   ├── StatusTooltipWidget.cs       # 状态提示控件
│   ├── UltimateSkillTooltipWidget.cs # 终极技能提示控件
│   ├── AchievementWidget.cs         # 成就控件
│   ├── AchievementHintWidget.cs     # 成就提示控件
│   ├── DollTooltipSource.cs         # 人偶提示源
│   ├── ExhibitTooltipSource.cs      # 宝物提示源
│   ├── IntentionTooltipSource.cs    # 意图提示源
│   ├── StatusTooltipSource.cs       # 状态提示源
│   ├── SimpleTooltipSource.cs       # 简单提示源
│   ├── MuseumExhibitTooltip.cs      # 博物馆宝物提示
│   ├── MuseumExhibitWidget.cs       # 博物馆宝物控件
│   ├── MusicWidget.cs               # 音乐控件
│   ├── BgmHint.cs                   # BGM提示
│   ├── ProfileWidget.cs             # 配置文件控件
│   ├── ScoreWidget.cs               # 分数控件
│   ├── RecordCardCell.cs            # 记录卡牌单元
│   ├── RecyclableScrollRectWidget.cs # 可循环滚动矩形控件
│   ├── ScrollBarWidget.cs           # 滚动条控件
│   ├── SideTipWidget.cs             # 侧边提示控件
│   ├── SpellDeclareWidget.cs        # 符卡声明控件
│   ├── CharacterButtonWidget.cs     # 角色按钮控件
│   ├── CharacterToggleWidget.cs     # 角色切换控件
│   ├── CharacterLifetimeWidget.cs   # 角色生命期控件
│   ├── CommonButtonWidget.cs        # 通用按钮控件
│   ├── CommonToggleWidget.cs        # 通用切换控件
│   ├── SwitchWidget.cs              # 开关控件
│   ├── DropdownWidget.cs            # 下拉框控件
│   ├── DropdownOptionWidget.cs      # 下拉选项控件
│   ├── MinimizedButtonWidget.cs     # 最小化按钮控件
│   ├── OptionWidget.cs              # 选项控件
│   ├── ChatWidget.cs                # 聊天控件
│   ├── LifetimeWidget.cs            # 生命期控件
│   ├── LogoOrbWidget.cs             # Logo球控件
│   ├── GameLogoWidget.cs            # 游戏Logo控件
│   ├── MapNodeWidget.cs             # 地图节点控件
│   ├── MapLineWidget.cs             # 地图线控件
│   ├── MapPageChangeWidget.cs       # 地图页面切换控件
│   ├── DifficultyGroup.cs           # 难度组
│   ├── LocaleSettingItem.cs         # 区域设置项
│   ├── SeedInputValidator.cs        # 种子输入验证器
│   ├── PuzzleToggleWidget.cs        # 谜题切换控件
│   └── ...
│
├── 单位显示 (Units/)
│   ├── UnitView.cs                  # 单位视图
│   ├── DollView.cs                  # 人偶视图
│   ├── DollSlotView.cs              # 人偶槽位视图
│   ├── EnemyFormation.cs            # 敌人群组
│   ├── GunHitArgs.cs                # 枪械命中参数
│   ├── GameDirector.cs              # 游戏导演
│   ├── DialogMessageKeys.cs         # 对话框消息键
│   └── SpecialUnits/                # 特殊单位
│       └── KokoroUnitController.cs  # 秦心单位控制器
│
├── UI过渡 (UI/Transitions/)
│   ├── UiTransition.cs              # UI过渡基类
│   ├── SimpleTransition.cs          # 简单过渡
│   ├── AnimationTransition.cs       # 动画过渡
│   ├── MapTransition.cs             # 地图过渡
│   ├── GameResultTransition.cs      # 游戏结果过渡
│   ├── BossExhibitTransition.cs     # Boss宝物过渡
│   ├── StartGameTransition.cs       # 开始游戏过渡
│   ├── MusicRoomTransition.cs       # 音乐室过渡
│   ├── ProfileTransition.cs         # 配置文件过渡
│   └── SelectBaseManaTransition.cs  # 选择基础法力过渡
│
├── UI额外控件 (UI/ExtraWidgets/)
│   ├── AchievementWidget.cs         # 成就控件
│   ├── AchievementHintWidget.cs     # 成就提示控件
│   ├── DollTooltipSource.cs         # 人偶提示源
│   ├── ExhibitTooltipSource.cs      # 宝物提示源
│   ├── IntentionTooltipSource.cs    # 意图提示源
│   ├── StatusTooltipSource.cs       # 状态提示源
│   ├── UltimateSkillTooltipSource.cs # 终极技能提示源
│   ├── ICardTooltipSource.cs        # 卡牌提示源接口
│   ├── IMultiCardTooltipSource.cs   # 多卡牌提示源接口
│   ├── SelectBaseManaWidget.cs      # 选择基础法力控件
│   ├── SelectCardWidget.cs          # 选择卡牌控件
│   ├── HandCard.cs                  # 手牌
│   ├── ShowingCard.cs               # 展示卡牌
│   ├── ShowingCardRelative.cs       # 相对展示卡牌
│   ├── ShopCard.cs                  # 商店卡牌
│   ├── ShopExhibit.cs               # 商店宝物
│   ├── CardsRow.cs                  # 卡牌行
│   ├── PuzzleToggleWidget.cs        # 谜题切换控件
│   ├── ClearState.cs                # 清除状态
│   ├── DamagePopup.cs               # 伤害弹出
│   ├── SimpleTooltipSource.cs       # 简单提示源
│   └── ...
│
├── 输入系统 (InputSystemExtend/)
│   ├── InputDeviceManager.cs        # 输入设备管理器
│   ├── InputDeviceType.cs           # 输入设备类型
│   ├── GamepadNavigationManager.cs  # 手柄导航管理器
│   ├── GamepadBehaviour.cs          # 手柄行为
│   ├── GamepadButton.cs             # 手柄按钮
│   ├── GamepadButtonPressType.cs    # 手柄按钮按压类型
│   ├── GamepadButtonKey.cs          # 手柄按钮键
│   ├── GamepadButtonTip.cs          # 手柄按钮提示
│   ├── GamepadKeySprite.cs          # 手柄键精灵
│   ├── GamepadCommonButtonTip.cs    # 手柄通用按钮提示
│   ├── GamepadCardCursor.cs         # 手柄卡牌光标
│   ├── GamepadButtonCursor.cs       # 手柄按钮光标
│   ├── GamepadScrollRectItem.cs     # 手柄滚动矩形项
│   ├── GamepadUGUISliderAdapter.cs  # 手柄UGUI滑块适配器
│   ├── GamepadPairButton.cs         # 手柄配对按钮
│   ├── GamepadNavigationOrigin.cs   # 手柄导航原点
│   └── IInteractablePanel.cs        # 可交互面板接口
│
├── 国际化 (I10N/)
│   ├── L10nManager.cs               # 本地化管理器
│   ├── L10nInfo.cs                  # 本地化信息
│   ├── LocalizedText.cs             # 本地化文本
│   └── LocalizedGameObject.cs       # 本地化游戏对象
│
├── 本地化 (UI/LocalizationManager.cs) # UI本地化管理器
│
├── 音频管理 (AudioManager.cs)         # 音频管理器
│
├── 摄像机控制
│   ├── CameraController.cs          # 摄像机控制器
│   └── OutlineCameraController.cs   # 轮廓摄像机控制器
│
├── 高亮管理 (HighlightManager.cs)    # 高亮管理器
│
├── 分辨率助手 (ResolutionHelper.cs)  # 分辨率助手
│
├── 资源助手 (ResourcesHelper.cs)     # 资源助手
│
├── 可触摸 (Touchable.cs)             # 可触摸接口
│
├── 动画 (Animations/)
│   └── SingleAnimationClipPlayer.cs # 单动画剪辑播放器
│
├── 紫室 (YukariRoom.cs)              # 紫室（特殊场景）
│
└── Properties/
    └── AssemblyInfo.cs              # 程序集信息
```

---

## 📊 非核心模块统计

| 模块 | 文件数量 | 主要内容 | 说明 |
|-----|---------|---------|------|
| **LBoL.Base** | 30+ | 基础类型、枚举、扩展方法 | 底层基础库 |
| **LBoL.ConfigData** | 25+ | 游戏配置、设置 | 数据配置 |
| **LBoL.EntityLib** | **600+** | 卡牌、敌人、冒险事件 | **主要内容库** |
| **LBoL.Presentation** | **400+** | UI、特效、音频、输入 | **表现层** |
| **总计** | **1055+** | 所有非核心代码 | 占总代码量约 85% |

---

## 🎯 LBoL.EntityLib 详细统计

| 类别 | 数量 | 示例 |
|-----|------|------|
| **角色卡牌** | 300+ | 每个角色30-50张专属卡 |
| **中立卡牌** | 200+ | 无色、五色、双色卡 |
| **敌人单位** | 80+ | 自机、普通敌人、Boss |
| **冒险事件** | 40+ | 各关卡、角色特殊事件 |
| **道具/特殊** | 30+ | 工具卡、调试卡等 |

### 主要角色卡牌分布：
- **琪露诺 (Cirno)**：45+ 张（冰系/妖精主题）
- **雾雨魔理沙 (Marisa)**：42+ 张（魔法/星幻主题）
- **古明地恋 (Koishi)**：38+ 张（意识/使魔主题）
- **博丽灵梦 (Reimu)**：35+ 张（符卡/灵力主题）
- **十六夜咲夜 (Sakuya)**：32+ 张（时间/飞刀主题）
- **爱丽丝 (Alice)**：28+ 张（人偶主题）

---

## 🎨 LBoL.Presentation 详细统计

| 系统 | 数量 | 说明 |
|------|------|------|
| **UI面板** | 80+ | 所有游戏界面 |
| **UI控件** | 120+ | 按钮、卡牌显示、状态条等 |
| **特效** | 30+ | 魔法特效、子弹、粒子效果 |
| **输入处理** | 15+ | 键盘、手柄输入 |
| **过渡动画** | 10+ | 界面切换效果 |

---

## 🔧 开发提示

### 新增MOD内容建议关注：

1. **LBoL.EntityLib**（主要内容）：
   - `Cards/Character/YourCharacter/` - 添加角色卡牌
   - `EnemyUnits/` - 添加新敌人
   - `Adventures/` - 添加冒险事件
   - `Dolls/` - 添加人偶

2. **LBoL.ConfigData**（配置）：
   - 修改游戏平衡参数
   - 调整掉落率、难度曲线

3. **LBoL.Presentation**（表现）：
   - 自定义UI皮肤
   - 添加特效
   - 修改输入绑定

4. **LBoL.Base**（基础类型）：
   - 需要深度定制时扩展基础类型

### 文件总数对比：
- **核心代码 (LBoL.Core)**：~200 文件
- **非核心代码 (其他模块)**：~1055+ 文件
- **总计**：~1255+ 文件

**非核心代码占总代码量的84%**，包含了游戏绝大多数具体内容。
