using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using UnityEngine;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x02000387 RID: 903
	[UsedImplicitly]
	public sealed class DefenseDraw : Card
	{
		// Token: 0x1700016B RID: 363
		// (get) Token: 0x06000CDC RID: 3292 RVA: 0x00018B10 File Offset: 0x00016D10
		public override bool OnDrawVisual
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700016C RID: 364
		// (get) Token: 0x06000CDD RID: 3293 RVA: 0x00018B13 File Offset: 0x00016D13
		public override bool OnMoveVisual
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000CDE RID: 3294 RVA: 0x00018B16 File Offset: 0x00016D16
		public override IEnumerable<BattleAction> OnDraw()
		{
			return this.EnterHandReactor();
		}

		// Token: 0x06000CDF RID: 3295 RVA: 0x00018B1E File Offset: 0x00016D1E
		public override IEnumerable<BattleAction> OnMove(CardZone srcZone, CardZone dstZone)
		{
			if (dstZone != CardZone.Hand)
			{
				return null;
			}
			return this.EnterHandReactor();
		}

		// Token: 0x06000CE0 RID: 3296 RVA: 0x00018B2C File Offset: 0x00016D2C
		protected override void OnEnterBattle(BattleController battle)
		{
			if (base.Zone == CardZone.Hand)
			{
				base.React(this.EnterHandReactor());
			}
		}

		// Token: 0x06000CE1 RID: 3297 RVA: 0x00018B43 File Offset: 0x00016D43
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

		// Token: 0x06000CE2 RID: 3298 RVA: 0x00018B53 File Offset: 0x00016D53
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return new DrawManyCardAction(base.Value1);
			yield break;
		}
	}
}
