using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Koishi;
namespace LBoL.EntityLib.Cards.Character.Koishi
{
	[UsedImplicitly]
	public sealed class TheHermit : Card
	{
		public override bool Triggered
		{
			get
			{
				return this.IsForceCost;
			}
		}
		public override bool IsForceCost
		{
			get
			{
				return base.Battle != null && base.Battle.Player.HasStatusEffect<MoodEpiphany>();
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ScryAction(base.Scry);
			if (this.IsUpgraded)
			{
				Card card = Enumerable.FirstOrDefault<Card>(base.Battle.DrawZone);
				if (card != null && card.CanUpgradeAndPositive)
				{
					yield return new UpgradeCardAction(card);
				}
			}
			yield return new GainManaAction(base.Mana);
			yield break;
		}
	}
}
