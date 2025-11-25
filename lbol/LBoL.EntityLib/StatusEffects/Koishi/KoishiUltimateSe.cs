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
	[UsedImplicitly]
	public sealed class KoishiUltimateSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarting));
		}
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
