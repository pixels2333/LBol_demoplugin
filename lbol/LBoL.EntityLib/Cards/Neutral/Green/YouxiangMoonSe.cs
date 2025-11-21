using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Cards.Neutral.Green
{
	// Token: 0x02000303 RID: 771
	public sealed class YouxiangMoonSe : StatusEffect
	{
		// Token: 0x1700014A RID: 330
		// (get) Token: 0x06000B77 RID: 2935 RVA: 0x00017042 File Offset: 0x00015242
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Single(ManaColor.Green);
			}
		}

		// Token: 0x06000B78 RID: 2936 RVA: 0x0001704A File Offset: 0x0001524A
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted));
		}

		// Token: 0x06000B79 RID: 2937 RVA: 0x0001706E File Offset: 0x0001526E
		private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
		{
			if (!base.Battle.BattleShouldEnd)
			{
				base.NotifyActivating();
				yield return new GainManaAction(ManaGroup.Greens(base.Level));
			}
			yield break;
		}
	}
}
