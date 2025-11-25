using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Reimu
{
	[UsedImplicitly]
	public sealed class BoliDajiejieSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<BlockShieldEventArgs>(base.Owner.BlockShieldGaining, new GameEventHandler<BlockShieldEventArgs>(this.OnOwnerBlockShieldGaining));
			base.React(new LazySequencedReactor(this.BlockToShield));
		}
		private IEnumerable<BattleAction> BlockToShield()
		{
			int block = base.Owner.Block;
			if (block > 0)
			{
				yield return new LoseBlockShieldAction(base.Owner, block, 0, false);
				yield return new CastBlockShieldAction(base.Owner, 0, block, BlockShieldType.Direct, false);
			}
			yield break;
		}
		private void OnOwnerBlockShieldGaining(BlockShieldEventArgs args)
		{
			if (args.Block != 0f && args.Cause != ActionCause.OnlyCalculate)
			{
				args.Shield += args.Block;
				args.Block = 0f;
				args.AddModifier(this);
				base.NotifyActivating();
			}
		}
	}
}
