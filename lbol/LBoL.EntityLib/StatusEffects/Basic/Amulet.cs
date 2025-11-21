using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Neutral.TwoColor;
using UnityEngine;

namespace LBoL.EntityLib.StatusEffects.Basic
{
	// Token: 0x020000EB RID: 235
	[UsedImplicitly]
	public sealed class Amulet : StatusEffect
	{
		// Token: 0x06000346 RID: 838 RVA: 0x00008A65 File Offset: 0x00006C65
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<StatusEffectApplyEventArgs>(base.Owner.StatusEffectAdding, new EventSequencedReactor<StatusEffectApplyEventArgs>(this.OnStatusEffectAdding));
		}

		// Token: 0x06000347 RID: 839 RVA: 0x00008A84 File Offset: 0x00006C84
		private IEnumerable<BattleAction> OnStatusEffectAdding(StatusEffectApplyEventArgs args)
		{
			if (base.Level <= 0)
			{
				Debug.LogError(string.Format("Activating {0}'s {1} with level = {2}", base.Owner.Name, this.Name, base.Level));
				yield break;
			}
			if (args.Effect is LockedOn)
			{
				TianziRockSe statusEffect = base.Owner.GetStatusEffect<TianziRockSe>();
				if (statusEffect != null && statusEffect.Limit == 1)
				{
					yield break;
				}
			}
			if (args.Effect.Type == StatusEffectType.Negative && !args.IsCanceled)
			{
				int num = base.Level - 1;
				base.Level = num;
				args.CancelBy(this);
				base.NotifyActivating();
				yield return PerformAction.Sfx("Amulet", 0f);
				yield return PerformAction.SePop(base.Owner, args.Effect.Name);
				if (base.Level <= 0)
				{
					yield return new RemoveStatusEffectAction(this, true, 0.1f);
				}
			}
			yield break;
		}
	}
}
