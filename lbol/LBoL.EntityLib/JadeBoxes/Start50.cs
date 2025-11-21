using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;

namespace LBoL.EntityLib.JadeBoxes
{
	// Token: 0x02000119 RID: 281
	[UsedImplicitly]
	public sealed class Start50 : JadeBox
	{
		// Token: 0x060003E2 RID: 994 RVA: 0x0000ACD0 File Offset: 0x00008ED0
		protected override void OnGain(GameRunController gameRun)
		{
			gameRun.RemoveGamerunInitialCards();
			for (int i = 0; i < base.Value1; i++)
			{
				Card[] array = gameRun.RollCards(gameRun.CardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), 1, false, false, null);
				gameRun.AddDeckCards(array, false, null);
			}
		}

		// Token: 0x060003E3 RID: 995 RVA: 0x0000AD23 File Offset: 0x00008F23
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x060003E4 RID: 996 RVA: 0x0000AD47 File Offset: 0x00008F47
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			yield return ConvertManaAction.PhilosophyRandomMana(base.Battle.BattleMana, 1, base.GameRun.BattleRng);
			yield break;
		}
	}
}
