using System;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x020000B2 RID: 178
	[UsedImplicitly]
	public sealed class TempSpiritNegative : StatusEffect, IOpposing<TempSpirit>
	{
		// Token: 0x0600081A RID: 2074 RVA: 0x00017F80 File Offset: 0x00016180
		public OpposeResult Oppose(TempSpirit other)
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

		// Token: 0x0600081B RID: 2075 RVA: 0x00017FD4 File Offset: 0x000161D4
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
						args.Block = Math.Max(args.Block - (float)base.Level, 0f);
					}
					if (args.HasShield)
					{
						args.Shield = Math.Max(args.Shield - (float)base.Level, 0f);
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
						args.Block = Math.Max(args.Block - (float)base.Level, 0f);
					}
					if (args.HasShield)
					{
						args.Shield = Math.Max(args.Shield - (float)base.Level, 0f);
					}
					args.AddModifier(this);
				}
			});
		}
	}
}
