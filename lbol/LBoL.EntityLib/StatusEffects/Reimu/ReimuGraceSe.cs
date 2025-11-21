using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Reimu
{
	// Token: 0x02000032 RID: 50
	[UsedImplicitly]
	public sealed class ReimuGraceSe : StatusEffect
	{
		// Token: 0x06000092 RID: 146 RVA: 0x0000304E File Offset: 0x0000124E
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted));
		}

		// Token: 0x06000093 RID: 147 RVA: 0x00003072 File Offset: 0x00001272
		private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
		{
			yield return new ApplyStatusEffectAction<Grace>(base.Owner, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
	}
}
