using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core.Adventures;
using LBoL.Core.Attributes;
using LBoL.Core.Cards;
using LBoL.Core.GapOptions;
using LBoL.Core.Randoms;
using LBoL.Core.SaveData;
using LBoL.Core.Stations;
using LBoL.Core.Units;
using UnityEngine;

namespace LBoL.Core
{
	// Token: 0x0200006E RID: 110
	[Localizable]
	public abstract class Stage : GameEntity
	{
		// Token: 0x1700015C RID: 348
		// (get) Token: 0x06000489 RID: 1161 RVA: 0x0000FB15 File Offset: 0x0000DD15
		internal override GameEventPriority DefaultEventPriority
		{
			get
			{
				return (GameEventPriority)0;
			}
		}

		// Token: 0x1700015D RID: 349
		// (get) Token: 0x0600048A RID: 1162 RVA: 0x0000FB18 File Offset: 0x0000DD18
		// (set) Token: 0x0600048B RID: 1163 RVA: 0x0000FB20 File Offset: 0x0000DD20
		public StageConfig Config { get; private set; }

		// Token: 0x0600048C RID: 1164 RVA: 0x0000FB29 File Offset: 0x0000DD29
		protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
		{
			return TypeFactory<Stage>.LocalizeProperty(base.Id, key, decorated, required);
		}

		// Token: 0x0600048D RID: 1165 RVA: 0x0000FB39 File Offset: 0x0000DD39
		protected virtual IReadOnlyList<string> LocalizeListProperty(string key, bool required = true)
		{
			return TypeFactory<Stage>.LocalizeListProperty(base.Id, key, required);
		}

		// Token: 0x1700015E RID: 350
		// (get) Token: 0x0600048E RID: 1166 RVA: 0x0000FB48 File Offset: 0x0000DD48
		// (set) Token: 0x0600048F RID: 1167 RVA: 0x0000FB50 File Offset: 0x0000DD50
		public int Index { get; internal set; }

		// Token: 0x1700015F RID: 351
		// (get) Token: 0x06000490 RID: 1168 RVA: 0x0000FB59 File Offset: 0x0000DD59
		// (set) Token: 0x06000491 RID: 1169 RVA: 0x0000FB61 File Offset: 0x0000DD61
		public ulong MapSeed { get; internal set; }

		// Token: 0x17000160 RID: 352
		// (get) Token: 0x06000492 RID: 1170 RVA: 0x0000FB6A File Offset: 0x0000DD6A
		// (set) Token: 0x06000493 RID: 1171 RVA: 0x0000FB72 File Offset: 0x0000DD72
		public int Level { get; protected internal set; }

		// Token: 0x17000161 RID: 353
		// (get) Token: 0x06000494 RID: 1172 RVA: 0x0000FB7B File Offset: 0x0000DD7B
		public override string Name
		{
			get
			{
				return this.LocalizeProperty("Name", false, true);
			}
		}

		// Token: 0x17000162 RID: 354
		// (get) Token: 0x06000495 RID: 1173 RVA: 0x0000FB8A File Offset: 0x0000DD8A
		// (set) Token: 0x06000496 RID: 1174 RVA: 0x0000FB92 File Offset: 0x0000DD92
		public bool IsSelectingBoss { get; protected set; }

		// Token: 0x17000163 RID: 355
		// (get) Token: 0x06000497 RID: 1175 RVA: 0x0000FB9B File Offset: 0x0000DD9B
		// (set) Token: 0x06000498 RID: 1176 RVA: 0x0000FBA3 File Offset: 0x0000DDA3
		public string SelectedBoss { get; private set; }

		// Token: 0x17000164 RID: 356
		// (get) Token: 0x06000499 RID: 1177 RVA: 0x0000FBAC File Offset: 0x0000DDAC
		// (set) Token: 0x0600049A RID: 1178 RVA: 0x0000FBB4 File Offset: 0x0000DDB4
		public Type SentinelExhibitType
		{
			[return: MaybeNull]
			get;
			protected set; }

		// Token: 0x17000165 RID: 357
		// (get) Token: 0x0600049B RID: 1179 RVA: 0x0000FBBD File Offset: 0x0000DDBD
		// (set) Token: 0x0600049C RID: 1180 RVA: 0x0000FBC5 File Offset: 0x0000DDC5
		public Type DebutAdventureType { get; internal set; }

		// Token: 0x17000166 RID: 358
		// (get) Token: 0x0600049D RID: 1181 RVA: 0x0000FBCE File Offset: 0x0000DDCE
		// (set) Token: 0x0600049E RID: 1182 RVA: 0x0000FBD6 File Offset: 0x0000DDD6
		public Type SupplyAdventureType { get; protected set; }

