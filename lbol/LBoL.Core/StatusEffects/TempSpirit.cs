using System;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x020000B1 RID: 177
	[UsedImplicitly]
	public sealed class TempSpirit : StatusEffect, IOpposing<TempSpiritNegative>
	{
		// Token: 0x06000814 RID: 2068 RVA: 0x00017DE0 File Offset: 0x00015FE0
		public OpposeResult Oppose(TempSpiritNegative other)
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

		// Token: 0x06000815 RID: 2069 RVA: 0x00017E34 File Offset: 0x00016034
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<UnitEventArgs>(unit.TurnEnded, delegate(UnitEventArgs _)
			{
				this.React(new RemoveStatusEffectAction(this, true, 0.1f));
			});
			base.HandleOwnerEvent<BlockShieldEventArgs>(base.Owner.BlockShieldGaining, delegate(BlockShieldEventArgs args)
			{
				if (args.Type == BlockShieldType.Direct)
				{
					return;
				}
				ActionCause cause = args.Cause;
				if (cause == ActionCause.Card || cause == ActionCause.OnlyCalculate || cause == ActionCause.Us)
				{
					if (args.HasBlock)
					{
						args.Block += (float)base.Level;
					}
					if (args.HasShield)
					{
						args.Shield += (float)base.Level;
					}
					args.AddModifier(this);
				}
			});
			base.HandleOwnerEvent<BlockShieldEventArgs>(base.Owner.BlockShieldCasting, delegate(BlockShieldEventArgs args)
			{
				if (args.Type == BlockShieldType.Direct)
				{
					return;
				}
				if (args.Cause == ActionCause.EnemyAction)
				{
					if (args.HasBlock)
					{
						args.Block += (float)base.Level;
					}
					if (args.HasShield)
					{
						args.Shield += (float)base.Level;
					}
					args.AddModifier(this);
				}
			});
		}
	}
}
