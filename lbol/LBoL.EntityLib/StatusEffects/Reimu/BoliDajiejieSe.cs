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
	// Token: 0x02000026 RID: 38
	[UsedImplicitly]
	public sealed class BoliDajiejieSe : StatusEffect
	{
		// Token: 0x06000062 RID: 98 RVA: 0x00002A97 File Offset: 0x00000C97
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<BlockShieldEventArgs>(base.Owner.BlockShieldGaining, new GameEventHandler<BlockShieldEventArgs>(this.OnOwnerBlockShieldGaining));
			base.React(new LazySequencedReactor(this.BlockToShield));
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00002AC8 File Offset: 0x00000CC8
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

		// Token: 0x06000064 RID: 100 RVA: 0x00002AD8 File Offset: 0x00000CD8
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
