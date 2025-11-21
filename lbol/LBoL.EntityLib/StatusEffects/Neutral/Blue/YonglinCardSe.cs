using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Neutral.Blue;

namespace LBoL.EntityLib.StatusEffects.Neutral.Blue
{
	// Token: 0x0200005E RID: 94
	public sealed class YonglinCardSe : StatusEffect
	{
		// Token: 0x06000147 RID: 327 RVA: 0x000047E3 File Offset: 0x000029E3
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding));
		}

		// Token: 0x06000148 RID: 328 RVA: 0x00004807 File Offset: 0x00002A07
		private IEnumerable<BattleAction> OnPlayerTurnEnding(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<FakeMoon>(base.Level, false), DrawZoneTarget.Random, AddCardsType.Normal);
			yield break;
		}
	}
}
