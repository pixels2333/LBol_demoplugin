using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.Green
{
	[UsedImplicitly]
	public sealed class TakaneDefense : Card
	{
		public override ManaGroup GetXCostFromPooled(ManaGroup pooledMana)
		{
			return pooledMana;
		}
		[UsedImplicitly]
		public override BlockInfo Block
		{
			get
			{
				return this.CalculateBlock(base.PendingManaUsage);
			}
		}
		private BlockInfo CalculateBlock(ManaGroup? manaGroup)
		{
			if (manaGroup != null)
			{
				ManaGroup valueOrDefault = manaGroup.GetValueOrDefault();
				return new BlockInfo(this.RawBlock + (base.SynergyAmount(valueOrDefault, ManaColor.Any, 1) - 1) * base.Value1, BlockShieldType.Normal);
			}
			return new BlockInfo(this.RawBlock, BlockShieldType.Normal);
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield break;
		}
	}
}
