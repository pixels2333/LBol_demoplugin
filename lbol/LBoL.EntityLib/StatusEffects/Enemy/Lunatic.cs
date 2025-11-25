using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Normal;
namespace LBoL.EntityLib.StatusEffects.Enemy
{
	public sealed class Lunatic : StatusEffect
	{
		[UsedImplicitly]
		public ManaColor Color { get; set; } = ManaColor.Colorless;
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Single(this.Color);
			}
		}
		protected override void OnAdded(Unit unit)
		{
			ManaColor manaColor;
			if (!(unit is BlackFairy))
			{
				if (!(unit is WhiteFairy))
				{
					manaColor = ManaColor.Colorless;
				}
				else
				{
					manaColor = ManaColor.White;
				}
			}
			else
			{
				manaColor = ManaColor.Black;
			}
			this.Color = manaColor;
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			yield return new GainManaAction(this.Mana);
			yield break;
		}
	}
}
