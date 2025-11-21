using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x02000138 RID: 312
	[UsedImplicitly]
	public sealed class QipaiYouhua : ShiningExhibit
	{
		// Token: 0x06000447 RID: 1095 RVA: 0x0000B700 File Offset: 0x00009900
		protected override void OnGain(PlayerUnit player)
		{
			base.GameRun.GainMoney(base.Value1, true, new VisualSourceData
			{
				SourceType = VisualSourceType.Entity,
				Source = this
			});
			base.GameRun.UpgradeRandomCards(base.Value2, default(CardType?));
			base.GameRun.GainMaxHp(base.Value3, true, true);
		}

		// Token: 0x06000448 RID: 1096 RVA: 0x0000B75F File Offset: 0x0000995F
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}

		// Token: 0x06000449 RID: 1097 RVA: 0x0000B77E File Offset: 0x0000997E
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			base.NotifyActivating();
			ManaGroup manaGroup = ManaGroup.Single(ManaColors.Colors.Sample(base.GameRun.BattleRng));
			yield return new GainManaAction(manaGroup);
			yield break;
		}
	}
}
