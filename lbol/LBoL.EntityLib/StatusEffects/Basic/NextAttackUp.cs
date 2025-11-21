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
	// Token: 0x020000F0 RID: 240
	[UsedImplicitly]
	public sealed class NextAttackUp : StatusEffect
	{
		// Token: 0x0600035C RID: 860 RVA: 0x00008CBD File Offset: 0x00006EBD
		protected override void OnAdded(Unit unit)
		{
			this._activated = false;
			base.HandleOwnerEvent<DamageDealingEventArgs>(unit.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnDamageDealing));
			base.ReactOwnerEvent<StatisticalDamageEventArgs>(unit.StatisticalTotalDamageDealt, new EventSequencedReactor<StatisticalDamageEventArgs>(this.OnStatisticalTotalDamageDealt));
		}

		// Token: 0x0600035D RID: 861 RVA: 0x00008CF8 File Offset: 0x00006EF8
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

		// Token: 0x0600035E RID: 862 RVA: 0x00008D63 File Offset: 0x00006F63
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

		// Token: 0x04000030 RID: 48
		private bool _activated;
	}
}
