using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x0200008F RID: 143
	[UsedImplicitly]
	public sealed class Appliance : StatusEffect
	{
		// Token: 0x0600020A RID: 522 RVA: 0x00006440 File Offset: 0x00004640
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<StatusEffectApplyEventArgs>(unit.StatusEffectAdding, new EventSequencedReactor<StatusEffectApplyEventArgs>(this.OnStatusEffectAdding));
		}

		// Token: 0x0600020B RID: 523 RVA: 0x0000645A File Offset: 0x0000465A
		private IEnumerable<BattleAction> OnStatusEffectAdding(StatusEffectApplyEventArgs args)
		{
			StatusEffect effect = args.Effect;
			if (effect is Weak || effect is Vulnerable)
			{
				args.CancelBy(this);
				base.NotifyActivating();
				yield return PerformAction.Sfx("Amulet", 0f);
				yield return PerformAction.SePop(base.Owner, args.Effect.Name);
			}
			yield break;
		}
	}
}
