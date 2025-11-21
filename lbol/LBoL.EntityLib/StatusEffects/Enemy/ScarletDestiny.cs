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
	// Token: 0x020000C5 RID: 197
	[UsedImplicitly]
	public sealed class ScarletDestiny : StatusEffect
	{
		// Token: 0x060002AA RID: 682 RVA: 0x00007550 File Offset: 0x00005750
		protected override void OnAdded(Unit unit)
		{
			base.Count = base.Limit;
			base.HandleOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, delegate(UnitEventArgs _)
			{
				base.Count = base.Limit;
			});
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}

		// Token: 0x060002AB RID: 683 RVA: 0x000075A8 File Offset: 0x000057A8
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

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x060002AC RID: 684 RVA: 0x000075B8 File Offset: 0x000057B8
		public override string UnitEffectName
		{
			get
			{
				return "ReimiChain";
			}
		}
	}
}
