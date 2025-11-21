using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Randoms;
using LBoL.Core.SaveData;
using LBoL.Core.Stations;
using LBoL.Core.Units;
using LBoL.EntityLib.Adventures.Common;
using LBoL.EntityLib.Adventures.Shared12;
using LBoL.EntityLib.Adventures.Stage1;

namespace LBoL.EntityLib.Stages.NormalStages
{
	// Token: 0x02000100 RID: 256
	[UsedImplicitly]
	public sealed class BambooForest : NormalStageBase
	{
		// Token: 0x06000398 RID: 920 RVA: 0x00009750 File Offset: 0x00007950
		public BambooForest()
		{
			base.Level = 1;
			base.CardUpgradedChance = 0f;
			base.IsSelectingBoss = true;
			base.EnemyPoolAct1 = new UniqueRandomPool<string>(true)
			{
				{ "11", 1f },
				{ "12", 1f },
				{ "13", 1f }
			};
			base.EnemyPoolAct2 = new UniqueRandomPool<string>(true)
			{
				{ "14", 1f },
				{ "15", 1f },
				{ "16", 1f }
			};
			base.EnemyPoolAct3 = new UniqueRandomPool<string>(true)
			{
				{ "17", 1f },
				{ "18", 1f },
				{ "19", 1f }
			};
			base.EliteEnemyPool = new UniqueRandomPool<string>(true)
			{
				{ "Sanyue", 1f },
				{ "Aya", 1f },
				{ "Rin", 1f }
			};
			base.AdventurePool = new UniqueRandomPool<Type>(true)
			{
				{
					typeof(YorigamiSisters),
					1f
				},
				{
					typeof(HecatiaTshirt),
					1f
				},
				{
					typeof(KeineSales),
					1f
				},
				{
					typeof(MikeInvest),
					1f
				},
				{
					typeof(YoumuDelivery),
					1f
				},
				{
					typeof(AssistKagerou),
					1.2f
				},
				{
					typeof(EternityAscension),
					1.2f
				},
				{
					typeof(KaguyaVersusMokou),
					1.2f
				},
				{
					typeof(MystiaBbq),
					1.2f
				},
				{
					typeof(ParseeJealousy),
					1.2f
				},
				{
					typeof(RumiaDriving),
					1.2f
				},
				{
					typeof(TewiThreat),
					1.2f
				}
			};
		}

		// Token: 0x06000399 RID: 921 RVA: 0x00009974 File Offset: 0x00007B74
		public override void InitExtraFlags(ProfileSaveData userProfile)
		{
			if (!userProfile.EnemyGroupRevealed.Contains("Sanyue"))
			{
				base.ExtraFlags.Add("ForceSanyue");
			}
			if (!userProfile.EnemyGroupRevealed.Contains("11"))
			{
				base.ExtraFlags.Add("Force11");
			}
			if (!userProfile.EnemyGroupRevealed.Contains("14"))
			{
				base.ExtraFlags.Add("Force14");
			}
			if (!userProfile.EnemyGroupRevealed.Contains("17"))
			{
				base.ExtraFlags.Add("Force17");
			}
		}

		// Token: 0x0600039A RID: 922 RVA: 0x00009A10 File Offset: 0x00007C10
		public override EnemyGroupEntry GetEnemies(Station station)
		{
			switch (station.Act)
			{
			case 1:
				if (base.ExtraFlags.Contains("Force11"))
				{
					base.ExtraFlags.Remove("Force11");
					base.EnemyPoolAct1.Remove("11", false);
					return Library.GetEnemyGroupEntry("11");
				}
				break;
			case 2:
				if (base.ExtraFlags.Contains("Force14"))
				{
					base.ExtraFlags.Remove("Force14");
					base.EnemyPoolAct2.Remove("14", false);
					return Library.GetEnemyGroupEntry("14");
				}
				break;
			case 3:
				if (base.ExtraFlags.Contains("Force17"))
				{
					base.ExtraFlags.Remove("Force17");
					base.EnemyPoolAct3.Remove("17", false);
					return Library.GetEnemyGroupEntry("17");
				}
				break;
			}
			return base.GetEnemies(station);
		}

		// Token: 0x0600039B RID: 923 RVA: 0x00009B04 File Offset: 0x00007D04
		public override EnemyGroupEntry GetEliteEnemies(Station station)
		{
			if (base.ExtraFlags.Contains("ForceSanyue"))
			{
				base.ExtraFlags.Remove("ForceSanyue");
				base.EliteEnemyPool.Remove("Sanyue", false);
				return Library.GetEnemyGroupEntry("Sanyue");
			}
			if (station.Act == 1)
			{
				string text = base.EliteEnemyPool.Without("Aya").Sample(base.GameRun.StationRng);
				base.EliteEnemyPool.Remove(text, false);
				return Library.GetEnemyGroupEntry(text);
			}
			return base.GetEliteEnemies(station);
		}

		// Token: 0x04000034 RID: 52
		private const float W = 1.2f;

		// Token: 0x04000035 RID: 53
		private const string ForceSanyueFlag = "ForceSanyue";

		// Token: 0x04000036 RID: 54
		private const string Force11Flag = "Force11";

		// Token: 0x04000037 RID: 55
		private const string Force14Flag = "Force14";

		// Token: 0x04000038 RID: 56
		private const string Force17Flag = "Force17";
	}
}
