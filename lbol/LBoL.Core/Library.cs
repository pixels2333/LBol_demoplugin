using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core.Adventures;
using LBoL.Core.Cards;
using LBoL.Core.GapOptions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using UnityEngine;

namespace LBoL.Core
{
	// Token: 0x02000058 RID: 88
	public static class Library
	{
		// Token: 0x060003A8 RID: 936 RVA: 0x0000BD14 File Offset: 0x00009F14
		private static void RegisterTypes<T>(IDictionary<string, Assembly> asmTable) where T : class
		{
			Library.RegisterAssembly<T>(asmTable, "LBoL.Core");
			Library.RegisterAssembly<T>(asmTable, "LBoL.EntityLib");
			Library.RegisterAssembly<T>(asmTable, "LBoL." + typeof(T).Name + "s");
		}

		// Token: 0x060003A9 RID: 937 RVA: 0x0000BD50 File Offset: 0x00009F50
		private static void RegisterAssembly<T>(IDictionary<string, Assembly> asmTable, string assemblyName) where T : class
		{
			Assembly assembly;
			if (asmTable.TryGetValue(assemblyName, ref assembly))
			{
				Library.RegisterAssembly<T>(assembly);
			}
		}

		// Token: 0x060003AA RID: 938 RVA: 0x0000BD6E File Offset: 0x00009F6E
		private static void RegisterAssembly<T>(Assembly assembly) where T : class
		{
			TypeFactory<T>.RegisterAssembly(assembly);
		}

		// Token: 0x060003AB RID: 939 RVA: 0x0000BD78 File Offset: 0x00009F78
		public static async UniTask RegisterAllAsync()
		{
			if (!Library._registered)
			{
				Dictionary<string, Assembly> asmTable = Enumerable.ToDictionary<ValueTuple<string, Assembly>, string, Assembly>(Enumerable.Select<IGrouping<string, Assembly>, ValueTuple<string, Assembly>>(Enumerable.GroupBy<Assembly, string>(Enumerable.Where<Assembly>(AppDomain.CurrentDomain.GetAssemblies(), (Assembly asm) => !asm.IsDynamic), (Assembly asm) => asm.GetName().Name), (IGrouping<string, Assembly> grp) => new ValueTuple<string, Assembly>(grp.Key, Enumerable.First<Assembly>(grp))), ([TupleElementNames(new string[] { "name", "value" })] ValueTuple<string, Assembly> kv) => kv.Item1, ([TupleElementNames(new string[] { "name", "value" })] ValueTuple<string, Assembly> kv) => kv.Item2);
				await UniTask.WhenAll(new UniTask[]
				{
					UniTask.RunOnThreadPool(delegate
					{
						Library.RegisterTypes<Stage>(asmTable);
					}, true, default(CancellationToken)),
					UniTask.RunOnThreadPool(delegate
					{
						Library.RegisterTypes<Card>(asmTable);
					}, true, default(CancellationToken)),
					UniTask.RunOnThreadPool(delegate
					{
						Library.RegisterTypes<UltimateSkill>(asmTable);
					}, true, default(CancellationToken)),
					UniTask.RunOnThreadPool(delegate
					{
						Library.RegisterTypes<Exhibit>(asmTable);
					}, true, default(CancellationToken)),
					UniTask.RunOnThreadPool(delegate
					{
						Library.RegisterTypes<PlayerUnit>(asmTable);
					}, true, default(CancellationToken)),
					UniTask.RunOnThreadPool(delegate
					{
						Library.RegisterTypes<EnemyUnit>(asmTable);
					}, true, default(CancellationToken)),
					UniTask.RunOnThreadPool(delegate
					{
						Library.RegisterTypes<StatusEffect>(asmTable);
					}, true, default(CancellationToken)),
					UniTask.RunOnThreadPool(delegate
					{
						Library.RegisterTypes<Doll>(asmTable);
					}, true, default(CancellationToken)),
					UniTask.RunOnThreadPool(delegate
					{
						Library.RegisterTypes<GapOption>(asmTable);
					}, true, default(CancellationToken)),
					UniTask.RunOnThreadPool(delegate
					{
						Library.RegisterTypes<Intention>(asmTable);
					}, true, default(CancellationToken)),
					UniTask.RunOnThreadPool(delegate
					{
						Library.RegisterTypes<Adventure>(asmTable);
					}, true, default(CancellationToken)),
					UniTask.RunOnThreadPool(delegate
					{
						Library.RegisterTypes<JadeBox>(asmTable);
					}, true, default(CancellationToken))
				});
				Library._exhibitWeighterTable = new Dictionary<Type, IExhibitWeighter>();
				foreach (Type type in TypeFactory<Exhibit>.AllTypes)
				{
					ExhibitInfoAttribute customAttribute = CustomAttributeExtensions.GetCustomAttribute<ExhibitInfoAttribute>(type);
					IExhibitWeighter exhibitWeighter = ((customAttribute != null) ? customAttribute.CreateWeighter() : null);
					if (exhibitWeighter != null)
					{
						Library._exhibitWeighterTable.Add(type, exhibitWeighter);
					}
				}
				Dictionary<Type, IAdventureWeighter> dictionary = new Dictionary<Type, IAdventureWeighter>();
				Library._adventureWeighterTable = new Dictionary<Type, IAdventureWeighter>();
				foreach (Type type2 in TypeFactory<Adventure>.AllTypes)
				{
					AdventureInfoAttribute customAttribute2 = CustomAttributeExtensions.GetCustomAttribute<AdventureInfoAttribute>(type2);
					if (((customAttribute2 != null) ? customAttribute2.WeighterType : null) != null)
					{
						Type weighterType = customAttribute2.WeighterType;
						IAdventureWeighter adventureWeighter;
						if (!dictionary.TryGetValue(weighterType, ref adventureWeighter))
						{
							adventureWeighter = (IAdventureWeighter)Activator.CreateInstance(weighterType);
							dictionary.Add(weighterType, adventureWeighter);
						}
						Library._adventureWeighterTable.Add(type2, adventureWeighter);
					}
				}
				Library._registered = true;
			}
		}

