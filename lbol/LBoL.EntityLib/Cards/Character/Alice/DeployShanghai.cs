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
	public sealed class DeployShanghai : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new AddDollAction(Library.CreateDoll<Shanghai>());
			yield break;
		}
	}
}
