using System;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Basic;
namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	[UsedImplicitly]
	public sealed class VampireHunter : Card
	{
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<DamageDealingEventArgs>(base.Battle.Player.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnPlayerDamageDealing), (GameEventPriority)0);
		}
		private void OnPlayerDamageDealing(DamageDealingEventArgs args)
		{
			if (args.ActionSource == this && args.Targets != null)
			{
				if (Enumerable.Any<Unit>(args.Targets, (Unit target) => target.HasStatusEffect<Vampire>()))
				{
					args.DamageInfo = args.DamageInfo.MultiplyBy(base.Value1);
				}
			}
		}
	}
}