		// Token: 0x060003AC RID: 940 RVA: 0x0000BDB3 File Offset: 0x00009FB3
		public static T CreateStage<T>() where T : Stage
		{
			return TypeFactory<Stage>.CreateInstance<T>();
		}

		// Token: 0x060003AD RID: 941 RVA: 0x0000BDBA File Offset: 0x00009FBA
		public static Stage CreateStage(Type stageType)
		{
			return TypeFactory<Stage>.CreateInstance(stageType);
		}

		// Token: 0x060003AE RID: 942 RVA: 0x0000BDC2 File Offset: 0x00009FC2
		public static Stage CreateStage(string name)
		{
			return TypeFactory<Stage>.CreateInstance(name);
		}

		// Token: 0x060003AF RID: 943 RVA: 0x0000BDCA File Offset: 0x00009FCA
		public static T CreateCard<T>() where T : Card
		{
			return TypeFactory<Card>.CreateInstance<T>();
		}

		// Token: 0x060003B0 RID: 944 RVA: 0x0000BDD1 File Offset: 0x00009FD1
		public static Card CreateCard(Type cardType)
		{
			return TypeFactory<Card>.CreateInstance(cardType);
		}

		// Token: 0x060003B1 RID: 945 RVA: 0x0000BDDC File Offset: 0x00009FDC
		public static Card CreateCard(string name)
		{
			if (name.Length > 0 && Enumerable.Last<char>(name) == '+')
			{
				Card card = TypeFactory<Card>.CreateInstance(name.Substring(0, name.Length - 1));
				card.Upgrade();
				return card;
			}
			return TypeFactory<Card>.CreateInstance(name);
		}

		// Token: 0x060003B2 RID: 946 RVA: 0x0000BE20 File Offset: 0x0000A020
		public static T CreateCard<T>(bool upgraded) where T : Card
		{
			T t = TypeFactory<Card>.CreateInstance<T>();
			if (upgraded)
			{
				t.Upgrade();
			}
			return t;
		}

		// Token: 0x060003B3 RID: 947 RVA: 0x0000BE44 File Offset: 0x0000A044
		public static Card CreateCard(Type cardType, bool upgraded)
		{
			Card card = TypeFactory<Card>.CreateInstance(cardType);
			if (upgraded)
			{
				card.Upgrade();
			}
			return card;
		}

