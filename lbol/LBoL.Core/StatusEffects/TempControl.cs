using System;
using JetBrains.Annotations;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x020000AD RID: 173
	[UsedImplicitly]
	public sealed class TempControl : StatusEffect, IOpposing<TempControlNegative>
	{
		// Token: 0x06000803 RID: 2051 RVA: 0x000179E9 File Offset: 0x00015BE9
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DollValueArgs>(base.Owner.DollValueGenerating, delegate(DollValueArgs args)
			{
				args.Value += base.Level;
				args.AddModifier(this);
			});
			base.HandleOwnerEvent<UnitEventArgs>(unit.TurnEnded, delegate(UnitEventArgs _)
			{
				this.React(new RemoveStatusEffectAction(this, true, 0.1f));
			});
		}

		// Token: 0x06000804 RID: 2052 RVA: 0x00017A20 File Offset: 0x00015C20
		public OpposeResult Oppose(TempControlNegative other)
		{
			if (base.Level < other.Level)
			{
				other.Level -= base.Level;
				return OpposeResult.KeepOther;
			}
			if (base.Level == other.Level)
			{
				return OpposeResult.Neutralize;
			}
			base.Level -= other.Level;
			return OpposeResult.KeepSelf;
		}
	}
}
