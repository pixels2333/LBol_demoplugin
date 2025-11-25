using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Cards.Neutral.Green
{
	public sealed class YouxiangMoonSe : StatusEffect
	{
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Single(ManaColor.Green);
			}
		}
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted));
		}
		private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
		{
			if (!base.Battle.BattleShouldEnd)
			{
				base.NotifyActivating();
				yield return new GainManaAction(ManaGroup.Greens(base.Level));
			}
			yield break;
		}
	}
}