		// Token: 0x060003B4 RID: 948 RVA: 0x0000BE64 File Offset: 0x0000A064
		public static Card CreateCard(string name, bool upgraded, int? upgradeCounter = null)
		{
			Card card = TypeFactory<Card>.CreateInstance(name);
			if (upgraded)
			{
				if (card.CanUpgrade)
				{
					card.Upgrade();
					if (upgradeCounter != null && upgradeCounter.GetValueOrDefault() > 0)
					{
						card.UpgradeCounter = upgradeCounter;
					}
				}
				else
				{
					Debug.LogError(string.Format("[Library] Cannot create upgraded version of non-upgradable card {0} (IsUpgraded={1})", card.DebugName, card.IsUpgraded));
				}
			}
			return card;
		}

		// Token: 0x060003B5 RID: 949 RVA: 0x0000BEC8 File Offset: 0x0000A0C8
		public static Card TryCreateCard(string name, bool upgraded, int? upgradeCounter = null)
		{
			Card card = TypeFactory<Card>.TryCreateInstance(name);
			if (card == null)
			{
				return null;
			}
			if (upgraded)
			{
				if (card.CanUpgrade)
				{
					card.Upgrade();
					if (upgradeCounter != null && upgradeCounter.GetValueOrDefault() > 0)
					{
						card.UpgradeCounter = upgradeCounter;
					}
				}
				else
				{
					Debug.LogError(string.Format("[Library] Cannot create upgraded version of non-upgradable card {0} (IsUpgraded={1})", card.DebugName, card.IsUpgraded));
				}
			}
			return card;
		}

		// Token: 0x060003B6 RID: 950 RVA: 0x0000BF30 File Offset: 0x0000A130
		public static IEnumerable<T> CreateCards<T>(int count, bool upgraded = false) where T : Card
		{
			T[] array = new T[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = Library.CreateCard<T>(upgraded);
			}
			return array;
		}

		// Token: 0x060003B7 RID: 951 RVA: 0x0000BF5E File Offset: 0x0000A15E
		public static T CreateUs<T>() where T : UltimateSkill
		{
			return TypeFactory<UltimateSkill>.CreateInstance<T>();
		}

		// Token: 0x060003B8 RID: 952 RVA: 0x0000BF65 File Offset: 0x0000A165
		public static UltimateSkill CreateUs(Type usType)
		{
			return TypeFactory<UltimateSkill>.CreateInstance(usType);
		}

		// Token: 0x060003B9 RID: 953 RVA: 0x0000BF6D File Offset: 0x0000A16D
		public static UltimateSkill CreateUs(string name)
		{
			return TypeFactory<UltimateSkill>.CreateInstance(name);
		}

		// Token: 0x060003BA RID: 954 RVA: 0x0000BF75 File Offset: 0x0000A175
		public static UltimateSkill TryCreateUs(string name)
		{
			return TypeFactory<UltimateSkill>.TryCreateInstance(name);
		}

		// Token: 0x060003BB RID: 955 RVA: 0x0000BF7D File Offset: 0x0000A17D
		public static T CreateExhibit<T>() where T : Exhibit
		{
			return TypeFactory<Exhibit>.CreateInstance<T>();
		}

		// Token: 0x060003BC RID: 956 RVA: 0x0000BF84 File Offset: 0x0000A184
		public static Exhibit CreateExhibit(Type exhibitType)
		{
			return TypeFactory<Exhibit>.CreateInstance(exhibitType);
		}

		// Token: 0x060003BD RID: 957 RVA: 0x0000BF8C File Offset: 0x0000A18C
		public static Exhibit CreateExhibit(string name)
		{
			return TypeFactory<Exhibit>.CreateInstance(name);
		}

		// Token: 0x060003BE RID: 958 RVA: 0x0000BF94 File Offset: 0x0000A194
		public static Exhibit TryCreateExhibit(string name)
		{
			return TypeFactory<Exhibit>.TryCreateInstance(name);
		}

		// Token: 0x060003BF RID: 959 RVA: 0x0000BF9C File Offset: 0x0000A19C
		public static T CreateJadeBox<T>() where T : JadeBox
		{
			return TypeFactory<JadeBox>.CreateInstance<T>();
		}

		// Token: 0x060003C0 RID: 960 RVA: 0x0000BFA3 File Offset: 0x0000A1A3
		public static JadeBox CreateJadeBox(Type jadeBoxType)
		{
			return TypeFactory<JadeBox>.CreateInstance(jadeBoxType);
		}

		// Token: 0x060003C1 RID: 961 RVA: 0x0000BFAB File Offset: 0x0000A1AB
		public static JadeBox CreateJadeBox(string name)
		{
			return TypeFactory<JadeBox>.CreateInstance(name);
		}

