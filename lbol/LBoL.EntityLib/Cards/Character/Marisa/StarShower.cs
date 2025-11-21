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
	// Token: 0x0200044C RID: 1100
	[UsedImplicitly]
	public sealed class StarShower : Card
	{
		// Token: 0x170001A4 RID: 420
		// (get) Token: 0x06000EF5 RID: 3829 RVA: 0x0001B1EF File Offset: 0x000193EF
		public override bool Triggered
		{
			get
			{
				BattleController battle = base.Battle;
				return ((battle != null) ? battle.Player : null) != null && !base.Battle.Player.HasStatusEffect<Charging>();
			}
		}

		// Token: 0x06000EF6 RID: 3830 RVA: 0x0001B21A File Offset: 0x0001941A
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return base.BuffAction<Charging>(base.TriggeredAnyhow ? base.Value2 : base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
