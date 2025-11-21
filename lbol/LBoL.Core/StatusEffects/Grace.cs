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
	// Token: 0x0200009B RID: 155
	[UsedImplicitly]
	public sealed class Grace : StatusEffect
	{
		// Token: 0x0600076B RID: 1899 RVA: 0x00015EC8 File Offset: 0x000140C8
		protected override void OnAdded(Unit unit)
		{
			this._active = false;
			base.HandleOwnerEvent<DamageDealingEventArgs>(unit.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnDamageDealing));
			base.HandleOwnerEvent<BlockShieldEventArgs>(unit.BlockShieldGaining, new GameEventHandler<BlockShieldEventArgs>(this.OnBlockShieldGaining));
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
			base.ReactOwnerEvent<UsUsingEventArgs>(base.Battle.UsUsed, new EventSequencedReactor<UsUsingEventArgs>(this.OnUsUsed));
		}

		// Token: 0x0600076C RID: 1900 RVA: 0x00015F48 File Offset: 0x00014148
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

		// Token: 0x0600076D RID: 1901 RVA: 0x00015FB4 File Offset: 0x000141B4
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

		// Token: 0x0600076E RID: 1902 RVA: 0x0001602F File Offset: 0x0001422F
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (this._active)
			{
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}

		// Token: 0x0600076F RID: 1903 RVA: 0x0001603F File Offset: 0x0001423F
		private IEnumerable<BattleAction> OnUsUsed(UsUsingEventArgs args)
		{
			if (this._active)
			{
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}

		// Token: 0x17000272 RID: 626
		// (get) Token: 0x06000770 RID: 1904 RVA: 0x0001604F File Offset: 0x0001424F
		public override string UnitEffectName
		{
			get
			{
				return "CallShenling";
			}
		}

		// Token: 0x04000351 RID: 849
		private bool _active;
	}
}