		// Token: 0x060003C2 RID: 962 RVA: 0x0000BFB3 File Offset: 0x0000A1B3
		public static JadeBox TryCreateJadeBox(string name)
		{
			return TypeFactory<JadeBox>.TryCreateInstance(name);
		}

		// Token: 0x060003C3 RID: 963 RVA: 0x0000BFBB File Offset: 0x0000A1BB
		public static T CreatePlayerUnit<T>() where T : PlayerUnit
		{
			return TypeFactory<PlayerUnit>.CreateInstance<T>();
		}

		// Token: 0x060003C4 RID: 964 RVA: 0x0000BFC2 File Offset: 0x0000A1C2
		public static PlayerUnit CreatePlayerUnit(Type playerUnitType)
		{
			return TypeFactory<PlayerUnit>.CreateInstance(playerUnitType);
		}

		// Token: 0x060003C5 RID: 965 RVA: 0x0000BFCA File Offset: 0x0000A1CA
		public static PlayerUnit CreatePlayerUnit(string name)
		{
			return TypeFactory<PlayerUnit>.CreateInstance(name);
		}

		// Token: 0x060003C6 RID: 966 RVA: 0x0000BFD2 File Offset: 0x0000A1D2
		public static PlayerUnit TryCreatePlayerUnit(string name)
		{
			return TypeFactory<PlayerUnit>.TryCreateInstance(name);
		}

		// Token: 0x060003C7 RID: 967 RVA: 0x0000BFDA File Offset: 0x0000A1DA
		public static T CreateEnemyUnit<T>() where T : EnemyUnit
		{
			return TypeFactory<EnemyUnit>.CreateInstance<T>();
		}

		// Token: 0x060003C8 RID: 968 RVA: 0x0000BFE1 File Offset: 0x0000A1E1
		public static EnemyUnit CreateEnemyUnit(Type enemyUnitType)
		{
			return TypeFactory<EnemyUnit>.CreateInstance(enemyUnitType);
		}

		// Token: 0x060003C9 RID: 969 RVA: 0x0000BFE9 File Offset: 0x0000A1E9
		public static EnemyUnit CreateEnemyUnit(string name)
		{
			return TypeFactory<EnemyUnit>.CreateInstance(name);
		}

		// Token: 0x060003CA RID: 970 RVA: 0x0000BFF1 File Offset: 0x0000A1F1
		public static EnemyUnit TryCreateEnemyUnit(string name)
		{
			return TypeFactory<EnemyUnit>.TryCreateInstance(name);
		}

		// Token: 0x060003CB RID: 971 RVA: 0x0000BFF9 File Offset: 0x0000A1F9
		public static T CreateStatusEffect<T>() where T : StatusEffect
		{
			return TypeFactory<StatusEffect>.CreateInstance<T>();
		}

		// Token: 0x060003CC RID: 972 RVA: 0x0000C000 File Offset: 0x0000A200
		public static StatusEffect CreateStatusEffect(Type statusEffectType)
		{
			return TypeFactory<StatusEffect>.CreateInstance(statusEffectType);
		}

		// Token: 0x060003CD RID: 973 RVA: 0x0000C008 File Offset: 0x0000A208
		public static StatusEffect CreateStatusEffect(string name)
		{
			return TypeFactory<StatusEffect>.CreateInstance(name);
		}

		// Token: 0x060003CE RID: 974 RVA: 0x0000C010 File Offset: 0x0000A210
		public static StatusEffect TryCreateStatusEffect(string name)
		{
			return TypeFactory<StatusEffect>.TryCreateInstance(name);
		}

		// Token: 0x060003CF RID: 975 RVA: 0x0000C018 File Offset: 0x0000A218
		public static T CreateDoll<T>() where T : Doll
		{
			return TypeFactory<Doll>.CreateInstance<T>();
		}

		// Token: 0x060003D0 RID: 976 RVA: 0x0000C01F File Offset: 0x0000A21F
		public static Doll CreateDoll(Type dollType)
		{
			return TypeFactory<Doll>.CreateInstance(dollType);
		}

		// Token: 0x060003D1 RID: 977 RVA: 0x0000C027 File Offset: 0x0000A227
		public static Doll CreateDoll(string id)
		{
			return TypeFactory<Doll>.CreateInstance(id);
		}

