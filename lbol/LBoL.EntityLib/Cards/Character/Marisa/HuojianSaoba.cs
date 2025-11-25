using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.Cards.Character.Marisa
{
	[UsedImplicitly]
	public sealed class HuojianSaoba : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ApplyStatusEffectAction<Graze>(base.Battle.Player, new int?(base.Value1), default(int?), default(int?), default(int?), 0.4f, true);
			yield return new ApplyStatusEffectAction<Charging>(base.Battle.Player, new int?(base.Value2), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
	}
}
