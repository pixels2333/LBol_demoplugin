using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Dolls;
namespace LBoL.EntityLib.Cards.Character.Alice
{
	[UsedImplicitly]
	public sealed class DeployPenglai : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new AddDollAction(Library.CreateDoll<Penglai>());
			yield break;
		}
		protected override IEnumerable<BattleAction> KickerActions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new AddDollAction(Library.CreateDoll<Penglai>());
			yield return new AddDollAction(Library.CreateDoll<Penglai>());
			yield break;
		}
	}
}
