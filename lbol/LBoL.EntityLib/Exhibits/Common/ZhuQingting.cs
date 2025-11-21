using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x020001B5 RID: 437
	[UsedImplicitly]
	public sealed class ZhuQingting : Exhibit
	{
		// Token: 0x0600064B RID: 1611 RVA: 0x0000E97C File Offset: 0x0000CB7C
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x0600064C RID: 1612 RVA: 0x0000E9A0 File Offset: 0x0000CBA0
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			base.Counter++;
			ValueTuple<int, int> valueTuple = base.Counter.DivRem(base.Value1);
			int item = valueTuple.Item1;
			int item2 = valueTuple.Item2;
			base.Counter = item2;
			if (item != 0)
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<Graze>(base.Owner, new int?(base.Value2), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}
	}
}
