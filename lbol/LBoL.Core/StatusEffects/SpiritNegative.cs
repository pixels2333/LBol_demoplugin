using System;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Units;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x020000A8 RID: 168
	[UsedImplicitly]
	public sealed class SpiritNegative : StatusEffect, IOpposing<Spirit>
	{
		// Token: 0x0600079A RID: 1946 RVA: 0x000166C8 File Offset: 0x000148C8
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

		// Token: 0x0600079B RID: 1947 RVA: 0x0001671C File Offset: 0x0001491C
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
