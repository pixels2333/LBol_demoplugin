using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.Red
{
	// Token: 0x02000053 RID: 83
	public sealed class MeihongFireSe : StatusEffect
	{
		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000114 RID: 276 RVA: 0x0000405C File Offset: 0x0000225C
		private string GunName
		{
			get
			{
				if (base.Level <= 10)
				{
					return "无差别起火";
				}
				return "无差别起火B";
			}
		}

		// Token: 0x06000115 RID: 277 RVA: 0x00004074 File Offset: 0x00002274
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding));
			base.ReactOwnerEvent<CardEventArgs>(base.Battle.CardDrawn, new EventSequencedReactor<CardEventArgs>(this.OnCardDrawn));
		}

		// Token: 0x06000116 RID: 278 RVA: 0x000040C0 File Offset: 0x000022C0
		private IEnumerable<BattleAction> OnPlayerTurnEnding(GameEventArgs args)
		{
			if (!base.Battle.BattleShouldEnd && base.Battle.EnemyGroup.Alives != null)
			{
				base.NotifyActivating();
				yield return new DamageAction(base.Owner, base.Battle.EnemyGroup.Alives, DamageInfo.Reaction((float)base.Level, false), this.GunName, GunType.Single);
			}
			yield break;
		}

		// Token: 0x06000117 RID: 279 RVA: 0x000040D0 File Offset: 0x000022D0
		private IEnumerable<BattleAction> OnCardDrawn(CardEventArgs args)
		{
			if (!base.Battle.BattleShouldEnd && base.Battle.EnemyGroup.Alives != null)
			{
				CardType cardType = args.Card.CardType;
				if (cardType == CardType.Misfortune || cardType == CardType.Status)
				{
					base.NotifyActivating();
					yield return new DamageAction(base.Battle.Player, base.Battle.AllAliveEnemies, DamageInfo.Reaction((float)base.Level, false), this.GunName, GunType.Single);
				}
			}
			yield break;
		}
	}
}
