using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Koishi;
using UnityEngine;

namespace LBoL.EntityLib.StatusEffects.Koishi
{
	// Token: 0x02000077 RID: 119
	[UsedImplicitly]
	public sealed class KoishiDnaSe : StatusEffect
	{
		// Token: 0x17000022 RID: 34
		// (get) Token: 0x0600019C RID: 412 RVA: 0x0000534D File Offset: 0x0000354D
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Anys(1);
			}
		}

		// Token: 0x0600019D RID: 413 RVA: 0x00005355 File Offset: 0x00003555
		protected override void OnAdded(Unit unit)
		{
			if (!(unit is PlayerUnit))
			{
				Debug.LogError(this.DebugName + " should not apply to non-player unit.");
			}
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}

		// Token: 0x0600019E RID: 414 RVA: 0x00005391 File Offset: 0x00003591
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return this.TakeEffect();
			yield break;
		}

		// Token: 0x0600019F RID: 415 RVA: 0x000053A1 File Offset: 0x000035A1
		public BattleAction TakeEffect()
		{
			base.NotifyActivating();
			return new AddCardsToHandAction(Library.CreateCards<KoishiDnaAttack>(base.Level, false), AddCardsType.Normal);
		}
	}
}
