using System;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
namespace LBoL.Core.StatusEffects
{
	[UsedImplicitly]
	public sealed class TempSpirit : StatusEffect, IOpposing<TempSpiritNegative>
	{
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
