using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Koishi;
using UnityEngine;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000466 RID: 1126
	[UsedImplicitly]
	public sealed class InspirationCard : Card
	{
		// Token: 0x06000F30 RID: 3888 RVA: 0x0001B579 File Offset: 0x00019779
		public override IEnumerable<BattleAction> OnDraw()
		{
			return this.EnterHandReactor(true);
		}

		// Token: 0x06000F31 RID: 3889 RVA: 0x0001B582 File Offset: 0x00019782
		public override IEnumerable<BattleAction> OnMove(CardZone srcZone, CardZone dstZone)
		{
			if (dstZone != CardZone.Hand)
			{
				return null;
			}
			return this.EnterHandReactor(true);
		}

		// Token: 0x06000F32 RID: 3890 RVA: 0x0001B591 File Offset: 0x00019791
		protected override void OnEnterBattle(BattleController battle)
		{
			if (base.Zone == CardZone.Hand)
			{
				base.React(new LazySequencedReactor(this.AddToHandReactor));
			}
		}

		// Token: 0x06000F33 RID: 3891 RVA: 0x0001B5AE File Offset: 0x000197AE
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

		// Token: 0x06000F34 RID: 3892 RVA: 0x0001B5BE File Offset: 0x000197BE
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			return this.EnterHandReactor(false);
		}

		// Token: 0x06000F35 RID: 3893 RVA: 0x0001B5C7 File Offset: 0x000197C7
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
			yield return new GainManaAction(base.Mana);
			yield return base.BuffAction<Inspiration>(base.Value1, 0, 0, 0, 0.2f);
			yield return new ExileCardAction(this);
			yield break;
		}
	}
}
