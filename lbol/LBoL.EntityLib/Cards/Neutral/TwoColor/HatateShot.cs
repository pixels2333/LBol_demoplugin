using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	[UsedImplicitly]
	public sealed class HatateShot : Card
	{
		[UsedImplicitly]
		public int Special
		{
			get
			{
				if (base.Battle == null)
				{
					return 0;
				}
				if (this.Playing)
				{
					return base.Value1 * base.Battle.BattleMana.Amount;
				}
				return base.Value1 * Math.Max(base.Battle.BattleMana.Amount - base.Cost.Amount, 0);
			}
		}
		protected override int AdditionalBlock
		{
			get
			{
				return this.Special;
			}
		}
		private bool Playing { get; set; }
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			this.Playing = true;
			yield return base.DefenseAction(true);
			this.Playing = false;
			yield break;
		}
	}
}
