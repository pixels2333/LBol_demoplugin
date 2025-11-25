using System;
using JetBrains.Annotations;
using LBoL.Core.Randoms;
using LBoL.EntityLib.Adventures.Common;
using LBoL.EntityLib.Adventures.FirstPlace;
using LBoL.EntityLib.Adventures.Shared12;
using LBoL.EntityLib.Adventures.Shared23;
using LBoL.EntityLib.Adventures.Stage2;
namespace LBoL.EntityLib.Stages.NormalStages
{
	[UsedImplicitly]
	public sealed class XuanwuRavine : NormalStageBase
	{
		public XuanwuRavine()
		{
			base.Level = 2;
			base.CardUpgradedChance = 0.25f;
			base.EnemyPoolAct1 = new UniqueRandomPool<string>(true)
			{
				{ "21", 1f },
				{ "22", 1f },
				{ "23", 1f }
			};
			base.EnemyPoolAct2 = new UniqueRandomPool<string>(true)
			{
				{ "24", 1f },
				{ "25", 1f },
				{ "26", 1f }
			};
			base.EnemyPoolAct3 = new UniqueRandomPool<string>(true)
			{
				{ "27", 1f },
				{ "28", 1f },
				{ "29", 1f }
			};
			base.EliteEnemyPool = new UniqueRandomPool<string>(true)
			{
				{ "Nitori", 1f },
				{ "Youmu", 1f },
				{ "Kokoro", 1f }
			};
			base.BossPool = new RepeatableRandomPool<string>
			{
				{ "Long", 1f },
				{ "Tianzi", 1f },
				{ "Yuyuko", 1f }
			};
			base.FirstAdventurePool = new UniqueRandomPool<Type>(false)
			{
				{
					typeof(PatchouliPhilosophy),
					1f
				},
				{
					typeof(JunkoColorless),
					1f
				},
				{
					typeof(ShinmyoumaruForge),
					1f
				}
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
					typeof(HatateInterview),
					1f
				},
				{
					typeof(HinaCollect),
					1f
				},
				{
					typeof(KogasaSpook),
					1f
				},
				{
					typeof(KosuzuBookstore),
					1f
				},
				{
					typeof(NarumiOfferCard),
					1f
				},
				{
					typeof(NazrinDetect),
					1f
				},
				{
					typeof(BuduSuanming),
					1.4f
				},
				{
					typeof(YachieOppression),
					1.4f
				},
				{
					typeof(RemiliaMeet),
					1.4f
				},
				{
					typeof(RingoEmp),
					1.4f
				}
			};
		}
		private const float W = 1.4f;
	}
}
