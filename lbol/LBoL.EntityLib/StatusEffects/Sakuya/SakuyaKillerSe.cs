using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Sakuya;

namespace LBoL.EntityLib.StatusEffects.Sakuya
{
	// Token: 0x0200001F RID: 31
	[UsedImplicitly]
	public sealed class SakuyaKillerSe : StatusEffect
	{
		// Token: 0x06000046 RID: 70 RVA: 0x00002714 File Offset: 0x00000914
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding));
			base.ReactOwnerEvent<DieEventArgs>(base.Battle.EnemyDied, new EventSequencedReactor<DieEventArgs>(this.OnEnemyDied));
		}

		// Token: 0x06000047 RID: 71 RVA: 0x00002760 File Offset: 0x00000960
		private IEnumerable<BattleAction> OnPlayerTurnEnding(GameEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return this.ActiveAction();
			yield break;
		}

		// Token: 0x06000048 RID: 72 RVA: 0x00002770 File Offset: 0x00000970
		private IEnumerable<BattleAction> OnEnemyDied(DieEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return this.ActiveAction();
			yield break;
		}

		// Token: 0x06000049 RID: 73 RVA: 0x00002780 File Offset: 0x00000980
		private BattleAction ActiveAction()
		{
			base.NotifyActivating();
			return new AddCardsToDrawZoneAction(Library.CreateCards<Knife>(base.Level, false), DrawZoneTarget.Random, AddCardsType.Normal);
		}
	}
}
