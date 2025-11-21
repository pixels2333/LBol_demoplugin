using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.EntityLib.Cards.Character.Sakuya;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x0200013C RID: 316
	[UsedImplicitly]
	public sealed class SakuyaW : ShiningExhibit
	{
		// Token: 0x06000457 RID: 1111 RVA: 0x0000B9A1 File Offset: 0x00009BA1
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
			base.ReactBattleEvent<GameEventArgs>(base.Battle.Reshuffled, new EventSequencedReactor<GameEventArgs>(this.OnReshuffled));
		}

		// Token: 0x06000458 RID: 1112 RVA: 0x0000B9DD File Offset: 0x00009BDD
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			base.NotifyActivating();
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<Knife>(base.Value1, false), DrawZoneTarget.Random, AddCardsType.Normal);
			yield break;
		}

		// Token: 0x06000459 RID: 1113 RVA: 0x0000B9ED File Offset: 0x00009BED
		private IEnumerable<BattleAction> OnReshuffled(GameEventArgs args)
		{
			base.NotifyActivating();
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<Knife>(base.Value1, false), DrawZoneTarget.Random, AddCardsType.Normal);
			yield break;
		}
	}
}
