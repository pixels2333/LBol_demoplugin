using System;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Units;
using UnityEngine;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x0200009A RID: 154
	[UsedImplicitly]
	public sealed class Fragil : StatusEffect
	{
		// Token: 0x17000271 RID: 625
		// (get) Token: 0x06000767 RID: 1895 RVA: 0x00015DDC File Offset: 0x00013FDC
		[UsedImplicitly]
		public int Value
		{
			get
			{
				GameRunController gameRun = base.GameRun;
				if (gameRun == null || !(base.Owner is EnemyUnit))
				{
					return 30;
				}
				return Math.Min(30 + gameRun.FragilExtraPercentage, 100);
			}
		}

		// Token: 0x06000768 RID: 1896 RVA: 0x00015E14 File Offset: 0x00014014
		protected override void OnAdded(Unit unit)
		{
			if (unit is PlayerUnit)
			{
				base.HandleOwnerEvent<BlockShieldEventArgs>(unit.BlockShieldGaining, new GameEventHandler<BlockShieldEventArgs>(this.OnBlockGaining));
				return;
			}
			Debug.LogError(this.Name + " added to enemy " + unit.Name + ", which has no effect.");
		}

		// Token: 0x06000769 RID: 1897 RVA: 0x00015E64 File Offset: 0x00014064
		private void OnBlockGaining(BlockShieldEventArgs args)
		{
			if (args.Cause != ActionCause.Card && args.Cause != ActionCause.OnlyCalculate)
			{
				return;
			}
			float num = 1f - (float)this.Value / 100f;
			if (args.Type == BlockShieldType.Direct)
			{
				return;
			}
			args.Block *= num;
			args.Shield *= num;
		}
	}
}
