using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x020000B6 RID: 182
	[UsedImplicitly]
	public sealed class WindGirl : StatusEffect
	{
		// Token: 0x06000829 RID: 2089 RVA: 0x000182A8 File Offset: 0x000164A8
		protected override void OnAdded(Unit unit)
		{
			this.React(new ApplyStatusEffectAction<Graze>(base.Owner, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true));
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnEnding));
		}

		// Token: 0x0600082A RID: 2090 RVA: 0x00018314 File Offset: 0x00016514
		private IEnumerable<BattleAction> OnOwnerTurnEnding(GameEventArgs args)
		{
			Graze statusEffect = base.Owner.GetStatusEffect<Graze>();
			int num = ((statusEffect != null) ? statusEffect.Level : 0);
			if (num < base.Level)
			{
				yield return new ApplyStatusEffectAction<Graze>(base.Owner, new int?(base.Level - num), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}

		// Token: 0x0600082B RID: 2091 RVA: 0x00018324 File Offset: 0x00016524
		public override bool Stack(StatusEffect other)
		{
			bool flag = base.Stack(other);
			if (flag)
			{
				this.React(new ApplyStatusEffectAction<Graze>(base.Owner, new int?(other.Level), default(int?), default(int?), default(int?), 0f, true));
			}
			return flag;
		}
	}
}
