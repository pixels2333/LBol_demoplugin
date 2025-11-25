using System;
using System.Collections.Generic;
using LBoL.Base.Extensions;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class CastBlockShieldAction : EventBattleAction<BlockShieldEventArgs>
	{
		public bool Cast { get; }
		public CastBlockShieldAction(Unit source, Unit target, int block, int shield, BlockShieldType type = BlockShieldType.Normal, bool cast = true)
		{
			base.Args = new BlockShieldEventArgs
			{
				Source = source,
				Target = target,
				Block = (float)block,
				Shield = (float)shield,
				Type = type,
				HasBlock = (block > 0),
				HasShield = (shield > 0)
			};
			this.Cast = cast;
		}
		public CastBlockShieldAction(Unit target, int block, int shield, BlockShieldType type = BlockShieldType.Normal, bool cast = true)
			: this(target, target, block, shield, type, cast)
		{
		}
		public CastBlockShieldAction(Unit source, Unit target, BlockInfo block, bool cast = true)
			: this(source, target, block.Block, 0, block.Type, cast)
		{
		}
		public CastBlockShieldAction(Unit target, BlockInfo block, bool cast = true)
			: this(target, target, block.Block, 0, block.Type, cast)
		{
		}
		public CastBlockShieldAction(Unit source, Unit target, ShieldInfo shield, bool cast = true)
			: this(source, target, 0, shield.Shield, shield.Type, cast)
		{
		}
		public CastBlockShieldAction(Unit target, ShieldInfo shield, bool cast = true)
			: this(target, target, 0, shield.Shield, shield.Type, cast)
		{
		}
		public CastBlockShieldAction(Unit source, Unit target, BlockInfo block, ShieldInfo shield, bool cast = true)
			: this(source, target, block.Block, shield.Shield, block.Type, cast)
		{
			if (shield.Type != block.Type)
			{
				throw new ArgumentException(string.Format("Block({0}) and Shield({1}) has different type", block.Type, shield.Type));
			}
		}
		public CastBlockShieldAction(Unit target, BlockInfo block, ShieldInfo shield, bool cast = true)
			: this(target, target, block.Block, shield.Shield, block.Type, cast)
		{
			if (shield.Type != block.Type)
			{
				throw new ArgumentException(string.Format("Block({0}) and Shield({1}) has different type", block.Type, shield.Type));
			}
		}
		internal override IEnumerable<Phase> GetPhases()
		{
			if (base.Args.Target.IsInvalidTarget)
			{
				base.Args.ForceCancelBecause(CancelCause.InvalidTarget);
				yield break;
			}
			string text = "BlockShieldCasting";
			BlockShieldEventArgs args = base.Args;
			Unit source = base.Args.Source;
			yield return base.CreateEventPhase<BlockShieldEventArgs>(text, args, (source != null) ? source.BlockShieldCasting : null);
			yield return base.CreateEventPhase<BlockShieldEventArgs>("BlockShieldGaining", base.Args, base.Args.Target.BlockShieldGaining);
			yield return base.CreatePhase("Main", delegate
			{
				ValueTuple<int, int> valueTuple = base.Battle.CastBlockShield(base.Args.Target, base.Args.Block, base.Args.Shield);
				int item = valueTuple.Item1;
				int item2 = valueTuple.Item2;
				if (!base.Args.Block.Approximately((float)item) || base.Args.Shield.Approximately((float)item2))
				{
					BlockShieldEventArgs args3 = base.Args;
					BlockShieldEventArgs args4 = base.Args;
					float num = (float)item;
					float num2 = (float)item2;
					args3.Block = num;
					args4.Shield = num2;
					base.Args.IsModified = true;
				}
			}, true);
			string text2 = "BlockShieldCasted";
			BlockShieldEventArgs args2 = base.Args;
			Unit source2 = base.Args.Source;
			yield return base.CreateEventPhase<BlockShieldEventArgs>(text2, args2, (source2 != null) ? source2.BlockShieldCasted : null);
			yield return base.CreateEventPhase<BlockShieldEventArgs>("BlockShieldGained", base.Args, base.Args.Target.BlockShieldGained);
			yield break;
		}
	}
}
