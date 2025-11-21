using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Reimu;
using UnityEngine;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003C5 RID: 965
	[UsedImplicitly]
	public sealed class AutoQumobang : Card
	{
		// Token: 0x06000D99 RID: 3481 RVA: 0x00019818 File Offset: 0x00017A18
		public override IEnumerable<BattleAction> OnDraw()
		{
			return this.EnterHandReactor(true);
		}

		// Token: 0x06000D9A RID: 3482 RVA: 0x00019821 File Offset: 0x00017A21
		public override IEnumerable<BattleAction> OnMove(CardZone srcZone, CardZone dstZone)
		{
			if (dstZone != CardZone.Hand)
			{
				return null;
			}
			return this.EnterHandReactor(true);
		}

		// Token: 0x06000D9B RID: 3483 RVA: 0x00019830 File Offset: 0x00017A30
		protected override void OnEnterBattle(BattleController battle)
		{
			if (base.Zone == CardZone.Hand)
			{
				base.React(new LazySequencedReactor(this.AddToHandReactor));
			}
		}

		// Token: 0x06000D9C RID: 3484 RVA: 0x0001984D File Offset: 0x00017A4D
		private IEnumerable<BattleAction> AddToHandReactor()
		{
			base.NotifyActivating();
			foreach (BattleAction battleAction in this.EnterHandReactor(true))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x06000D9D RID: 3485 RVA: 0x0001985D File Offset: 0x00017A5D
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			return this.EnterHandReactor(false);
		}

		// Token: 0x06000D9E RID: 3486 RVA: 0x00019866 File Offset: 0x00017A66
		private IEnumerable<BattleAction> EnterHandReactor(bool ensureInHand = true)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (ensureInHand && base.Zone != CardZone.Hand)
			{
				Debug.LogWarning(this.Name + " is not in hand.");
				yield break;
			}
			yield return base.BuffAction<AutoQumobangSe>(base.Value1, 0, 0, base.Value2, 0.2f);
			yield return new ExileCardAction(this);
			yield break;
		}
	}
}
