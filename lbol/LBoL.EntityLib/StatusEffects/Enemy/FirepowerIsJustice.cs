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
	// Token: 0x020000A3 RID: 163
	[UsedImplicitly]
	public sealed class FirepowerIsJustice : StatusEffect
	{
		// Token: 0x06000248 RID: 584 RVA: 0x00006B54 File Offset: 0x00004D54
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<StatusEffectApplyEventArgs>(base.Battle.Player.StatusEffectAdded, new EventSequencedReactor<StatusEffectApplyEventArgs>(this.OnPlayerStatusEffectAdded));
			base.ReactOwnerEvent<StatusEffectApplyEventArgs>(base.Owner.StatusEffectAdding, new EventSequencedReactor<StatusEffectApplyEventArgs>(this.OnOwnerStatusEffectAdding));
		}

		// Token: 0x06000249 RID: 585 RVA: 0x00006BA0 File Offset: 0x00004DA0
		private IEnumerable<BattleAction> OnPlayerStatusEffectAdded(StatusEffectApplyEventArgs args)
		{
			if (args.Effect is TempFirepower)
			{
				TempFirepower statusEffect = base.Battle.Player.GetStatusEffect<TempFirepower>();
				int num = ((statusEffect != null) ? statusEffect.Level : 0);
				TempFirepower statusEffect2 = base.Owner.GetStatusEffect<TempFirepower>();
				int num2 = ((statusEffect2 != null) ? statusEffect2.Level : 0);
				if (num2 < num)
				{
					base.NotifyActivating();
					yield return new ApplyStatusEffectAction<TempFirepower>(base.Owner, new int?(num - num2), default(int?), default(int?), default(int?), 0f, true);
				}
			}
			if (args.Effect is Firepower)
			{
				Firepower statusEffect3 = base.Battle.Player.GetStatusEffect<Firepower>();
				int num3 = ((statusEffect3 != null) ? statusEffect3.Level : 0);
				Firepower statusEffect4 = base.Owner.GetStatusEffect<Firepower>();
				int num4 = ((statusEffect4 != null) ? statusEffect4.Level : 0);
				if (num4 < num3)
				{
					base.NotifyActivating();
					yield return new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(num3 - num4), default(int?), default(int?), default(int?), 0f, true);
				}
			}
			yield break;
		}

		// Token: 0x0600024A RID: 586 RVA: 0x00006BB7 File Offset: 0x00004DB7
		private IEnumerable<BattleAction> OnOwnerStatusEffectAdding(StatusEffectApplyEventArgs args)
		{
			StatusEffect effect = args.Effect;
			if (effect is FirepowerNegative || effect is TempFirepowerNegative)
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
