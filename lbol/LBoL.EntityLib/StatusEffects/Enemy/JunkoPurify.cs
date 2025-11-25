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
	public sealed class JunkoPurify : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}
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
		public override string UnitEffectName
		{
			get
			{
				return "JunkoPurify";
			}
		}
	}
}
