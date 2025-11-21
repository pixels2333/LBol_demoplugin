using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Marisa;

namespace LBoL.EntityLib.StatusEffects.Marisa
{
	// Token: 0x0200006D RID: 109
	public sealed class PotionJungleSe : StatusEffect
	{
		// Token: 0x0600017B RID: 379 RVA: 0x00004F61 File Offset: 0x00003161
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding));
		}

		// Token: 0x0600017C RID: 380 RVA: 0x00004F85 File Offset: 0x00003185
		private IEnumerable<BattleAction> OnPlayerTurnEnding(GameEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			yield return new AddCardsToDiscardAction(Library.CreateCards<Potion>(base.Level, false), AddCardsType.Normal);
			yield break;
		}
	}
}
