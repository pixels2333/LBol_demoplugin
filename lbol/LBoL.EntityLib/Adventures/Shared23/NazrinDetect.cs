using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Cards;
using Yarn;

namespace LBoL.EntityLib.Adventures.Shared23
{
	// Token: 0x02000518 RID: 1304
	[AdventureInfo(WeighterType = typeof(NazrinDetect.NazrinDetectWeighter))]
	public sealed class NazrinDetect : Adventure
	{
		// Token: 0x0600111A RID: 4378 RVA: 0x0001F025 File Offset: 0x0001D225
		protected override void InitVariables(IVariableStorage storage)
		{
			storage.SetValue("$hpReward", (float)base.GameRun.Player.MaxHp);
		}

		// Token: 0x0600111B RID: 4379 RVA: 0x0001F044 File Offset: 0x0001D244
		[RuntimeCommand("cardReward", "")]
		[UsedImplicitly]
		public void RewardCard()
		{
			Card randomCurseCard = base.GameRun.GetRandomCurseCard(base.GameRun.AdventureRng, false);
			base.Storage.SetValue("$cardReward", randomCurseCard.ToString());
		}

		// Token: 0x0600111C RID: 4380 RVA: 0x0001F080 File Offset: 0x0001D280
		[RuntimeCommand("exhibitReward", "")]
		[UsedImplicitly]
		private void RewardExhibit()
		{
			Exhibit eliteEnemyExhibit = base.Stage.GetEliteEnemyExhibit();
			base.Storage.SetValue("$exhibitReward", eliteEnemyExhibit.Id);
			base.Storage.SetValue("$isSentinel", eliteEnemyExhibit.Config.IsSentinel);
		}

		// Token: 0x02000A72 RID: 2674
		private class NazrinDetectWeighter : IAdventureWeighter
		{
			// Token: 0x0600376B RID: 14187 RVA: 0x000869A6 File Offset: 0x00084BA6
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)((gameRun.Money >= 50) ? 1 : 0);
			}
		}
	}
}