		// Token: 0x17000167 RID: 359
		// (get) Token: 0x0600049F RID: 1183 RVA: 0x0000FBDF File Offset: 0x0000DDDF
		// (set) Token: 0x060004A0 RID: 1184 RVA: 0x0000FBE7 File Offset: 0x0000DDE7
		public Type TradeAdventureType { get; protected set; }

		// Token: 0x17000168 RID: 360
		// (get) Token: 0x060004A1 RID: 1185 RVA: 0x0000FBF0 File Offset: 0x0000DDF0
		// (set) Token: 0x060004A2 RID: 1186 RVA: 0x0000FBF8 File Offset: 0x0000DDF8
		public bool IsNormalFinalStage { get; internal set; }

		// Token: 0x17000169 RID: 361
		// (get) Token: 0x060004A3 RID: 1187 RVA: 0x0000FC01 File Offset: 0x0000DE01
		// (set) Token: 0x060004A4 RID: 1188 RVA: 0x0000FC09 File Offset: 0x0000DE09
		public bool IsTrueEndFinalStage { get; internal set; }

		// Token: 0x1700016A RID: 362
		// (get) Token: 0x060004A5 RID: 1189 RVA: 0x0000FC12 File Offset: 0x0000DE12
		// (set) Token: 0x060004A6 RID: 1190 RVA: 0x0000FC1A File Offset: 0x0000DE1A
		public bool EternalStageMusic { get; protected set; }

		// Token: 0x1700016B RID: 363
		// (get) Token: 0x060004A7 RID: 1191 RVA: 0x0000FC23 File Offset: 0x0000DE23
		// (set) Token: 0x060004A8 RID: 1192 RVA: 0x0000FC2B File Offset: 0x0000DE2B
		public bool DontAutoOpenMapInEntry { get; protected set; }

		// Token: 0x1700016C RID: 364
		// (get) Token: 0x060004A9 RID: 1193 RVA: 0x0000FC34 File Offset: 0x0000DE34
		// (set) Token: 0x060004AA RID: 1194 RVA: 0x0000FC3C File Offset: 0x0000DE3C
		public bool EnterWithSpecialPresentation { get; protected set; }

		// Token: 0x1700016D RID: 365
		// (get) Token: 0x060004AB RID: 1195 RVA: 0x0000FC45 File Offset: 0x0000DE45
		// (set) Token: 0x060004AC RID: 1196 RVA: 0x0000FC4D File Offset: 0x0000DE4D
		public EnemyGroupEntry Boss { get; protected set; }

		// Token: 0x1700016E RID: 366
		// (get) Token: 0x060004AD RID: 1197 RVA: 0x0000FC56 File Offset: 0x0000DE56
		// (set) Token: 0x060004AE RID: 1198 RVA: 0x0000FC5E File Offset: 0x0000DE5E
		public RepeatableRandomPool<string> BossPool { get; set; } = new RepeatableRandomPool<string>();

		// Token: 0x1700016F RID: 367
		// (get) Token: 0x060004AF RID: 1199 RVA: 0x0000FC67 File Offset: 0x0000DE67
		// (set) Token: 0x060004B0 RID: 1200 RVA: 0x0000FC6F File Offset: 0x0000DE6F
		public Type FirstAdventure { get; protected set; }

		// Token: 0x17000170 RID: 368
		// (get) Token: 0x060004B1 RID: 1201 RVA: 0x0000FC78 File Offset: 0x0000DE78
		// (set) Token: 0x060004B2 RID: 1202 RVA: 0x0000FC80 File Offset: 0x0000DE80
		public UniqueRandomPool<Type> FirstAdventurePool { get; set; } = new UniqueRandomPool<Type>(false);

		// Token: 0x17000171 RID: 369
		// (get) Token: 0x060004B3 RID: 1203 RVA: 0x0000FC89 File Offset: 0x0000DE89
		// (set) Token: 0x060004B4 RID: 1204 RVA: 0x0000FC91 File Offset: 0x0000DE91
		public UniqueRandomPool<Type> AdventurePool { get; set; } = new UniqueRandomPool<Type>(false);

		// Token: 0x17000172 RID: 370
		// (get) Token: 0x060004B5 RID: 1205 RVA: 0x0000FC9A File Offset: 0x0000DE9A
		// (set) Token: 0x060004B6 RID: 1206 RVA: 0x0000FCA2 File Offset: 0x0000DEA2
		public UniqueRandomPool<string> EnemyPoolAct1 { get; set; } = new UniqueRandomPool<string>(true);

		// Token: 0x17000173 RID: 371
		// (get) Token: 0x060004B7 RID: 1207 RVA: 0x0000FCAB File Offset: 0x0000DEAB
		// (set) Token: 0x060004B8 RID: 1208 RVA: 0x0000FCB3 File Offset: 0x0000DEB3
		public UniqueRandomPool<string> EnemyPoolAct2 { get; set; } = new UniqueRandomPool<string>(true);

