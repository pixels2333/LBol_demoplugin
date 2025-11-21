using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x0200012D RID: 301
	[UsedImplicitly]
	public sealed class HuiyeBaoxiang : ShiningExhibit
	{
		// Token: 0x06000421 RID: 1057 RVA: 0x0000B377 File Offset: 0x00009577
		protected override IEnumerator SpecialGain(PlayerUnit player)
		{
			base.OnGain(player);
			List<Exhibit> list = new List<Exhibit>();
			for (int i = 0; i < base.Value1; i++)
			{
				list.Add(base.GameRun.CurrentStage.GetEliteEnemyExhibit());
			}
			RewardInteraction rewardInteraction = new RewardInteraction(list)
			{
				CanCancel = false,
				Source = this
			};
			yield return base.GameRun.InteractionViewer.View(rewardInteraction);
			List<Card> list2 = new List<Card>();
			list2.Add(Library.CreateCard<Zhukeling>());
			List<Card> list3 = list2;
			base.GameRun.AddDeckCards(list3, false, null);
			yield break;
		}

		// Token: 0x06000422 RID: 1058 RVA: 0x0000B38D File Offset: 0x0000958D
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}

		// Token: 0x06000423 RID: 1059 RVA: 0x0000B3AC File Offset: 0x000095AC
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			base.NotifyActivating();
			ManaGroup manaGroup = ManaGroup.Single(ManaColors.Colors.Sample(base.GameRun.BattleRng));
			yield return new GainManaAction(manaGroup);
			yield break;
		}
	}
}
