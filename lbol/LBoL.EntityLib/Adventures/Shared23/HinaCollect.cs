using System;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Cards;
using Yarn;

namespace LBoL.EntityLib.Adventures.Shared23
{
	// Token: 0x02000514 RID: 1300
	[AdventureInfo(WeighterType = typeof(HinaCollect.HinaCollectWeighter))]
	public sealed class HinaCollect : Adventure
	{
		// Token: 0x0600110D RID: 4365 RVA: 0x0001ECE4 File Offset: 0x0001CEE4
		protected override void InitVariables(IVariableStorage storage)
		{
			Card randomCurseCard = base.GameRun.GetRandomCurseCard(base.GameRun.AdventureRng, false);
			storage.SetValue("$card", randomCurseCard.Id);
		}

		// Token: 0x0600110E RID: 4366 RVA: 0x0001ED1A File Offset: 0x0001CF1A
		[RuntimeCommand("yes", "")]
		[UsedImplicitly]
		public void Yes()
		{
			base.GameRun.RemoveDeckCards(Enumerable.Where<Card>(base.GameRun.BaseDeckWithoutUnremovable, (Card c) => c.CardType == CardType.Misfortune), true);
		}

		// Token: 0x02000A6B RID: 2667
		private class HinaCollectWeighter : IAdventureWeighter
		{
			// Token: 0x06003753 RID: 14163 RVA: 0x0008663D File Offset: 0x0008483D
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)(Enumerable.Any<Card>(gameRun.BaseDeckWithoutUnremovable, (Card c) => c.CardType == CardType.Misfortune) ? 1 : 0);
			}
		}
	}
}
