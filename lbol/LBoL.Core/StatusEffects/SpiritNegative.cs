using System;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Units;
namespace LBoL.Core.StatusEffects
{
	[UsedImplicitly]
	public sealed class SpiritNegative : StatusEffect, IOpposing<Spirit>
	{
		public OpposeResult Oppose(Spirit other)
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