		// Token: 0x17000174 RID: 372
		// (get) Token: 0x060004B9 RID: 1209 RVA: 0x0000FCBC File Offset: 0x0000DEBC
		// (set) Token: 0x060004BA RID: 1210 RVA: 0x0000FCC4 File Offset: 0x0000DEC4
		public UniqueRandomPool<string> EnemyPoolAct3 { get; set; } = new UniqueRandomPool<string>(true);

		// Token: 0x17000175 RID: 373
		// (get) Token: 0x060004BB RID: 1211 RVA: 0x0000FCCD File Offset: 0x0000DECD
		// (set) Token: 0x060004BC RID: 1212 RVA: 0x0000FCD5 File Offset: 0x0000DED5
		public UniqueRandomPool<string> EliteEnemyPool { get; set; } = new UniqueRandomPool<string>(true);

		// Token: 0x17000176 RID: 374
		// (get) Token: 0x060004BD RID: 1213 RVA: 0x0000FCDE File Offset: 0x0000DEDE
		public List<Type> AdventureHistory { get; } = new List<Type>();

		// Token: 0x17000177 RID: 375
		// (get) Token: 0x060004BE RID: 1214 RVA: 0x0000FCE6 File Offset: 0x0000DEE6
		// (set) Token: 0x060004BF RID: 1215 RVA: 0x0000FCEE File Offset: 0x0000DEEE
		public HashSet<string> ExtraFlags { get; internal set; } = new HashSet<string>();

		// Token: 0x17000178 RID: 376
		// (get) Token: 0x060004C0 RID: 1216 RVA: 0x0000FCF7 File Offset: 0x0000DEF7
		// (set) Token: 0x060004C1 RID: 1217 RVA: 0x0000FCFF File Offset: 0x0000DEFF
		protected ExhibitWeightTable ShopExhibitWeightTable { get; set; } = new ExhibitWeightTable(new RarityWeightTable(0.5f, 0.33f, 0.17f, 0f), new AppearanceWeightTable(1f, 2f, 0f, 0f));

		// Token: 0x17000179 RID: 377
		// (get) Token: 0x060004C2 RID: 1218 RVA: 0x0000FD08 File Offset: 0x0000DF08
		// (set) Token: 0x060004C3 RID: 1219 RVA: 0x0000FD10 File Offset: 0x0000DF10
		protected ExhibitWeightTable ShopExhibitWeightTableShopOnly { get; set; } = new ExhibitWeightTable(new RarityWeightTable(0.5f, 0.33f, 0.17f, 0f), AppearanceWeightTable.OnlyShopOnly);

		// Token: 0x1700017A RID: 378
		// (get) Token: 0x060004C4 RID: 1220 RVA: 0x0000FD19 File Offset: 0x0000DF19
		// (set) Token: 0x060004C5 RID: 1221 RVA: 0x0000FD21 File Offset: 0x0000DF21
		protected ExhibitWeightTable EliteEnemyExhibitWeightTable { get; set; } = new ExhibitWeightTable(new RarityWeightTable(0.5f, 0.33f, 0.17f, 0f), new AppearanceWeightTable(1f, 0f, 1f, 0f));

		// Token: 0x1700017B RID: 379
		// (get) Token: 0x060004C6 RID: 1222 RVA: 0x0000FD2A File Offset: 0x0000DF2A
		// (set) Token: 0x060004C7 RID: 1223 RVA: 0x0000FD32 File Offset: 0x0000DF32
		protected ExhibitWeightTable SupplyExhibitWeightTable { get; set; } = new ExhibitWeightTable(new RarityWeightTable(0.3f, 0.5f, 0.2f, 0f), new AppearanceWeightTable(1f, 0f, 1f, 0f));

		// Token: 0x1700017C RID: 380
		// (get) Token: 0x060004C8 RID: 1224 RVA: 0x0000FD3B File Offset: 0x0000DF3B
		// (set) Token: 0x060004C9 RID: 1225 RVA: 0x0000FD43 File Offset: 0x0000DF43
		protected CardWeightTable DrinkTeaAdditionalCardWeight { get; set; } = new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.Hierarchy, CardTypeWeightTable.CanBeLoot, false);

		// Token: 0x1700017D RID: 381
		// (get) Token: 0x060004CA RID: 1226 RVA: 0x0000FD4C File Offset: 0x0000DF4C
		// (set) Token: 0x060004CB RID: 1227 RVA: 0x0000FD54 File Offset: 0x0000DF54
		public float CardUpgradedChance { get; protected set; }

