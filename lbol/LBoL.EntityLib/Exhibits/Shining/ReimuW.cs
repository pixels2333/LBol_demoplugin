using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x0200013A RID: 314
	[UsedImplicitly]
	public sealed class ReimuW : ShiningExhibit
	{
		// Token: 0x0600044E RID: 1102 RVA: 0x0000B7D0 File Offset: 0x000099D0
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding));
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
		}

		// Token: 0x0600044F RID: 1103 RVA: 0x0000B81C File Offset: 0x00009A1C
		private IEnumerable<BattleAction> OnPlayerTurnEnding(UnitEventArgs args)
		{
			if (base.Battle.BattleMana != ManaGroup.Empty)
			{
				base.NotifyActivating();
				yield return new CastBlockShieldAction(base.Owner, 0, base.Value1, BlockShieldType.Normal, true);
			}
			yield break;
		}

		// Token: 0x06000450 RID: 1104 RVA: 0x0000B82C File Offset: 0x00009A2C
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			base.NotifyActivating();
			yield return new CastBlockShieldAction(base.Owner, 0, base.Value2, BlockShieldType.Normal, true);
			yield break;
		}
	}
}
