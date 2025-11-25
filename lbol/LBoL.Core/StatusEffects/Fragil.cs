using System;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Units;
using UnityEngine;
namespace LBoL.Core.StatusEffects
{
	[UsedImplicitly]
	public sealed class Fragil : StatusEffect
	{
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
		protected override void OnAdded(Unit unit)
		{
			if (unit is PlayerUnit)
			{
				base.HandleOwnerEvent<BlockShieldEventArgs>(unit.BlockShieldGaining, new GameEventHandler<BlockShieldEventArgs>(this.OnBlockGaining));
				return;
			}
			Debug.LogError(this.Name + " added to enemy " + unit.Name + ", which has no effect.");
		}
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