		// Token: 0x060003D2 RID: 978 RVA: 0x0000C02F File Offset: 0x0000A22F
		public static Doll TryCreate(string id)
		{
			return TypeFactory<Doll>.TryCreateInstance(id);
		}

		// Token: 0x060003D3 RID: 979 RVA: 0x0000C037 File Offset: 0x0000A237
		public static T CreateGapOption<T>() where T : GapOption
		{
			return TypeFactory<GapOption>.CreateInstance<T>();
		}

		// Token: 0x060003D4 RID: 980 RVA: 0x0000C03E File Offset: 0x0000A23E
		public static GapOption CreateGapOption(Type gapOptionType)
		{
			return TypeFactory<GapOption>.CreateInstance(gapOptionType);
		}

		// Token: 0x060003D5 RID: 981 RVA: 0x0000C046 File Offset: 0x0000A246
		public static GapOption CreateGapOption(string name)
		{
			return TypeFactory<GapOption>.CreateInstance(name);
		}

		// Token: 0x060003D6 RID: 982 RVA: 0x0000C04E File Offset: 0x0000A24E
		public static T CreateIntention<T>() where T : Intention
		{
			return TypeFactory<Intention>.CreateInstance<T>();
		}

		// Token: 0x060003D7 RID: 983 RVA: 0x0000C055 File Offset: 0x0000A255
		public static Intention CreateIntention(Type intentionType)
		{
			return TypeFactory<Intention>.CreateInstance(intentionType);
		}

		// Token: 0x060003D8 RID: 984 RVA: 0x0000C05D File Offset: 0x0000A25D
		public static Intention CreateIntention(string name)
		{
			return TypeFactory<Intention>.CreateInstance(name);
		}

		// Token: 0x060003D9 RID: 985 RVA: 0x0000C065 File Offset: 0x0000A265
		public static T CreateAdventure<T>() where T : Adventure
		{
			return TypeFactory<Adventure>.CreateInstance<T>();
		}

		// Token: 0x060003DA RID: 986 RVA: 0x0000C06C File Offset: 0x0000A26C
		public static Adventure CreateAdventure(Type adventureType)
		{
			return TypeFactory<Adventure>.CreateInstance(adventureType);
		}

		// Token: 0x060003DB RID: 987 RVA: 0x0000C074 File Offset: 0x0000A274
		public static Adventure CreateAdventure(string name)
		{
			return TypeFactory<Adventure>.CreateInstance(name);
		}

		// Token: 0x060003DC RID: 988 RVA: 0x0000C07C File Offset: 0x0000A27C
		public static Adventure TryCreateAdventure(string name)
		{
			return TypeFactory<Adventure>.TryCreateInstance(name);
		}

		// Token: 0x060003DD RID: 989 RVA: 0x0000C084 File Offset: 0x0000A284
		public static float WeightForExhibit(Type type, GameRunController gameRun)
		{
			IExhibitWeighter exhibitWeighter;
			if (Library._exhibitWeighterTable.TryGetValue(type, ref exhibitWeighter))
			{
				return exhibitWeighter.WeightFor(type, gameRun);
			}
			return 1f;
		}

		// Token: 0x060003DE RID: 990 RVA: 0x0000C0B0 File Offset: 0x0000A2B0
		public static float WeightForAdventure(Type type, GameRunController gameRun)
		{
			IAdventureWeighter adventureWeighter;
			if (Library._adventureWeighterTable.TryGetValue(type, ref adventureWeighter))
			{
				return adventureWeighter.WeightFor(type, gameRun);
			}
			return 1f;
		}

		// Token: 0x060003DF RID: 991 RVA: 0x0000C0DC File Offset: 0x0000A2DC
		public static EnemyGroupEntry CreateEnemyGroupEntryFromConfig(EnemyGroupConfig config)
		{
			EnemyGroupEntry enemyGroupEntry = new EnemyGroupEntry(config);
			foreach (ValueTuple<int, string> valueTuple in config.Enemies.WithIndices<string>())
			{
				int item = valueTuple.Item1;
				string item2 = valueTuple.Item2;
				if (item2 != "Empty")
				{
					enemyGroupEntry.Add(TypeFactory<EnemyUnit>.GetType(item2), item);
				}
			}
			return enemyGroupEntry;
		}

