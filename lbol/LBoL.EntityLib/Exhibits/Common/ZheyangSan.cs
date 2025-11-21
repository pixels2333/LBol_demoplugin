using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x020001B4 RID: 436
	[UsedImplicitly]
	public sealed class ZheyangSan : Exhibit
	{
		// Token: 0x06000647 RID: 1607 RVA: 0x0000E930 File Offset: 0x0000CB30
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x06000648 RID: 1608 RVA: 0x0000E954 File Offset: 0x0000CB54
		private IEnumerable<BattleAction> OnPlayerTurnStarted(GameEventArgs args)
		{
			if (base.Battle.Player.TurnCounter == 2)
			{
				base.NotifyActivating();
				yield return new CastBlockShieldAction(base.Owner, base.Owner, base.Value1, 0, BlockShieldType.Normal, true);
				yield return new ApplyStatusEffectAction<TempFirepower>(base.Owner, new int?(base.Value2), default(int?), default(int?), default(int?), 0f, true);
				base.Blackout = true;
			}
			base.Active = base.Battle.Player.TurnCounter == 1;
			yield break;
		}

		// Token: 0x06000649 RID: 1609 RVA: 0x0000E964 File Offset: 0x0000CB64
		protected override void OnLeaveBattle()
		{
			base.Active = false;
			base.Blackout = false;
		}
	}
}
