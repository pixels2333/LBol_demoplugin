using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace LBoL.EntityLib.Cards.Character.Cirno.Friend
{
	// Token: 0x020004DA RID: 1242
	[UsedImplicitly]
	public sealed class LeidiFriend : Card
	{
		// Token: 0x06001072 RID: 4210 RVA: 0x0001CFBB File Offset: 0x0001B1BB
		public override IEnumerable<BattleAction> OnTurnEndingInHand()
		{
			return this.GetPassiveActions();
		}

		// Token: 0x06001073 RID: 4211 RVA: 0x0001CFC3 File Offset: 0x0001B1C3
		public override IEnumerable<BattleAction> GetPassiveActions()
		{
			if (!base.Summoned || base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			base.Loyalty += base.PassiveCost;
			int num;
			for (int i = 0; i < base.Battle.FriendPassiveTimes; i = num + 1)
			{
				if (base.Battle.BattleShouldEnd)
				{
					yield break;
				}
				yield return base.DebuffAction<Cold>(base.Battle.RandomAliveEnemy, 0, 0, 0, 0, true, 0.1f);
				num = i;
			}
			if (base.Loyalty <= 0)
			{
				yield return new RemoveCardAction(this);
			}
			yield break;
		}

		// Token: 0x06001074 RID: 4212 RVA: 0x0001CFD3 File Offset: 0x0001B1D3
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			base.Loyalty += base.UltimateCost;
			base.UltimateUsed = true;
			yield return base.AttackAction(base.Battle.AllAliveEnemies);
			foreach (BattleAction battleAction in base.DebuffAction<Cold>(base.Battle.AllAliveEnemies, 0, 0, 0, 0, true, 0.03f))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield break;
			yield break;
		}
	}
}
