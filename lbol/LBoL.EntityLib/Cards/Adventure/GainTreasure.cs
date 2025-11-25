using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Adventure
{
	[UsedImplicitly]
	public sealed class GainTreasure : Card
	{
		[UsedImplicitly]
		public int TotalMoney
		{
			get
			{
				if (base.GameRun == null)
				{
					return 0;
				}
				return base.GameRun.Stats.TotalGainTreasure;
			}
		}
		protected override string GetBaseDescription()
		{
			if (!((base.Battle != null) | (this.TotalMoney != 0)))
			{
				return base.GetBaseDescription();
			}
			return base.GetExtraDescription1;
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new GainMoneyAction(base.Value1, SpecialSourceType.None);
			yield break;
		}
	}
}
