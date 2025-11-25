using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.Black
{
	[UsedImplicitly]
	public sealed class YukariAttack : Card
	{
		private bool TurnEndReturn { get; set; }
		protected override int AdditionalDamage
		{
			get
			{
				return base.GrowCount * base.Value2;
			}
		}
		protected override void SetGuns()
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
		}
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<DamageEventArgs>(base.Battle.Player.DamageTaking, new GameEventHandler<DamageEventArgs>(this.OnPlayerDamageTaking), (GameEventPriority)0);
			base.HandleBattleEvent<HealEventArgs>(base.Battle.Player.HealingReceived, new GameEventHandler<HealEventArgs>(this.OnPlayerHealingReceived), (GameEventPriority)0);
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnded));
		}
		private void OnPlayerDamageTaking(DamageEventArgs args)
		{
			if (base.Battle.Player.IsInTurn && args.DamageInfo.Damage > 0f && !base.Battle.BattleShouldEnd)
			{
				this.TurnEndReturn = true;
				if (base.Zone == CardZone.Hand)
				{
					base.NotifyActivating();
				}
			}
		}
		private void OnPlayerHealingReceived(HealEventArgs args)
		{
			if (base.Battle.Player.IsInTurn && !base.Battle.BattleShouldEnd)
			{
				this.TurnEndReturn = true;
				if (base.Zone == CardZone.Hand)
				{
					base.NotifyActivating();
				}
			}
		}
		private IEnumerable<BattleAction> OnPlayerTurnEnded(UnitEventArgs args)
		{
			if (this.TurnEndReturn && base.Zone == CardZone.Discard)
			{
				yield return new MoveCardAction(this, CardZone.Hand);
				base.SetTurnCost(base.Mana);
			}
			this.TurnEndReturn = false;
			yield break;
		}
	}
}
