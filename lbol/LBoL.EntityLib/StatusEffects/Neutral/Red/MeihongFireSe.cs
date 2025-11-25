using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Neutral.Red
{
	public sealed class MeihongFireSe : StatusEffect
	{
		private string GunName
		{
			get
			{
				if (base.Level <= 10)
				{
					return "无差别起火";
				}
				return "无差别起火B";
			}
		}
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding));
			base.ReactOwnerEvent<CardEventArgs>(base.Battle.CardDrawn, new EventSequencedReactor<CardEventArgs>(this.OnCardDrawn));
		}
		private IEnumerable<BattleAction> OnPlayerTurnEnding(GameEventArgs args)
		{
			if (!base.Battle.BattleShouldEnd && base.Battle.EnemyGroup.Alives != null)
			{
				base.NotifyActivating();
				yield return new DamageAction(base.Owner, base.Battle.EnemyGroup.Alives, DamageInfo.Reaction((float)base.Level, false), this.GunName, GunType.Single);
			}
			yield break;
		}
		private IEnumerable<BattleAction> OnCardDrawn(CardEventArgs args)
		{
			if (!base.Battle.BattleShouldEnd && base.Battle.EnemyGroup.Alives != null)
			{
				CardType cardType = args.Card.CardType;
				if (cardType == CardType.Misfortune || cardType == CardType.Status)
				{
					base.NotifyActivating();
					yield return new DamageAction(base.Battle.Player, base.Battle.AllAliveEnemies, DamageInfo.Reaction((float)base.Level, false), this.GunName, GunType.Single);
				}
			}
			yield break;
		}
	}
}