		// Token: 0x1700017E RID: 382
		// (get) Token: 0x060004CC RID: 1228 RVA: 0x0000FD5D File Offset: 0x0000DF5D
		// (set) Token: 0x060004CD RID: 1229 RVA: 0x0000FD65 File Offset: 0x0000DF65
		protected CardWeightTable EnemyCardWeight { get; set; } = CardWeightTable.WithoutTool;

		// Token: 0x1700017F RID: 383
		// (get) Token: 0x060004CE RID: 1230 RVA: 0x0000FD6E File Offset: 0x0000DF6E
		// (set) Token: 0x060004CF RID: 1231 RVA: 0x0000FD76 File Offset: 0x0000DF76
		protected CardWeightTable EnemyCardOnlyPlayerWeight { get; set; } = CardWeightTable.WithoutTool;

		// Token: 0x17000180 RID: 384
		// (get) Token: 0x060004D0 RID: 1232 RVA: 0x0000FD7F File Offset: 0x0000DF7F
		// (set) Token: 0x060004D1 RID: 1233 RVA: 0x0000FD87 File Offset: 0x0000DF87
		protected CardWeightTable EnemyCardWithFriendWeight { get; set; } = CardWeightTable.WithoutTool;

		// Token: 0x17000181 RID: 385
		// (get) Token: 0x060004D2 RID: 1234 RVA: 0x0000FD90 File Offset: 0x0000DF90
		// (set) Token: 0x060004D3 RID: 1235 RVA: 0x0000FD98 File Offset: 0x0000DF98
		protected CardWeightTable EnemyCardNeutralWeight { get; set; } = CardWeightTable.WithoutTool;

		// Token: 0x17000182 RID: 386
		// (get) Token: 0x060004D4 RID: 1236 RVA: 0x0000FDA1 File Offset: 0x0000DFA1
		// (set) Token: 0x060004D5 RID: 1237 RVA: 0x0000FDA9 File Offset: 0x0000DFA9
		protected CardWeightTable EliteEnemyCardWeight { get; set; } = CardWeightTable.WithoutTool;

		// Token: 0x17000183 RID: 387
		// (get) Token: 0x060004D6 RID: 1238 RVA: 0x0000FDB2 File Offset: 0x0000DFB2
		// (set) Token: 0x060004D7 RID: 1239 RVA: 0x0000FDBA File Offset: 0x0000DFBA
		protected CardWeightTable EliteEnemyCardCharaWeight { get; set; } = CardWeightTable.WithoutTool;

		// Token: 0x17000184 RID: 388
		// (get) Token: 0x060004D8 RID: 1240 RVA: 0x0000FDC3 File Offset: 0x0000DFC3
		// (set) Token: 0x060004D9 RID: 1241 RVA: 0x0000FDCB File Offset: 0x0000DFCB
		protected CardWeightTable EliteEnemyCardFriendWeight { get; set; } = CardWeightTable.WithoutTool;

		// Token: 0x17000185 RID: 389
		// (get) Token: 0x060004DA RID: 1242 RVA: 0x0000FDD4 File Offset: 0x0000DFD4
		// (set) Token: 0x060004DB RID: 1243 RVA: 0x0000FDDC File Offset: 0x0000DFDC
		protected CardWeightTable EliteEnemyCardNeutralWeight { get; set; } = CardWeightTable.WithoutTool;

		// Token: 0x17000186 RID: 390
		// (get) Token: 0x060004DC RID: 1244 RVA: 0x0000FDE5 File Offset: 0x0000DFE5
		// (set) Token: 0x060004DD RID: 1245 RVA: 0x0000FDED File Offset: 0x0000DFED
		protected CardWeightTable BossCardWeight { get; set; } = CardWeightTable.WithoutTool;

		// Token: 0x17000187 RID: 391
		// (get) Token: 0x060004DE RID: 1246 RVA: 0x0000FDF6 File Offset: 0x0000DFF6
		// (set) Token: 0x060004DF RID: 1247 RVA: 0x0000FDFE File Offset: 0x0000DFFE
		protected CardWeightTable BossCardCharaWeight { get; set; } = CardWeightTable.WithoutTool;

		// Token: 0x17000188 RID: 392
		// (get) Token: 0x060004E0 RID: 1248 RVA: 0x0000FE07 File Offset: 0x0000E007
		// (set) Token: 0x060004E1 RID: 1249 RVA: 0x0000FE0F File Offset: 0x0000E00F
		protected CardWeightTable BossCardFriendWeight { get; set; } = CardWeightTable.WithoutTool;

		// Token: 0x17000189 RID: 393
		// (get) Token: 0x060004E2 RID: 1250 RVA: 0x0000FE18 File Offset: 0x0000E018
		// (set) Token: 0x060004E3 RID: 1251 RVA: 0x0000FE20 File Offset: 0x0000E020
		protected CardWeightTable BossCardNeutralWeight { get; set; } = CardWeightTable.WithoutTool;

