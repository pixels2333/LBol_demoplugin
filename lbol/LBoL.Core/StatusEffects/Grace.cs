using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
namespace LBoL.Core.StatusEffects
{
	[UsedImplicitly]
	public sealed class Grace : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			this._active = false;
			base.HandleOwnerEvent<DamageDealingEventArgs>(unit.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnDamageDealing));
			base.HandleOwnerEvent<BlockShieldEventArgs>(unit.BlockShieldGaining, new GameEventHandler<BlockShieldEventArgs>(this.OnBlockShieldGaining));
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
			base.ReactOwnerEvent<UsUsingEventArgs>(base.Battle.UsUsed, new EventSequencedReactor<UsUsingEventArgs>(this.OnUsUsed));
		}
		private void OnDamageDealing(DamageDealingEventArgs args)
		{
			if (args.DamageInfo.DamageType == DamageType.Attack)
			{
				Card card = args.ActionSource as Card;
				if (card != null && card.CardType == CardType.Friend)
				{
					return;
				}
				args.DamageInfo = args.DamageInfo.IncreaseBy(base.Level);
				args.AddModifier(this);
				if (args.Cause != ActionCause.OnlyCalculate)
				{
					this._active = true;
				}
			}
		}
		private void OnBlockShieldGaining(BlockShieldEventArgs args)
		{
			if (args.Type == BlockShieldType.Direct)
			{
				return;
			}
			ActionCause cause = args.Cause;
			if (cause == ActionCause.Card || cause == ActionCause.OnlyCalculate || cause == ActionCause.Us)
			{
				if (args.HasBlock)
				{
					args.Block += (float)base.Level;
				}
				if (args.HasShield)
				{
					args.Shield += (float)base.Level;
				}
				args.AddModifier(this);
				if (args.Cause != ActionCause.OnlyCalculate)
				{
					this._active = true;
				}
			}
		}
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (this._active)
			{
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}
		private IEnumerable<BattleAction> OnUsUsed(UsUsingEventArgs args)
		{
			if (this._active)
			{
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}
		public override string UnitEffectName
		{
			get
			{
				return "CallShenling";
			}
		}
		private bool _active;
	}
}
