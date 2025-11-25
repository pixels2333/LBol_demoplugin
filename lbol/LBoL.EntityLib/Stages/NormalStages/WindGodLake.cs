using System;
using JetBrains.Annotations;
using LBoL.Core.Randoms;
using LBoL.EntityLib.Adventures;
using LBoL.EntityLib.Adventures.Common;
using LBoL.EntityLib.Adventures.FirstPlace;
using LBoL.EntityLib.Adventures.Shared23;
using LBoL.EntityLib.Adventures.Stage3;
namespace LBoL.EntityLib.Stages.NormalStages
{
	[UsedImplicitly]
	public sealed class WindGodLake : NormalStageBase
	{
		public WindGodLake()
		{
			base.Level = 3;
			base.CardUpgradedChance = 0.5f;
			base.TradeAdventureType = typeof(SumirekoGathering);
			base.EnemyPoolAct1 = new UniqueRandomPool<string>(true)
			{
				{ "31", 1f },
				{ "32", 1f },
				{ "33", 1f }
			};
			base.EnemyPoolAct2 = new UniqueRandomPool<string>(true)
			{
				{ "34", 1f },
				{ "35", 1f },
				{ "36", 1f }
			};
			base.EnemyPoolAct3 = new UniqueRandomPool<string>(true)
			{
				{ "37", 1f },
				{ "38", 1f },
				{ "39", 1f }
			};
			base.EliteEnemyPool = new UniqueRandomPool<string>(true)
			{
				{ "Clownpiece", 1f },
				{ "Siji", 1f },
				{ "Doremy", 1f }
			};
			base.BossPool = new RepeatableRandomPool<string>
			{
				{ "Sanae", 1f },
				{ "Remilia", 1f },
				{ "Junko", 1f }
			};
			base.FirstAdventurePool = new UniqueRandomPool<Type>(false)
			{
				{
					typeof(MiyoiBartender),
					1f
				},
				{
					typeof(WatatsukiPurify),
					1f
				},
				{
					typeof(DoremyPortal),
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
					typeof(MedicinePoison),
					1.2f
				},
				{
					typeof(BackgroundDancers),
					1.2f
				},
				{
					typeof(MikoDonation),
					1.2f
				},
				{
					typeof(SatoriCounseling),
					1.2f
				}
			};
			base.TradeAdventureType = typeof(SumirekoGathering);
		}
		private const float W = 1.2f;
	}
}
