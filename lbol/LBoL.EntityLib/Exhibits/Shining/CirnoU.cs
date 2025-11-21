using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.EntityLib.Cards.Character.Cirno;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x02000124 RID: 292
	[UsedImplicitly]
	public sealed class CirnoU : ShiningExhibit
	{
		// Token: 0x06000403 RID: 1027 RVA: 0x0000B05F File Offset: 0x0000925F
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x06000404 RID: 1028 RVA: 0x0000B083 File Offset: 0x00009283
		private IEnumerable<BattleAction> OnPlayerTurnStarted(GameEventArgs args)
		{
			if (base.Battle.Player.TurnCounter == 1)
			{
				base.NotifyActivating();
				yield return new AddCardsToHandAction(Library.CreateCards<IceWing>(base.Value1, false), AddCardsType.Normal);
			}
			yield break;
		}
	}
}
