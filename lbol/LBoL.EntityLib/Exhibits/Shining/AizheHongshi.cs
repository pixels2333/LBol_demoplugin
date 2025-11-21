using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.Cards.Enemy;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x0200011F RID: 287
	[UsedImplicitly]
	public sealed class AizheHongshi : ShiningExhibit
	{
		// Token: 0x060003F4 RID: 1012 RVA: 0x0000AEB4 File Offset: 0x000090B4
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x060003F5 RID: 1013 RVA: 0x0000AED8 File Offset: 0x000090D8
		private IEnumerable<BattleAction> OnPlayerTurnStarted(GameEventArgs args)
		{
			if (base.Battle.Player.TurnCounter == 1)
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(base.Value1), default(int?), default(int?), default(int?), 0f, true);
				yield return new AddCardsToDiscardAction(Library.CreateCards<Riguang>(base.Value2, false), AddCardsType.Normal);
			}
			yield break;
		}
	}
}
