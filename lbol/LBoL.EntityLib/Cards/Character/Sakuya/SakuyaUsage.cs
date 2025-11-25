using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	[UsedImplicitly]
	public sealed class SakuyaUsage : Card
	{
		protected override string GetBaseDescription()
		{
			if (base.PlayCount <= 0)
			{
				return base.GetBaseDescription();
			}
			return base.GetExtraDescription1;
		}
		public override bool Triggered
		{
			get
			{
				return base.PlayCount + 1 >= base.Value2;
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new DrawManyCardAction(base.Value1);
			if (base.TriggeredAnyhow)
			{
				this.RemoveFromBattleAfterPlay = true;
				yield return new AddCardsToDrawZoneAction(Library.CreateCards<SakuyaUsage2>(1, false), DrawZoneTarget.Top, AddCardsType.Normal);
			}
			yield break;
		}
	}
}
