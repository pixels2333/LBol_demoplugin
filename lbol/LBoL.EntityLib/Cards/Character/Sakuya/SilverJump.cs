using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	[UsedImplicitly]
	public sealed class SilverJump : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DebuffAction<Vulnerable>(selector.SelectedEnemy, 0, base.Value1, 0, 0, true, 0.2f);
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<Knife>(base.Value2, false), DrawZoneTarget.Random, AddCardsType.Normal);
			yield return new AddCardsToDiscardAction(Library.CreateCards<Knife>(base.Value2, false), AddCardsType.Normal);
			yield break;
		}
	}
}
