using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Adventure
{
	// Token: 0x020004FD RID: 1277
	[UsedImplicitly]
	public sealed class RabbitFoot : Card
	{
		// Token: 0x060010C7 RID: 4295 RVA: 0x0001D3D2 File Offset: 0x0001B5D2
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleEnding, new EventSequencedReactor<GameEventArgs>(this.OnBattleEnding));
		}

		// Token: 0x060010C8 RID: 4296 RVA: 0x0001D3F1 File Offset: 0x0001B5F1
		private IEnumerable<BattleAction> OnBattleEnding(GameEventArgs args)
		{
			switch (base.Zone)
			{
			case CardZone.Draw:
				yield return new GainMoneyAction(base.Value1, SpecialSourceType.DrawZone);
				break;
			case CardZone.Hand:
				yield return new GainMoneyAction(base.Value1, SpecialSourceType.None);
				break;
			case CardZone.Discard:
				yield return new GainMoneyAction(base.Value1, SpecialSourceType.DisCardZone);
				break;
			}
			yield break;
		}
	}
}
