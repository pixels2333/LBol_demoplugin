using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Marisa
{
	[UsedImplicitly]
	public sealed class DarkSatellite : Card
	{
		public override bool Triggered
		{
			get
			{
				return base.Battle != null && base.Battle.BattleCardUsageHistory.Count != 0 && Enumerable.Contains<ManaColor>(Enumerable.Last<Card>(base.Battle.BattleCardUsageHistory).Config.Colors, ManaColor.Black);
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			if (base.TriggeredAnyhow)
			{
				yield return new DrawManyCardAction(base.Value1);
			}
			yield break;
		}
	}
}
