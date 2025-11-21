using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000AA RID: 170
	public sealed class JunkoPurify : StatusEffect
	{
		// Token: 0x06000260 RID: 608 RVA: 0x00006E26 File Offset: 0x00005026
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}

		// Token: 0x06000261 RID: 609 RVA: 0x00006E45 File Offset: 0x00005045
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Battle.Player.IsInTurn && base.Battle.BattleMana.HasTrivial)
			{
				base.NotifyActivating();
				yield return PerformAction.Gun(base.Owner, base.Battle.Player, "Junko3C", 0f);
				yield return ConvertManaAction.Purify(base.Battle.BattleMana, base.Level);
				JunkoLily statusEffect = base.Owner.GetStatusEffect<JunkoLily>();
				if (statusEffect != null)
				{
					statusEffect.NotifyActivating();
					yield return new DamageAction(base.Owner, base.Battle.Player, DamageInfo.HpLose((float)statusEffect.Level, true), "JunkoLilyLaser", GunType.Single);
				}
			}
			yield break;
		}

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x06000262 RID: 610 RVA: 0x00006E55 File Offset: 0x00005055
		public override string UnitEffectName
		{
			get
			{
				return "JunkoPurify";
			}
		}
	}
}
