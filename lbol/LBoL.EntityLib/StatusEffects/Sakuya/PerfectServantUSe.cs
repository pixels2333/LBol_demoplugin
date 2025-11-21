using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Sakuya
{
	// Token: 0x0200001C RID: 28
	[UsedImplicitly]
	public sealed class PerfectServantUSe : StatusEffect
	{
		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600003C RID: 60 RVA: 0x00002667 File Offset: 0x00000867
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Philosophies(1);
			}
		}

		// Token: 0x0600003D RID: 61 RVA: 0x0000266F File Offset: 0x0000086F
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}

		// Token: 0x0600003E RID: 62 RVA: 0x0000268E File Offset: 0x0000088E
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			base.NotifyActivating();
			yield return new GainManaAction(this.Mana * base.Level);
			yield return new DrawManyCardAction(base.Level);
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}
	}
}
