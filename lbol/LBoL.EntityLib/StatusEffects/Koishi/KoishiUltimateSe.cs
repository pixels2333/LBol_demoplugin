using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Koishi
{
	// Token: 0x02000079 RID: 121
	[UsedImplicitly]
	public sealed class KoishiUltimateSe : StatusEffect
	{
		// Token: 0x060001A5 RID: 421 RVA: 0x00005446 File Offset: 0x00003646
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarting));
		}

		// Token: 0x060001A6 RID: 422 RVA: 0x0000546A File Offset: 0x0000366A
		private IEnumerable<BattleAction> OnPlayerTurnStarting(UnitEventArgs args)
		{
			if (base.Battle.DrawZone.Count > 0)
			{
				base.NotifyActivating();
				int level = base.Level;
				int num;
				for (int i = 0; i < level; i = num + 1)
				{
					if (base.Battle.BattleShouldEnd)
					{
						yield break;
					}
					Card card = Enumerable.FirstOrDefault<Card>(base.Battle.DrawZone);
					if (card != null)
					{
						yield return new PlayCardAction(card);
					}
					num = i;
				}
			}
			yield break;
		}
	}
}
