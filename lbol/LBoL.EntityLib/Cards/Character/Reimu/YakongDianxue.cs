using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x02000404 RID: 1028
	[UsedImplicitly]
	public sealed class YakongDianxue : Card
	{
		// Token: 0x06000E3A RID: 3642 RVA: 0x0001A456 File Offset: 0x00018656
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<StatusEffectEventArgs>(base.Battle.Player.StatusEffectRemoved, new EventSequencedReactor<StatusEffectEventArgs>(this.StatusEffectRemoved));
		}

		// Token: 0x06000E3B RID: 3643 RVA: 0x0001A47A File Offset: 0x0001867A
		private IEnumerable<BattleAction> StatusEffectRemoved(StatusEffectEventArgs args)
		{
			if (args.Effect.Type == StatusEffectType.Positive && base.Zone == CardZone.Discard && base.Battle.HandIsNotFull)
			{
				yield return new MoveCardAction(this, CardZone.Hand);
			}
			yield break;
		}
	}
}
