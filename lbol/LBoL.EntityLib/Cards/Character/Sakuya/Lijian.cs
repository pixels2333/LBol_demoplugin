using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using UnityEngine;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x02000397 RID: 919
	[UsedImplicitly]
	public sealed class Lijian : Card
	{
		// Token: 0x17000176 RID: 374
		// (get) Token: 0x06000D14 RID: 3348 RVA: 0x00018F15 File Offset: 0x00017115
		public override bool OnDrawVisual
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000177 RID: 375
		// (get) Token: 0x06000D15 RID: 3349 RVA: 0x00018F18 File Offset: 0x00017118
		public override bool OnMoveVisual
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000D16 RID: 3350 RVA: 0x00018F1B File Offset: 0x0001711B
		public override IEnumerable<BattleAction> OnDraw()
		{
			return this.EnterHandReactor();
		}

		// Token: 0x06000D17 RID: 3351 RVA: 0x00018F23 File Offset: 0x00017123
		public override IEnumerable<BattleAction> OnMove(CardZone srcZone, CardZone dstZone)
		{
			if (dstZone != CardZone.Hand)
			{
				return null;
			}
			return this.EnterHandReactor();
		}

		// Token: 0x06000D18 RID: 3352 RVA: 0x00018F31 File Offset: 0x00017131
		protected override void OnEnterBattle(BattleController battle)
		{
			if (base.Zone == CardZone.Hand)
			{
				base.React(this.EnterHandReactor());
			}
		}

		// Token: 0x06000D19 RID: 3353 RVA: 0x00018F48 File Offset: 0x00017148
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

		// Token: 0x06000D1A RID: 3354 RVA: 0x00018F58 File Offset: 0x00017158
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			EnemyUnit enemy = selector.GetEnemy(base.Battle);
			yield return new DamageAction(base.Battle.Player, enemy, this.Damage, base.GunName, GunType.Single);
			yield return new DrawManyCardAction(base.Value1);
			yield break;
		}
	}
}
