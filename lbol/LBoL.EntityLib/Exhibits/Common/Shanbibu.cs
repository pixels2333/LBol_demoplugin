using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x0200018A RID: 394
	[UsedImplicitly]
	public sealed class Shanbibu : Exhibit
	{
		// Token: 0x0600058C RID: 1420 RVA: 0x0000D740 File Offset: 0x0000B940
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, delegate(UnitEventArgs _)
			{
				if (base.Battle.Player.TurnCounter == 2)
				{
					base.Blackout = true;
				}
			});
		}

		// Token: 0x0600058D RID: 1421 RVA: 0x0000D791 File Offset: 0x0000B991
		private IEnumerable<BattleAction> OnPlayerTurnStarted(GameEventArgs args)
		{
			if (base.Battle.Player.TurnCounter == 1)
			{
				base.Active = true;
			}
			if (base.Battle.Player.TurnCounter == 2)
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<Graze>(base.Owner, new int?(base.Value1), default(int?), default(int?), default(int?), 0f, true);
				base.Active = false;
			}
			yield break;
		}

		// Token: 0x0600058E RID: 1422 RVA: 0x0000D7A1 File Offset: 0x0000B9A1
		protected override void OnLeaveBattle()
		{
			base.Active = false;
			base.Blackout = false;
		}
	}
}
