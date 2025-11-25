using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Dolls
{
	[UsedImplicitly]
	public sealed class DefenseDoll : Doll
	{
		[UsedImplicitly]
		public BlockInfo Block1
		{
			get
			{
				return new BlockInfo(base.Value1, BlockShieldType.Direct);
			}
		}
		[UsedImplicitly]
		public BlockInfo Block2
		{
			get
			{
				return new BlockInfo(base.Value2, BlockShieldType.Direct);
			}
		}
		public override int? DownCounter
		{
			get
			{
				return new int?(base.CalculateBlock(this.Block1));
			}
		}
		protected override IEnumerable<BattleAction> PassiveActions()
		{
			base.NotifyPassiveActivating();
			yield return new CastBlockShieldAction(base.Owner, base.Owner, this.Block1, true);
			yield break;
		}
		protected override IEnumerable<BattleAction> ActiveActions()
		{
			base.NotifyActiveActivating();
			yield return new CastBlockShieldAction(base.Owner, base.Owner, this.Block2, true);
			yield break;
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
		{
			base.NotifyActiveActivating();
			yield return new CastBlockShieldAction(base.Owner, base.Owner, this.Block2, true);
			yield break;
		}
	}
}
