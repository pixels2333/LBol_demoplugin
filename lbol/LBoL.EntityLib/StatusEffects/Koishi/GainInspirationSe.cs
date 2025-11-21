using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Koishi;

namespace LBoL.EntityLib.StatusEffects.Koishi
{
	// Token: 0x02000074 RID: 116
	[UsedImplicitly]
	public sealed class GainInspirationSe : StatusEffect
	{
		// Token: 0x06000193 RID: 403 RVA: 0x00005229 File Offset: 0x00003429
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding));
		}

		// Token: 0x06000194 RID: 404 RVA: 0x0000524D File Offset: 0x0000344D
		private IEnumerable<BattleAction> OnPlayerTurnEnding(GameEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<InspirationCard>(base.Level, false), DrawZoneTarget.Random, AddCardsType.Normal);
			yield break;
		}
	}
}