		// Token: 0x1700018A RID: 394
		// (get) Token: 0x060004E4 RID: 1252 RVA: 0x0000FE29 File Offset: 0x0000E029
		// (set) Token: 0x060004E5 RID: 1253 RVA: 0x0000FE31 File Offset: 0x0000E031
		protected CardWeightTable ShopNormalAtkWeight { get; set; } = CardWeightTable.ShopAtk;

		// Token: 0x1700018B RID: 395
		// (get) Token: 0x060004E6 RID: 1254 RVA: 0x0000FE3A File Offset: 0x0000E03A
		// (set) Token: 0x060004E7 RID: 1255 RVA: 0x0000FE42 File Offset: 0x0000E042
		protected CardWeightTable ShopNormalDefWeight { get; set; } = CardWeightTable.ShopDef;

		// Token: 0x1700018C RID: 396
		// (get) Token: 0x060004E8 RID: 1256 RVA: 0x0000FE4B File Offset: 0x0000E04B
		// (set) Token: 0x060004E9 RID: 1257 RVA: 0x0000FE53 File Offset: 0x0000E053
		protected CardWeightTable ShopNormalSklWeight { get; set; } = CardWeightTable.ShopSkl;

		// Token: 0x1700018D RID: 397
		// (get) Token: 0x060004EA RID: 1258 RVA: 0x0000FE5C File Offset: 0x0000E05C
		// (set) Token: 0x060004EB RID: 1259 RVA: 0x0000FE64 File Offset: 0x0000E064
		protected CardWeightTable ShopNormalAblWeight { get; set; } = CardWeightTable.ShopAbl;

		// Token: 0x1700018E RID: 398
		// (get) Token: 0x060004EC RID: 1260 RVA: 0x0000FE6D File Offset: 0x0000E06D
		// (set) Token: 0x060004ED RID: 1261 RVA: 0x0000FE75 File Offset: 0x0000E075
		protected CardWeightTable ShopSkillAndFriendWeight { get; set; } = CardWeightTable.ShopSkillAndFriend;

		// Token: 0x1700018F RID: 399
		// (get) Token: 0x060004EE RID: 1262 RVA: 0x0000FE7E File Offset: 0x0000E07E
		// (set) Token: 0x060004EF RID: 1263 RVA: 0x0000FE86 File Offset: 0x0000E086
		protected CardWeightTable ShopToolCardWeight { get; set; } = CardWeightTable.OnlyTool;

		// Token: 0x060004F0 RID: 1264 RVA: 0x0000FE8F File Offset: 0x0000E08F
		public override void Initialize()
		{
			base.Initialize();
			this.Config = StageConfig.FromId(base.Id);
			if (this.Config == null)
			{
				throw new InvalidDataException("Cannot find stage config for <" + base.Id + ">");
			}
		}

		// Token: 0x060004F1 RID: 1265 RVA: 0x0000FECB File Offset: 0x0000E0CB
		public Stage AsNormalFinal()
		{
			this.IsNormalFinalStage = true;
			return this;
		}

		// Token: 0x060004F2 RID: 1266 RVA: 0x0000FED5 File Offset: 0x0000E0D5
		public Stage AsTrueEndFinal()
		{
			this.IsTrueEndFinalStage = true;
			return this;
		}

		// Token: 0x060004F3 RID: 1267 RVA: 0x0000FEDF File Offset: 0x0000E0DF
		internal void Enter()
		{
			this.OnEnter();
		}

		// Token: 0x060004F4 RID: 1268 RVA: 0x0000FEE7 File Offset: 0x0000E0E7
		protected void OnEnter()
		{
		}

		// Token: 0x060004F5 RID: 1269 RVA: 0x0000FEE9 File Offset: 0x0000E0E9
		public virtual void InitExtraFlags(ProfileSaveData userProfile)
		{
		}

		// Token: 0x060004F6 RID: 1270 RVA: 0x0000FEEC File Offset: 0x0000E0EC
		public virtual void InitBoss(RandomGen rng)
		{
			string text = this.BossPool.SampleOrDefault(rng);
			if (text != null)
			{
				this.Boss = Library.GetEnemyGroupEntry(text);
				return;
			}
			if (!this.IsSelectingBoss)
			{
				throw new InvalidOperationException("Cannot generate non-boss-selecting stage '" + base.GetType().Name + "' map with empty boss pool");
			}
		}

		// Token: 0x060004F7 RID: 1271 RVA: 0x0000FF3E File Offset: 0x0000E13E
		public void InitFirstAdventure(RandomGen initBossRng)
		{
			this.FirstAdventure = (this.FirstAdventurePool.IsEmpty ? null : this.FirstAdventurePool.Sample(initBossRng));
		}

