using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Cirno;
namespace LBoL.EntityLib.Cards.Character.Cirno
{
	[UsedImplicitly]
	public sealed class FruitShake : Card
	{
		public override bool Triggered
		{
			get
			{
				return this.IsForceCost;
			}
		}
		public override bool IsForceCost
		{
			get
			{
				BattleController battle = base.Battle;
				return battle != null && battle.PlayerSummonAFriendThisTurn;
			}
		}
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new GameEventHandler<UnitEventArgs>(this.OnPlayerTurnStarted), (GameEventPriority)0);
			base.HandleBattleEvent<DamageEventArgs>(base.Battle.Player.DamageDealt, new GameEventHandler<DamageEventArgs>(this.OnPlayerDamageDealt), (GameEventPriority)0);
			this._coldingEnemies = new List<Unit>();
		}
		private void OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (base.Zone == CardZone.Hand)
			{
				this.NotifyChanged();
			}
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
			if (base.Battle.BattleShouldEnd || this._coldingEnemies.Count == 0)
			{
				yield break;
			}
			foreach (Unit unit in this._coldingEnemies)
			{
				if (unit.IsAlive)
				{
					yield return base.DebuffAction<Cold>(unit, 0, 0, 0, 0, true, 0.03f);
				}
			}
			List<Unit>.Enumerator enumerator = default(List<Unit>.Enumerator);
			this._coldingEnemies.Clear();
			yield break;
			yield break;
		}
		private List<Unit> _coldingEnemies;
	}
}