		// Token: 0x060003E0 RID: 992 RVA: 0x0000C158 File Offset: 0x0000A358
		public static EnemyGroupEntry TryGetEnemyGroupEntry(string id)
		{
			EnemyGroupConfig enemyGroupConfig = EnemyGroupConfig.FromId(id);
			if (enemyGroupConfig != null)
			{
				return Library.CreateEnemyGroupEntryFromConfig(enemyGroupConfig);
			}
			return null;
		}

		// Token: 0x060003E1 RID: 993 RVA: 0x0000C177 File Offset: 0x0000A377
		public static EnemyGroupEntry GetEnemyGroupEntry(string id)
		{
			EnemyGroupConfig enemyGroupConfig = EnemyGroupConfig.FromId(id);
			if (enemyGroupConfig == null)
			{
				throw new InvalidDataException("Cannot get enemy group entry '" + id + "'");
			}
			return Library.CreateEnemyGroupEntryFromConfig(enemyGroupConfig);
		}

		// Token: 0x060003E2 RID: 994 RVA: 0x0000C19D File Offset: 0x0000A39D
		public static EnemyGroup GenerateEnemyGroup(GameRunController gameRun, string name)
		{
			return Library.GetEnemyGroupEntry(name).Generate(gameRun);
		}

		// Token: 0x060003E3 RID: 995 RVA: 0x0000C1AC File Offset: 0x0000A3AC
		public static async UniTask ReloadLocalizationsAsync()
		{
			await UniTask.WhenAll(new UniTask[]
			{
				TypeFactory<Stage>.ReloadLocalizationTableAsync(),
				TypeFactory<Card>.ReloadLocalizationTableAsync(),
				TypeFactory<UltimateSkill>.ReloadLocalizationTableAsync(),
				TypeFactory<PlayerUnit>.ReloadLocalizationTableAsync(),
				TypeFactory<EnemyUnit>.ReloadLocalizationTableAsync(),
				TypeFactory<StatusEffect>.ReloadLocalizationTableAsync(),
				TypeFactory<Doll>.ReloadLocalizationTableAsync(),
				TypeFactory<Exhibit>.ReloadLocalizationTableAsync(),
				TypeFactory<GapOption>.ReloadLocalizationTableAsync(),
				TypeFactory<Intention>.ReloadLocalizationTableAsync(),
				TypeFactory<Adventure>.ReloadLocalizationTableAsync(),
				TypeFactory<JadeBox>.ReloadLocalizationTableAsync()
			});
		}

		// Token: 0x060003E4 RID: 996 RVA: 0x0000C1E8 File Offset: 0x0000A3E8
		public static PlayerUnit[] GetSelectablePlayers()
		{
			return Enumerable.ToArray<PlayerUnit>(Enumerable.Select<PlayerUnitConfig, PlayerUnit>(Enumerable.Where<PlayerUnitConfig>(PlayerUnitConfig.AllConfig(), (PlayerUnitConfig config) => config.IsSelectable), (PlayerUnitConfig config) => Library.CreatePlayerUnit(config.Id)));
		}

