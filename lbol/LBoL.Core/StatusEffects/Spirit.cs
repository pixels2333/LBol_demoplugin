using System;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Units;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x020000A7 RID: 167
	[UsedImplicitly]
	public sealed class Spirit : StatusEffect, IOpposing<SpiritNegative>
	{
		// Token: 0x06000795 RID: 1941 RVA: 0x00016564 File Offset: 0x00014764
		public OpposeResult Oppose(SpiritNegative other)
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

		// Token: 0x06000796 RID: 1942 RVA: 0x000165B8 File Offset: 0x000147B8
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
