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
	[UsedImplicitly]
	public sealed class HuiyeBaoxiang : ShiningExhibit
	{
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
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			base.NotifyActivating();
			ManaGroup manaGroup = ManaGroup.Single(ManaColors.Colors.Sample(base.GameRun.BattleRng));
			yield return new GainManaAction(manaGroup);
			yield break;
		}
	}
}
