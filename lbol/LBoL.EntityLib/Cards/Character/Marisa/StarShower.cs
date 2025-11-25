using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.Cards.Character.Marisa
{
	[UsedImplicitly]
	public sealed class StarShower : Card
	{
		public override bool Triggered
		{
			get
			{
				BattleController battle = base.Battle;
				return ((battle != null) ? battle.Player : null) != null && !base.Battle.Player.HasStatusEffect<Charging>();
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return base.BuffAction<Charging>(base.TriggeredAnyhow ? base.Value2 : base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
