using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using UnityEngine;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x0200039A RID: 922
	[UsedImplicitly]
	public sealed class LunaDial : Card
	{
		// Token: 0x17000178 RID: 376
		// (get) Token: 0x06000D21 RID: 3361 RVA: 0x00018FCB File Offset: 0x000171CB
		public override bool OnDrawVisual
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000179 RID: 377
		// (get) Token: 0x06000D22 RID: 3362 RVA: 0x00018FCE File Offset: 0x000171CE
		public override bool OnMoveVisual
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000D23 RID: 3363 RVA: 0x00018FD1 File Offset: 0x000171D1
		public override IEnumerable<BattleAction> OnDraw()
		{
			return this.EnterHandReactor();
		}

		// Token: 0x06000D24 RID: 3364 RVA: 0x00018FD9 File Offset: 0x000171D9
		public override IEnumerable<BattleAction> OnMove(CardZone srcZone, CardZone dstZone)
		{
			if (dstZone != CardZone.Hand)
			{
				return null;
			}
			return this.EnterHandReactor();
		}

		// Token: 0x06000D25 RID: 3365 RVA: 0x00018FE7 File Offset: 0x000171E7
		protected override void OnEnterBattle(BattleController battle)
		{
			if (base.Zone == CardZone.Hand)
			{
				base.React(this.EnterHandReactor());
			}
		}

		// Token: 0x06000D26 RID: 3366 RVA: 0x00018FFE File Offset: 0x000171FE
		private IEnumerable<BattleAction> EnterHandReactor()
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Zone != CardZone.Hand)
			{
				Debug.LogWarning(this.Name + " is not in hand.");
				yield break;
			}
			base.DecreaseBaseCost(base.Mana);
			yield break;
		}

		// Token: 0x06000D27 RID: 3367 RVA: 0x0001900E File Offset: 0x0001720E
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return PerformAction.Effect(base.Battle.Player, "ExtraTime", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
			yield return PerformAction.Sfx("ExtraTurnLaunch", 0f);
			yield return PerformAction.Animation(base.Battle.Player, "spell", 1.6f, null, 0f, -1);
			yield return base.BuffAction<ExtraTurn>(1, 0, 0, 0, 0.2f);
			yield return new RequestEndPlayerTurnAction();
			yield break;
		}
	}
}
