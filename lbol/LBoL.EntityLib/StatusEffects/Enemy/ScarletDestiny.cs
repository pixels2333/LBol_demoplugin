using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Enemy
{
	[UsedImplicitly]
	public sealed class ScarletDestiny : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.Count = base.Limit;
			base.HandleOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, delegate(UnitEventArgs _)
			{
				base.Count = base.Limit;
			});
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Battle.Player.IsInTurn)
			{
				int num = base.Count - 1;
				base.Count = num;
				yield return PerformAction.Sfx("雷米线", 0f);
				if (base.Count <= 0)
				{
					base.NotifyActivating();
					EnemyUnit remi = (EnemyUnit)base.Owner;
					yield return new EnemyMoveAction(remi, remi.GetMove(1), false);
					yield return PerformAction.Effect(remi, "ReimiCharge", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
					yield return PerformAction.Sfx("雷米结束回合", 0f);
					yield return PerformAction.Animation(remi, "defend", 2f, null, 0f, -1);
					yield return new RequestEndPlayerTurnAction();
					base.Count = base.Limit;
					remi = null;
				}
			}
			yield break;
		}
		public override string UnitEffectName
		{
			get
			{
				return "ReimiChain";
			}
		}
	}
}
