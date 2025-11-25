using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Basic
{
	[UsedImplicitly]
	public sealed class NextAttackUp : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			this._activated = false;
			base.HandleOwnerEvent<DamageDealingEventArgs>(unit.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnDamageDealing));
			base.ReactOwnerEvent<StatisticalDamageEventArgs>(unit.StatisticalTotalDamageDealt, new EventSequencedReactor<StatisticalDamageEventArgs>(this.OnStatisticalTotalDamageDealt));
		}
		private void OnDamageDealing(DamageDealingEventArgs args)
		{
			if (args.DamageInfo.DamageType == DamageType.Attack && args.ActionSource is Card)
			{
				args.DamageInfo = args.DamageInfo.IncreaseBy(base.Level);
				args.AddModifier(this);
				if (!this._activated && args.Cause != ActionCause.OnlyCalculate)
				{
					base.NotifyActivating();
					this._activated = true;
				}
			}
		}
		private IEnumerable<BattleAction> OnStatisticalTotalDamageDealt(StatisticalDamageEventArgs args)
		{
			if (!this._activated)
			{
				yield break;
			}
			foreach (KeyValuePair<Unit, IReadOnlyList<DamageEventArgs>> keyValuePair in args.ArgsTable)
			{
				Unit unit;
				IReadOnlyList<DamageEventArgs> readOnlyList;
				keyValuePair.Deconstruct(ref unit, ref readOnlyList);
				if (Enumerable.Any<DamageEventArgs>(readOnlyList, delegate(DamageEventArgs damage)
				{
					Card card = damage.ActionSource as Card;
					return card != null && card.CardType == CardType.Attack;
				}))
				{
					yield return new RemoveStatusEffectAction(this, true, 0.1f);
					break;
				}
			}
			IEnumerator<KeyValuePair<Unit, IReadOnlyList<DamageEventArgs>>> enumerator = null;
			yield break;
			yield break;
		}
		private bool _activated;
	}
}
