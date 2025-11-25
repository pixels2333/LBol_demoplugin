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
	public static class Library
	{
		private static void RegisterTypes<T>(IDictionary<string, Assembly> asmTable) where T : class
		{
			Library.RegisterAssembly<T>(asmTable, "LBoL.Core");
			Library.RegisterAssembly<T>(asmTable, "LBoL.EntityLib");
			Library.RegisterAssembly<T>(asmTable, "LBoL." + typeof(T).Name + "s");
		}
		private static void RegisterAssembly<T>(IDictionary<string, Assembly> asmTable, string assemblyName) where T : class
		{
			Assembly assembly;
			if (asmTable.TryGetValue(assemblyName, ref assembly))
			{
				Library.RegisterAssembly<T>(assembly);
			}
		}
		private static void RegisterAssembly<T>(Assembly assembly) where T : class
		{
			TypeFactory<T>.RegisterAssembly(assembly);
		}
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
		public static T CreateStage<T>() where T : Stage
		{
			return TypeFactory<Stage>.CreateInstance<T>();
		}
		public static Stage CreateStage(Type stageType)
		{
			return TypeFactory<Stage>.CreateInstance(stageType);
		}
		public static Stage CreateStage(string name)
		{
			return TypeFactory<Stage>.CreateInstance(name);
		}
		public static T CreateCard<T>() where T : Card
		{
			return TypeFactory<Card>.CreateInstance<T>();
		}
		public static Card CreateCard(Type cardType)
		{
			return TypeFactory<Card>.CreateInstance(cardType);
		}
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
		public static T CreateCard<T>(bool upgraded) where T : Card
		{
			T t = TypeFactory<Card>.CreateInstance<T>();
			if (upgraded)
			{
				t.Upgrade();
			}
			return t;
		}
		public static Card CreateCard(Type cardType, bool upgraded)
		{
			Card card = TypeFactory<Card>.CreateInstance(cardType);
			if (upgraded)
			{
				card.Upgrade();
			}
			return card;
		}
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
		public static IEnumerable<T> CreateCards<T>(int count, bool upgraded = false) where T : Card
		{
			T[] array = new T[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = Library.CreateCard<T>(upgraded);
			}
			return array;
		}
		public static T CreateUs<T>() where T : UltimateSkill
		{
			return TypeFactory<UltimateSkill>.CreateInstance<T>();
		}
		public static UltimateSkill CreateUs(Type usType)
		{
			return TypeFactory<UltimateSkill>.CreateInstance(usType);
		}
		public static UltimateSkill CreateUs(string name)
		{
			return TypeFactory<UltimateSkill>.CreateInstance(name);
		}
		public static UltimateSkill TryCreateUs(string name)
		{
			return TypeFactory<UltimateSkill>.TryCreateInstance(name);
		}
		public static T CreateExhibit<T>() where T : Exhibit
		{
			return TypeFactory<Exhibit>.CreateInstance<T>();
		}
		public static Exhibit CreateExhibit(Type exhibitType)
		{
			return TypeFactory<Exhibit>.CreateInstance(exhibitType);
		}
		public static Exhibit CreateExhibit(string name)
		{
			return TypeFactory<Exhibit>.CreateInstance(name);
		}
		public static Exhibit TryCreateExhibit(string name)
		{
			return TypeFactory<Exhibit>.TryCreateInstance(name);
		}
		public static T CreateJadeBox<T>() where T : JadeBox
		{
			return TypeFactory<JadeBox>.CreateInstance<T>();
		}
		public static JadeBox CreateJadeBox(Type jadeBoxType)
		{
			return TypeFactory<JadeBox>.CreateInstance(jadeBoxType);
		}
		public static JadeBox CreateJadeBox(string name)
		{
			return TypeFactory<JadeBox>.CreateInstance(name);
		}
		public static JadeBox TryCreateJadeBox(string name)
		{
			return TypeFactory<JadeBox>.TryCreateInstance(name);
		}
		public static T CreatePlayerUnit<T>() where T : PlayerUnit
		{
			return TypeFactory<PlayerUnit>.CreateInstance<T>();
		}
		public static PlayerUnit CreatePlayerUnit(Type playerUnitType)
		{
			return TypeFactory<PlayerUnit>.CreateInstance(playerUnitType);
		}
		public static PlayerUnit CreatePlayerUnit(string name)
		{
			return TypeFactory<PlayerUnit>.CreateInstance(name);
		}
		public static PlayerUnit TryCreatePlayerUnit(string name)
		{
			return TypeFactory<PlayerUnit>.TryCreateInstance(name);
		}
		public static T CreateEnemyUnit<T>() where T : EnemyUnit
		{
			return TypeFactory<EnemyUnit>.CreateInstance<T>();
		}
		public static EnemyUnit CreateEnemyUnit(Type enemyUnitType)
		{
			return TypeFactory<EnemyUnit>.CreateInstance(enemyUnitType);
		}
		public static EnemyUnit CreateEnemyUnit(string name)
		{
			return TypeFactory<EnemyUnit>.CreateInstance(name);
		}
		public static EnemyUnit TryCreateEnemyUnit(string name)
		{
			return TypeFactory<EnemyUnit>.TryCreateInstance(name);
		}
		public static T CreateStatusEffect<T>() where T : StatusEffect
		{
			return TypeFactory<StatusEffect>.CreateInstance<T>();
		}
		public static StatusEffect CreateStatusEffect(Type statusEffectType)
		{
			return TypeFactory<StatusEffect>.CreateInstance(statusEffectType);
		}
		public static StatusEffect CreateStatusEffect(string name)
		{
			return TypeFactory<StatusEffect>.CreateInstance(name);
		}
		public static StatusEffect TryCreateStatusEffect(string name)
		{
			return TypeFactory<StatusEffect>.TryCreateInstance(name);
		}
		public static T CreateDoll<T>() where T : Doll
		{
			return TypeFactory<Doll>.CreateInstance<T>();
		}
		public static Doll CreateDoll(Type dollType)
		{
			return TypeFactory<Doll>.CreateInstance(dollType);
		}
		public static Doll CreateDoll(string id)
		{
			return TypeFactory<Doll>.CreateInstance(id);
		}
		public static Doll TryCreate(string id)
		{
			return TypeFactory<Doll>.TryCreateInstance(id);
		}
		public static T CreateGapOption<T>() where T : GapOption
		{
			return TypeFactory<GapOption>.CreateInstance<T>();
		}
		public static GapOption CreateGapOption(Type gapOptionType)
		{
			return TypeFactory<GapOption>.CreateInstance(gapOptionType);
		}
		public static GapOption CreateGapOption(string name)
		{
			return TypeFactory<GapOption>.CreateInstance(name);
		}
		public static T CreateIntention<T>() where T : Intention
		{
			return TypeFactory<Intention>.CreateInstance<T>();
		}
		public static Intention CreateIntention(Type intentionType)
		{
			return TypeFactory<Intention>.CreateInstance(intentionType);
		}
		public static Intention CreateIntention(string name)
		{
			return TypeFactory<Intention>.CreateInstance(name);
		}
		public static T CreateAdventure<T>() where T : Adventure
		{
			return TypeFactory<Adventure>.CreateInstance<T>();
		}
		public static Adventure CreateAdventure(Type adventureType)
		{
			return TypeFactory<Adventure>.CreateInstance(adventureType);
		}
		public static Adventure CreateAdventure(string name)
		{
			return TypeFactory<Adventure>.CreateInstance(name);
		}
		public static Adventure TryCreateAdventure(string name)
		{
			return TypeFactory<Adventure>.TryCreateInstance(name);
		}
		public static float WeightForExhibit(Type type, GameRunController gameRun)
		{
			IExhibitWeighter exhibitWeighter;
			if (Library._exhibitWeighterTable.TryGetValue(type, ref exhibitWeighter))
			{
				return exhibitWeighter.WeightFor(type, gameRun);
			}
			return 1f;
		}
		public static float WeightForAdventure(Type type, GameRunController gameRun)
		{
			IAdventureWeighter adventureWeighter;
			if (Library._adventureWeighterTable.TryGetValue(type, ref adventureWeighter))
			{
				return adventureWeighter.WeightFor(type, gameRun);
			}
			return 1f;
		}
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
		public static EnemyGroupEntry TryGetEnemyGroupEntry(string id)
		{
			EnemyGroupConfig enemyGroupConfig = EnemyGroupConfig.FromId(id);
			if (enemyGroupConfig != null)
			{
				return Library.CreateEnemyGroupEntryFromConfig(enemyGroupConfig);
			}
			return null;
		}
		public static EnemyGroupEntry GetEnemyGroupEntry(string id)
		{
			EnemyGroupConfig enemyGroupConfig = EnemyGroupConfig.FromId(id);
			if (enemyGroupConfig == null)
			{
				throw new InvalidDataException("Cannot get enemy group entry '" + id + "'");
			}
			return Library.CreateEnemyGroupEntryFromConfig(enemyGroupConfig);
		}
		public static EnemyGroup GenerateEnemyGroup(GameRunController gameRun, string name)
		{
			return Library.GetEnemyGroupEntry(name).Generate(gameRun);
		}
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
		public static PlayerUnit[] GetSelectablePlayers()
		{
			return Enumerable.ToArray<PlayerUnit>(Enumerable.Select<PlayerUnitConfig, PlayerUnit>(Enumerable.Where<PlayerUnitConfig>(PlayerUnitConfig.AllConfig(), (PlayerUnitConfig config) => config.IsSelectable), (PlayerUnitConfig config) => Library.CreatePlayerUnit(config.Id)));
		}
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
		public static IEnumerable<EnemyGroupEntry> EnumerateEnemyGroupEntries()
		{
			return Enumerable.Select<EnemyGroupConfig, EnemyGroupEntry>(EnemyGroupConfig.AllConfig(), new Func<EnemyGroupConfig, EnemyGroupEntry>(Library.CreateEnemyGroupEntryFromConfig));
		}
		public static IEnumerable<Type> EnumerateAdventureTypes()
		{
			return TypeFactory<Adventure>.AllTypes;
		}
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
		private static bool _registered;
		private static Dictionary<Type, IExhibitWeighter> _exhibitWeighterTable;
		private static Dictionary<Type, IAdventureWeighter> _adventureWeighterTable;
	}
}