		// Token: 0x060004F8 RID: 1272 RVA: 0x0000FF62 File Offset: 0x0000E162
		public virtual GameMap CreateMap()
		{
			RandomGen randomGen = new RandomGen(this.MapSeed);
			EnemyGroupEntry boss = this.Boss;
			return GameMap.CreateNormalMap(randomGen, (boss != null) ? boss.Id : null, this.IsSelectingBoss);
		}

		// Token: 0x060004F9 RID: 1273 RVA: 0x0000FF8C File Offset: 0x0000E18C
		public virtual void SetBoss(string enemyGroupName)
		{
			if (this.Boss != null)
			{
				throw new InvalidOperationException(string.Concat(new string[]
				{
					this.DebugName,
					" already has boss '",
					this.Boss.Id,
					"', tried setting to '",
					enemyGroupName,
					"'"
				}));
			}
			this.Boss = Library.GetEnemyGroupEntry(enemyGroupName);
			this.SelectedBoss = enemyGroupName;
		}

		// Token: 0x060004FA RID: 1274 RVA: 0x0000FFF8 File Offset: 0x0000E1F8
		public Station CreateStation(MapNode node)
		{
			return this.CreateStationFromType(node, node.StationType);
		}

		// Token: 0x060004FB RID: 1275 RVA: 0x00010008 File Offset: 0x0000E208
		internal Station CreateStationFromType(MapNode node, StationType type)
		{
			Station station = Station.Create(type);
			BossStation bossStation = station as BossStation;
			if (bossStation != null)
			{
				if (this.Boss == null)
				{
					Debug.LogError("Stage has no boss.");
				}
				else
				{
					bossStation.BossId = this.Boss.Id;
				}
			}
			station.GameRun = base.GameRun;
			station.Stage = this;
			station.Level = node.X;
			station.Act = node.Act;
			if (node.FollowerList.Empty<int>())
			{
				station.IsStageEnd = true;
				if (this.IsTrueEndFinalStage)
				{
					station.IsTrueEnd = true;
				}
				else if (this.IsNormalFinalStage)
				{
					station.IsNormalEnd = true;
				}
			}
			return station;
		}

		// Token: 0x060004FC RID: 1276 RVA: 0x000100AC File Offset: 0x0000E2AC
		public virtual Type GetAdventure()
		{
			UniqueRandomPool<Type> uniqueRandomPool = new UniqueRandomPool<Type>(false);
			foreach (RandomPoolEntry<Type> randomPoolEntry in Enumerable.ToList<RandomPoolEntry<Type>>(this.AdventurePool))
			{
				Type type;
				float num;
				randomPoolEntry.Deconstruct(out type, out num);
				Type type2 = type;
				float num2 = num;
				if (base.GameRun.AdventureHistory.Contains(type2))
				{
					Debug.Log("[Stage: " + this.DebugName + "] Removing duplicated adventure " + type2.Name);
					this.AdventurePool.Remove(type2, true);
				}
				else
				{
					uniqueRandomPool.Add(type2, Library.WeightForAdventure(type2, base.GameRun) * num2);
				}
			}
			if (uniqueRandomPool.IsEmpty)
			{
				return typeof(FakeAdventure);
			}
			Type type3 = uniqueRandomPool.Sample(base.GameRun.StationRng);
			this.AdventurePool.Remove(type3, true);
			return type3;
		}

		// Token: 0x060004FD RID: 1277 RVA: 0x000101A0 File Offset: 0x0000E3A0
		public virtual Card[] GetShopNormalCards()
		{
			List<Card> list = new List<Card>();
			list.AddRange(base.GameRun.GetShopCards(2, this.ShopNormalAtkWeight, null));
			list.AddRange(base.GameRun.GetShopCards(2, this.ShopNormalDefWeight, null));
			Card card = Enumerable.First<Card>(base.GameRun.GetShopCards(1, this.ShopNormalSklWeight, null));
			list.Add(card);
			GameRunController gameRun = base.GameRun;
			int num = 1;
			CardWeightTable shopSkillAndFriendWeight = this.ShopSkillAndFriendWeight;
			List<string> list2 = new List<string>();
			list2.Add(card.Id);
			list.AddRange(gameRun.GetShopCards(num, shopSkillAndFriendWeight, list2));
			list.AddRange(base.GameRun.GetShopCards(2, this.ShopNormalAblWeight, null));
			return list.ToArray();
		}

		// Token: 0x060004FE RID: 1278 RVA: 0x0001024B File Offset: 0x0000E44B
		public virtual Card[] GetShopToolCards(int count)
		{
			return base.GameRun.GetShopCards(count, this.ShopToolCardWeight, null);
		}

