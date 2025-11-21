using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Dolls
{
	// Token: 0x02000255 RID: 597
	[UsedImplicitly]
	public sealed class DefenseDoll : Doll
	{
		// Token: 0x17000120 RID: 288
		// (get) Token: 0x060009A6 RID: 2470 RVA: 0x00014C8C File Offset: 0x00012E8C
		[UsedImplicitly]
		public BlockInfo Block1
		{
			get
			{
				return new BlockInfo(base.Value1, BlockShieldType.Direct);
			}
		}

		// Token: 0x17000121 RID: 289
		// (get) Token: 0x060009A7 RID: 2471 RVA: 0x00014C9A File Offset: 0x00012E9A
		[UsedImplicitly]
		public BlockInfo Block2
		{
			get
			{
				return new BlockInfo(base.Value2, BlockShieldType.Direct);
			}
		}

		// Token: 0x17000122 RID: 290
		// (get) Token: 0x060009A8 RID: 2472 RVA: 0x00014CA8 File Offset: 0x00012EA8
		public override int? DownCounter
		{
			get
			{
				return new int?(base.CalculateBlock(this.Block1));
			}
		}

		// Token: 0x060009A9 RID: 2473 RVA: 0x00014CBB File Offset: 0x00012EBB
		protected override IEnumerable<BattleAction> PassiveActions()
		{
			base.NotifyPassiveActivating();
			yield return new CastBlockShieldAction(base.Owner, base.Owner, this.Block1, true);
			yield break;
		}

		// Token: 0x060009AA RID: 2474 RVA: 0x00014CCB File Offset: 0x00012ECB
		protected override IEnumerable<BattleAction> ActiveActions()
		{
			base.NotifyActiveActivating();
			yield return new CastBlockShieldAction(base.Owner, base.Owner, this.Block2, true);
			yield break;
		}

		// Token: 0x060009AB RID: 2475 RVA: 0x00014CDB File Offset: 0x00012EDB
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
		{
			base.NotifyActiveActivating();
			yield return new CastBlockShieldAction(base.Owner, base.Owner, this.Block2, true);
			yield break;
		}
	}
}