		// Token: 0x060003E5 RID: 997 RVA: 0x0000C247 File Offset: 0x0000A447
		internal static IEnumerable<string> EnumerateOpponentIds()
		{
			foreach (EnemyUnitConfig enemyUnitConfig in EnemyUnitConfig.AllConfig())
			{
				if (enemyUnitConfig.IsPreludeOpponent)
				{
					yield return enemyUnitConfig.Id;
				}
			}
			IEnumerator<EnemyUnitConfig> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x060003E6 RID: 998 RVA: 0x0000C250 File Offset: 0x0000A450
		[return: TupleElementNames(new string[] { "cardType", "config" })]
		public static IEnumerable<ValueTuple<Type, CardConfig>> EnumerateCardTypes()
		{
			foreach (Type type in TypeFactory<Card>.AllTypes)
			{
				CardConfig cardConfig = CardConfig.FromId(type.Name);
				if (cardConfig != null)
				{
					yield return new ValueTuple<Type, CardConfig>(type, cardConfig);
				}
				else
				{
					Debug.LogError("[Library] card config for '" + type.Name + "' not found");
				}
			}
			IEnumerator<Type> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x060003E7 RID: 999 RVA: 0x0000C259 File Offset: 0x0000A459
		[return: TupleElementNames(new string[] { "exhibitType", "config" })]
		public static IEnumerable<ValueTuple<Type, ExhibitConfig>> EnumerateExhibitTypes()
		{
			foreach (Type type in TypeFactory<Exhibit>.AllTypes)
			{
				ExhibitConfig exhibitConfig = ExhibitConfig.FromId(type.Name);
				if (exhibitConfig != null)
				{
					yield return new ValueTuple<Type, ExhibitConfig>(type, exhibitConfig);
				}
				else
				{
					Debug.LogError("[Library] exhibit config for '" + type.Name + "' not found");
				}
			}
			IEnumerator<Type> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x060003E8 RID: 1000 RVA: 0x0000C262 File Offset: 0x0000A462
		[return: TupleElementNames(new string[] { "statusEffectType", "config" })]
		public static IEnumerable<ValueTuple<Type, StatusEffectConfig>> EnumerateStatusEffectTypes()
		{
			foreach (Type type in TypeFactory<StatusEffect>.AllTypes)
			{
				StatusEffectConfig statusEffectConfig = StatusEffectConfig.FromId(type.Name);
				if (statusEffectConfig != null)
				{
					yield return new ValueTuple<Type, StatusEffectConfig>(type, statusEffectConfig);
				}
				else
				{
					Debug.LogError("[Library] status-effect config for '" + type.Name + "' not found");
				}
			}
			IEnumerator<Type> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x060003E9 RID: 1001 RVA: 0x0000C26B File Offset: 0x0000A46B
		[return: TupleElementNames(new string[] { "enemyUnitType", "config" })]
		public static IEnumerable<ValueTuple<Type, EnemyUnitConfig>> EnumerateEnemyUnitTypes()
		{
			foreach (Type type in TypeFactory<EnemyUnit>.AllTypes)
			{
				EnemyUnitConfig enemyUnitConfig = EnemyUnitConfig.FromId(type.Name);
				if (enemyUnitConfig != null)
				{
					yield return new ValueTuple<Type, EnemyUnitConfig>(type, enemyUnitConfig);
				}
				else
				{
					Debug.LogError("[Library] enemy config for '" + type.Name + "' not found");
				}
			}
			IEnumerator<Type> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x060003EA RID: 1002 RVA: 0x0000C274 File Offset: 0x0000A474
		internal static IEnumerable<ValueTuple<Type, CardConfig>> EnumerateRollableCardTypes(int unlockLevel)
		{
			foreach (CardConfig cardConfig in CardConfig.AllConfig())
			{
				CardType type = cardConfig.Type;
				if (type != CardType.Misfortune && type != CardType.Status && type != CardType.Unknown && unlockLevel >= ExpHelper.GetCardUnlockLevel(cardConfig.Id))
				{
					Type type2 = TypeFactory<Card>.TryGetType(cardConfig.Id);
					if (type2 != null)
					{
						yield return new ValueTuple<Type, CardConfig>(type2, cardConfig);
					}
				}
			}
			IEnumerator<CardConfig> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x060003EB RID: 1003 RVA: 0x0000C284 File Offset: 0x0000A484
		internal static IEnumerable<ValueTuple<Type, ExhibitConfig>> EnumerateRollableExhibitTypes(int unlockLevel)
		{
			foreach (ExhibitConfig exhibitConfig in ExhibitConfig.AllConfig())
			{
				if (!exhibitConfig.IsDebug && unlockLevel >= ExpHelper.GetExhibitUnlockLevel(exhibitConfig.Id))
				{
					Type type = TypeFactory<Exhibit>.TryGetType(exhibitConfig.Id);
					if (type != null)
					{
						yield return new ValueTuple<Type, ExhibitConfig>(type, exhibitConfig);
					}
				}
			}
			IEnumerator<ExhibitConfig> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x060003EC RID: 1004 RVA: 0x0000C294 File Offset: 0x0000A494
		public static IEnumerable<EnemyGroupEntry> EnumerateEnemyGroupEntries()
		{
			return Enumerable.Select<EnemyGroupConfig, EnemyGroupEntry>(EnemyGroupConfig.AllConfig(), new Func<EnemyGroupConfig, EnemyGroupEntry>(Library.CreateEnemyGroupEntryFromConfig));
		}

		// Token: 0x060003ED RID: 1005 RVA: 0x0000C2AC File Offset: 0x0000A4AC
		public static IEnumerable<Type> EnumerateAdventureTypes()
		{
			return TypeFactory<Adventure>.AllTypes;
		}

		// Token: 0x060003EE RID: 1006 RVA: 0x0000C2B3 File Offset: 0x0000A4B3
		public static IEnumerable<ValueTuple<Type, JadeBoxConfig>> EnumerateJadeBoxTypes()
		{
			foreach (Type type in TypeFactory<JadeBox>.AllTypes)
			{
				yield return new ValueTuple<Type, JadeBoxConfig>(type, JadeBoxConfig.FromId(type.Name));
			}
			IEnumerator<Type> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x060003EF RID: 1007 RVA: 0x0000C2BC File Offset: 0x0000A4BC
		internal static IEnumerable<IDisplayWord> InternalEnumerateDisplayWords(GameRunController gameRun, Keyword keyword, IEnumerable<string> initEffects, bool verbose, Keyword? exceptKeywords = null)
		{
			int counter = 0;
			HashSet<string> traveledTypes = new HashSet<string>();
			Keyword traveledKeywords = Keyword.None;
			List<string> initEffectList = null;
			if (initEffects != null)
			{
				initEffectList = Enumerable.ToList<string>(initEffects);
			}
			Queue<IDisplayWord> queue = new Queue<IDisplayWord>();
			foreach (Keyword keyword2 in Keywords.EnumerateComponents(keyword))
			{
				KeywordDisplayWord displayWord = Keywords.GetDisplayWord(keyword2);
				if (!displayWord.IsHidden && (!displayWord.IsVerbose || verbose))
				{
					queue.Enqueue(displayWord);
				}
			}
			if (initEffectList == null || !Enumerable.Any<string>(initEffectList))
			{
				goto IL_036F;
			}
			using (IEnumerator<string> enumerator2 = Enumerable.Distinct<string>(initEffectList).GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					string text = enumerator2.Current;
					StatusEffect statusEffect = TypeFactory<StatusEffect>.CreateInstance(text);
					if (verbose || !statusEffect.Config.IsVerbose)
					{
						statusEffect.GameRun = gameRun;
						queue.Enqueue(statusEffect);
					}
				}
				goto IL_036F;
			}
			IL_0140:
			int num = counter + 1;
			counter = num;
			if (counter > 99)
			{
				throw new OverflowException("Too many references, traveled types = [" + ", ".Join(traveledTypes) + "]");
			}
			IDisplayWord front = queue.Dequeue();
			if (exceptKeywords != null)
			{
				Keyword valueOrDefault = exceptKeywords.GetValueOrDefault();
				KeywordDisplayWord keywordDisplayWord = front as KeywordDisplayWord;
				if (keywordDisplayWord != null)
				{
					if (!valueOrDefault.HasFlag(keywordDisplayWord.Keyword))
					{
						traveledKeywords |= keywordDisplayWord.Keyword;
						yield return front;
						goto IL_0225;
					}
					goto IL_0225;
				}
			}
			yield return front;
			IL_0225:
			StatusEffect statusEffect2 = front as StatusEffect;
			if (statusEffect2 != null)
			{
				traveledTypes.Add(statusEffect2.Id);
				foreach (Keyword keyword3 in Keywords.EnumerateComponents(statusEffect2.Config.Keywords))
				{
					if (!traveledKeywords.HasFlag(keyword3))
					{
						traveledKeywords |= keyword3;
						KeywordDisplayWord displayWord2 = Keywords.GetDisplayWord(keyword3);
						if (!displayWord2.IsHidden && (!displayWord2.IsVerbose || verbose))
						{
							queue.Enqueue(displayWord2);
						}
					}
				}
				foreach (string text2 in statusEffect2.Config.RelativeEffects)
				{
					if (!traveledTypes.Contains(text2) && (initEffectList == null || !initEffectList.Contains(text2)))
					{
						StatusEffect statusEffect3 = TypeFactory<StatusEffect>.CreateInstance(text2);
						if (verbose || !statusEffect3.Config.IsVerbose)
						{
							statusEffect3.GameRun = gameRun;
							queue.Enqueue(statusEffect3);
						}
					}
				}
			}
			front = null;
			IL_036F:
			if (queue.Count <= 0)
			{
				yield break;
			}
			goto IL_0140;
		}

		// Token: 0x04000217 RID: 535
		private static bool _registered;

		// Token: 0x04000218 RID: 536
		private static Dictionary<Type, IExhibitWeighter> _exhibitWeighterTable;

		// Token: 0x04000219 RID: 537
		private static Dictionary<Type, IAdventureWeighter> _adventureWeighterTable;
	}
}
