using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x020003C2 RID: 962
	[UsedImplicitly]
	public sealed class TunnelEffect : Card
	{
		// Token: 0x06000D90 RID: 3472 RVA: 0x0001971F File Offset: 0x0001791F
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			yield return new GainManaAction(base.Mana);
			yield break;
		}

		// Token: 0x06000D91 RID: 3473 RVA: 0x00019736 File Offset: 0x00017936
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.Reshuffling, new EventSequencedReactor<GameEventArgs>(this.Reshuffling));
		}

		// Token: 0x06000D92 RID: 3474 RVA: 0x00019755 File Offset: 0x00017955
		private IEnumerable<BattleAction> Reshuffling(GameEventArgs args)
		{
			if (base.Zone == CardZone.Discard)
			{
				yield return new MoveCardAction(this, CardZone.Hand);
			}
			yield break;
		}
	}
}