		// Token: 0x060004FF RID: 1279 RVA: 0x00010260 File Offset: 0x0000E460
		public virtual Card SupplyShopCard(Card justBought, List<string> nowCardIds)
		{
			switch (justBought.CardType)
			{
			case CardType.Attack:
				return base.GameRun.GetShopCards(1, this.ShopNormalAtkWeight, nowCardIds)[0];
			case CardType.Defense:
				return base.GameRun.GetShopCards(1, this.ShopNormalDefWeight, nowCardIds)[0];
			case CardType.Skill:
				return base.GameRun.GetShopCards(1, this.ShopSkillAndFriendWeight, nowCardIds)[0];
			case CardType.Ability:
				return base.GameRun.GetShopCards(1, this.ShopNormalAblWeight, nowCardIds)[0];
			case CardType.Friend:
				return base.GameRun.GetShopCards(1, this.ShopSkillAndFriendWeight, nowCardIds)[0];
			case CardType.Tool:
				return base.GameRun.GetShopCards(1, this.ShopToolCardWeight, nowCardIds)[0];
			default:
				throw new InvalidOperationException(string.Format("Bought a card:{0} with wrong card type:{1}.", justBought.DebugName, justBought.CardType));
			}
		}

		// Token: 0x06000500 RID: 1280 RVA: 0x0001033D File Offset: 0x0000E53D
		public virtual Exhibit GetShopExhibit(bool shopOnly)
		{
			return base.GameRun.RollNormalExhibit(base.GameRun.ShopRng, shopOnly ? this.ShopExhibitWeightTableShopOnly : this.ShopExhibitWeightTable, new Func<Exhibit>(this.GetSentinelExhibit), null);
		}

		// Token: 0x06000501 RID: 1281 RVA: 0x00010373 File Offset: 0x0000E573
		public virtual Exhibit GetEliteEnemyExhibit()
		{
			return base.GameRun.RollNormalExhibit(base.GameRun.ExhibitRng, this.EliteEnemyExhibitWeightTable, new Func<Exhibit>(this.GetSentinelExhibit), null);
		}

		// Token: 0x06000502 RID: 1282 RVA: 0x0001039E File Offset: 0x0000E59E
		public virtual Exhibit GetSupplyExhibit()
		{
			return base.GameRun.RollNormalExhibit(base.GameRun.ExhibitRng, this.SupplyExhibitWeightTable, new Func<Exhibit>(this.GetSentinelExhibit), null);
		}

		// Token: 0x06000503 RID: 1283 RVA: 0x000103CC File Offset: 0x0000E5CC
		public Exhibit GetSpecialAdventureExhibit()
		{
			return base.GameRun.RollNormalExhibit(base.GameRun.ExhibitRng, new ExhibitWeightTable(new RarityWeightTable(0.5f, 0.33f, 0.17f, 0f), new AppearanceWeightTable(1f, 1f, 1f, 0f)), new Func<Exhibit>(this.GetSentinelExhibit), null);
		}

		// Token: 0x06000504 RID: 1284 RVA: 0x00010434 File Offset: 0x0000E634
		public virtual Exhibit GetNeutralShiningExhibit()
		{
			return base.GameRun.RollShiningExhibit(base.GameRun.ShinningExhibitRng, new Func<Exhibit>(this.GetSentinelExhibit), (ExhibitConfig config) => string.IsNullOrWhiteSpace(config.Owner));
		}

		// Token: 0x06000505 RID: 1285 RVA: 0x00010482 File Offset: 0x0000E682
		public virtual Exhibit RollExhibitInAdventure(ExhibitWeightTable weightTable, [MaybeNull] Predicate<ExhibitConfig> filter = null)
		{
			return base.GameRun.RollNormalExhibit(base.GameRun.ExhibitRng, weightTable, new Func<Exhibit>(this.GetSentinelExhibit), filter);
		}

		// Token: 0x06000506 RID: 1286 RVA: 0x000104A8 File Offset: 0x0000E6A8
		[return: MaybeNull]
		internal Exhibit GetSentinelExhibit()
		{
			if (this.SentinelExhibitType == null)
			{
				throw new InvalidOperationException("[Stage: " + this.DebugName + "] Has no sentinal exhibit");
			}
			Exhibit exhibit = Library.CreateExhibit(this.SentinelExhibitType);
			if (!exhibit.Config.IsSentinel)
			{
				Debug.LogError(string.Concat(new string[] { "[Stage: ", this.DebugName, "] Sentinal exhibit ", exhibit.DebugName, " is not sentinal in config" }));
			}
			return exhibit;
		}

