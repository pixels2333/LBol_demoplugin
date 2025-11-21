using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x020001AD RID: 429
	[UsedImplicitly]
	[ExhibitInfo(WeighterType = typeof(YuekuangQiang.YuekuangQiangWeighter))]
	public sealed class YuekuangQiang : Exhibit
	{
		// Token: 0x06000629 RID: 1577 RVA: 0x0000E534 File Offset: 0x0000C734
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<CardEventArgs>(base.Battle.CardDiscarded, new EventSequencedReactor<CardEventArgs>(this.OnCardDiscarded));
		}

		// Token: 0x0600062A RID: 1578 RVA: 0x0000E553 File Offset: 0x0000C753
		private IEnumerable<BattleAction> OnCardDiscarded(CardEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			EnemyUnit randomAliveEnemy = base.Battle.RandomAliveEnemy;
			if (randomAliveEnemy != null)
			{
				base.NotifyActivating();
				yield return new DamageAction(base.Battle.Player, randomAliveEnemy, DamageInfo.Reaction((float)base.Value1, false), "ExhYuekuang", GunType.Single);
			}
			yield break;
		}

		// Token: 0x0200066C RID: 1644
		private class YuekuangQiangWeighter : IExhibitWeighter
		{
			// Token: 0x06001A9B RID: 6811 RVA: 0x00036DC8 File Offset: 0x00034FC8
			public float WeightFor(Type type, GameRunController gameRun)
			{
				if (gameRun.Player.HasExhibit<ShanshuoBishou>())
				{
					return 1f;
				}
				if (Enumerable.Any<Card>(gameRun.BaseDeck, (Card card) => card.DiscardCard))
				{
					return 1f;
				}
				return 0f;
			}
		}
	}
}
