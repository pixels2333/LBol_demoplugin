using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using UnityEngine;

namespace LBoL.EntityLib.StatusEffects.Reimu
{
	// Token: 0x02000034 RID: 52
	[UsedImplicitly]
	public sealed class ReimuSilenceSe : StatusEffect
	{
		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000098 RID: 152 RVA: 0x000030C6 File Offset: 0x000012C6
		[UsedImplicitly]
		public DamageInfo Damage
		{
			get
			{
				return DamageInfo.Attack((float)base.Level, false);
			}
		}

		// Token: 0x06000099 RID: 153 RVA: 0x000030D8 File Offset: 0x000012D8
		protected override void OnAdded(Unit unit)
		{
			if (base.Limit <= 0)
			{
				Debug.LogError(this.DebugName + "'s limit not provide, set to 3 (default)");
				base.Limit = 3;
			}
			base.Count = base.Limit;
			base.HandleOwnerEvent<CardEventArgs>(base.Battle.CardDrawn, new GameEventHandler<CardEventArgs>(this.OnCardDrawn));
		}

		// Token: 0x0600009A RID: 154 RVA: 0x00003134 File Offset: 0x00001334
		private void OnCardDrawn(CardEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				return;
			}
			if (args.Cause == ActionCause.TurnStart)
			{
				return;
			}
			int num = base.Count - 1;
			base.Count = num;
			if (base.Count > 0)
			{
				return;
			}
			DamageAction damageAction = new DamageAction(base.Owner, base.Battle.AllAliveEnemies, DamageInfo.Attack((float)base.Level, false), "梦想封印寂", GunType.Single);
			this.React(damageAction);
			this.React(new StatisticalTotalDamageAction(new DamageAction[] { damageAction }));
			base.Count = base.Limit;
		}
	}
}
