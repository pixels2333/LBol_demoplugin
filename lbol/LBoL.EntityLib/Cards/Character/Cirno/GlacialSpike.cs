using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Cirno;
namespace LBoL.EntityLib.Cards.Character.Cirno
{
	[UsedImplicitly]
	public sealed class GlacialSpike : Card
	{
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<DamageEventArgs>(base.Battle.Player.DamageDealt, new GameEventHandler<DamageEventArgs>(this.OnPlayerDamageDealt), (GameEventPriority)0);
			this._coldingEnemies = new List<Unit>();
		}
		private void OnPlayerDamageDealt(DamageEventArgs args)
		{
			if (args.ActionSource == this && !args.DamageInfo.IsGrazed)
			{
				Unit target = args.Target;
				this._coldingEnemies.Add(target);
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (this._coldingEnemies.Count > 0)
			{
				foreach (Unit unit in this._coldingEnemies)
				{
					if (unit.IsAlive)
					{
						yield return base.DebuffAction<Cold>(unit, 0, 0, 0, 0, true, 0.03f);
					}
				}
				List<Unit>.Enumerator enumerator = default(List<Unit>.Enumerator);
				this._coldingEnemies.Clear();
			}
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<IceBolt>() });
			yield break;
			yield break;
		}
		private List<Unit> _coldingEnemies;
	}
}
