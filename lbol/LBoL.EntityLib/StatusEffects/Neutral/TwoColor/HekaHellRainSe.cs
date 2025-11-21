using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.TwoColor
{
	// Token: 0x02000041 RID: 65
	public sealed class HekaHellRainSe : StatusEffect
	{
		// Token: 0x060000C7 RID: 199 RVA: 0x00003678 File Offset: 0x00001878
		protected override void OnAdded(Unit unit)
		{
			this.SetCount();
			base.HandleOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, delegate(CardUsingEventArgs _)
			{
				this.SetCount();
			});
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding));
		}

		// Token: 0x060000C8 RID: 200 RVA: 0x000036CA File Offset: 0x000018CA
		private void SetCount()
		{
			base.Count = base.Battle.TurnCardUsageHistory.Count * base.Level;
		}

		// Token: 0x060000C9 RID: 201 RVA: 0x000036E9 File Offset: 0x000018E9
		private IEnumerable<BattleAction> OnPlayerTurnEnding(UnitEventArgs args)
		{
			if (base.Count > 0)
			{
				base.NotifyActivating();
				string text = "地狱雨";
				if (base.Count > 10)
				{
					text = "地狱雨B";
				}
				if (base.Count > 20)
				{
					text = "地狱雨C";
				}
				yield return new DamageAction(base.Battle.Player, base.Battle.EnemyGroup.Alives, DamageInfo.Reaction((float)base.Count, false), text, GunType.Single);
				base.Count = 0;
			}
			yield break;
		}
	}
}
