using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x020003AB RID: 939
	[UsedImplicitly]
	public sealed class SakuyaGraze : Card
	{
		// Token: 0x06000D57 RID: 3415 RVA: 0x000193AB File Offset: 0x000175AB
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return base.BuffAction<Graze>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}

		// Token: 0x06000D58 RID: 3416 RVA: 0x000193BB File Offset: 0x000175BB
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.Reshuffling, new EventSequencedReactor<GameEventArgs>(this.Reshuffling));
		}

		// Token: 0x06000D59 RID: 3417 RVA: 0x000193DA File Offset: 0x000175DA
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
