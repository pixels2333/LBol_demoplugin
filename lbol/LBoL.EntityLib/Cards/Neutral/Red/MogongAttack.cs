using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.Red
{
	[UsedImplicitly]
	public sealed class MogongAttack : Card
	{
		private int CopiedDamage { get; set; }
		private int CopiedBlock { get; set; }
		protected override int AdditionalDamage
		{
			get
			{
				if (base.Battle == null || this.CopiedDamage <= 1)
				{
					return 0;
				}
				return this.CopiedDamage - 1;
			}
		}
		protected override int AdditionalBlock
		{
			get
			{
				if (base.Battle == null || this.CopiedBlock <= 1)
				{
					return 0;
				}
				return this.CopiedBlock - 1;
			}
		}
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new GameEventHandler<CardUsingEventArgs>(this.OnCardUsed), (GameEventPriority)0);
		}
		private void OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Zone == CardZone.Hand)
			{
				Card card = args.Card;
				CardConfig config = card.Config;
				if (config.Damage != null && card.RawDamage > 0)
				{
					this.CopiedDamage = Math.Max(args.Card.RawDamage, this.CopiedDamage);
				}
				if (config.Block != null && card.RawBlock > 0)
				{
					this.CopiedBlock = Math.Max(args.Card.RawBlock, this.CopiedBlock);
				}
				if (config.Shield != null && card.RawShield > 0)
				{
					this.CopiedBlock = Math.Max(args.Card.RawShield, this.CopiedBlock);
				}
				this.NotifyChanged();
			}
		}
		public override void OnLeaveHand()
		{
			base.OnLeaveHand();
			this.CopiedDamage = 0;
			this.CopiedBlock = 0;
			this.NotifyChanged();
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(false);
			yield return base.AttackAction(selector, null);
			yield break;
		}
	}
}
