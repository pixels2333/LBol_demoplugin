using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Green
{
	// Token: 0x02000300 RID: 768
	[UsedImplicitly]
	public sealed class TakaneDefense : Card
	{
		// Token: 0x06000B6B RID: 2923 RVA: 0x00016F18 File Offset: 0x00015118
		public override ManaGroup GetXCostFromPooled(ManaGroup pooledMana)
		{
			return pooledMana;
		}

		// Token: 0x17000148 RID: 328
		// (get) Token: 0x06000B6C RID: 2924 RVA: 0x00016F1B File Offset: 0x0001511B
		[UsedImplicitly]
		public override BlockInfo Block
		{
			get
			{
				return this.CalculateBlock(base.PendingManaUsage);
			}
		}

		// Token: 0x06000B6D RID: 2925 RVA: 0x00016F2C File Offset: 0x0001512C
		private BlockInfo CalculateBlock(ManaGroup? manaGroup)
		{
			if (manaGroup != null)
			{
				ManaGroup valueOrDefault = manaGroup.GetValueOrDefault();
				return new BlockInfo(this.RawBlock + (base.SynergyAmount(valueOrDefault, ManaColor.Any, 1) - 1) * base.Value1, BlockShieldType.Normal);
			}
			return new BlockInfo(this.RawBlock, BlockShieldType.Normal);
		}

		// Token: 0x06000B6E RID: 2926 RVA: 0x00016F76 File Offset: 0x00015176
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield break;
		}
	}
}