		// Token: 0x06000507 RID: 1287 RVA: 0x00010534 File Offset: 0x0000E734
		public virtual Exhibit[] GetBossExhibits()
		{
			if (this.Boss == null)
			{
				Debug.LogError("Stage has no boss, thus cannot get boss exhibits");
				return Array.Empty<Exhibit>();
			}
			return base.GameRun.RollBossExhibits(base.GameRun.ShinningExhibitRng, this.Boss.Id, this.Boss.RollBossExhibit, new Func<Exhibit>(this.<GetBossExhibits>g__FallbackShining|229_0));
		}

		// Token: 0x06000508 RID: 1288 RVA: 0x00010591 File Offset: 0x0000E791
		public virtual StationReward GetDrinkTeaCardReward(DrinkTea drinkTea)
		{
			return StationReward.CreateCards(base.GameRun.GetRewardCards(this.EnemyCardOnlyPlayerWeight, this.EnemyCardWithFriendWeight, this.EnemyCardNeutralWeight, this.DrinkTeaAdditionalCardWeight, drinkTea.CardCount, false));
		}

		// Token: 0x06000509 RID: 1289 RVA: 0x000105C2 File Offset: 0x0000E7C2
		public virtual StationReward GetEnemyCardReward()
		{
			return StationReward.CreateCards(base.GameRun.GetRewardCards(this.EnemyCardOnlyPlayerWeight, this.EnemyCardWithFriendWeight, this.EnemyCardNeutralWeight, this.EnemyCardWeight, base.GameRun.RewardCardCount, false));
		}

		// Token: 0x0600050A RID: 1290 RVA: 0x000105F8 File Offset: 0x0000E7F8
		public virtual StationReward GetEliteEnemyCardReward()
		{
			return StationReward.CreateCards(base.GameRun.GetRewardCards(this.EliteEnemyCardCharaWeight, this.EliteEnemyCardFriendWeight, this.EliteEnemyCardNeutralWeight, this.EliteEnemyCardWeight, base.GameRun.RewardCardCount, false));
		}

		// Token: 0x0600050B RID: 1291 RVA: 0x0001062E File Offset: 0x0000E82E
		public virtual StationReward GetBossCardReward()
		{
			return StationReward.CreateCards(base.GameRun.GetRewardCards(this.BossCardCharaWeight, this.BossCardFriendWeight, this.BossCardNeutralWeight, this.BossCardWeight, base.GameRun.RewardCardCount, true));
		}

		// Token: 0x0600050C RID: 1292 RVA: 0x00010664 File Offset: 0x0000E864
		public virtual EnemyGroupEntry GetEnemies(Station station)
		{
			string text;
			switch (station.Act)
			{
			case 1:
				text = this.EnemyPoolAct1.Sample(base.GameRun.StationRng);
				break;
			case 2:
				text = this.EnemyPoolAct2.Sample(base.GameRun.StationRng);
				break;
			case 3:
				text = this.EnemyPoolAct3.Sample(base.GameRun.StationRng);
				break;
			default:
				throw new ArgumentOutOfRangeException("Act");
			}
			return Library.GetEnemyGroupEntry(text);
		}

		// Token: 0x0600050D RID: 1293 RVA: 0x000106EA File Offset: 0x0000E8EA
		public virtual EnemyGroupEntry GetEliteEnemies(Station station)
		{
			return Library.GetEnemyGroupEntry(this.EliteEnemyPool.Sample(base.GameRun.StationRng));
		}

		// Token: 0x0600050E RID: 1294 RVA: 0x00010707 File Offset: 0x0000E907
		public virtual EnemyGroupEntry GetBoss()
		{
			return this.Boss;
		}

		// Token: 0x06000510 RID: 1296 RVA: 0x00010950 File Offset: 0x0000EB50
		[CompilerGenerated]
		private Exhibit <GetBossExhibits>g__FallbackShining|229_0()
		{
			return base.GameRun.RollShiningExhibit(base.GameRun.ExhibitRng, new Func<Exhibit>(this.<GetBossExhibits>g__FallbackNormal|229_1), (ExhibitConfig config) => string.IsNullOrWhiteSpace(config.Owner) && config.BaseManaColor == null);
		}

		// Token: 0x06000511 RID: 1297 RVA: 0x000109A0 File Offset: 0x0000EBA0
		[CompilerGenerated]
		private Exhibit <GetBossExhibits>g__FallbackNormal|229_1()
		{
			return base.GameRun.RollNormalExhibit(base.GameRun.ExhibitRng, ExhibitWeightTable.AllOnes, new Func<Exhibit>(this.GetSentinelExhibit), (ExhibitConfig config) => string.IsNullOrWhiteSpace(config.Owner));
		}

		// Token: 0x04000263 RID: 611
		public bool isNormalStage;
	}
}
