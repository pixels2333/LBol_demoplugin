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
	[UsedImplicitly]
	public sealed class ReimuSilenceSe : StatusEffect
	{
		[UsedImplicitly]
		public DamageInfo Damage
		{
			get
			{
				return DamageInfo.Attack((float)base.Level, false);
			}
		}
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
