using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.MultiColor
{
	// Token: 0x02000056 RID: 86
	[UsedImplicitly]
	public sealed class JinziDoppelgangerSe : StatusEffect
	{
		// Token: 0x06000123 RID: 291 RVA: 0x00004248 File Offset: 0x00002448
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, delegate(UnitEventArgs _)
			{
				base.Count = base.Level;
				base.Highlight = true;
			});
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsing, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsing));
			base.HandleOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new GameEventHandler<CardUsingEventArgs>(this.OnCardUsed));
		}

		// Token: 0x06000124 RID: 292 RVA: 0x000042B1 File Offset: 0x000024B1
		private IEnumerable<BattleAction> OnCardUsing(CardUsingEventArgs args)
		{
			if (base.Count > 0)
			{
				base.NotifyActivating();
				yield return PerformAction.Sfx("二重身回合开始", 0f);
				yield return PerformAction.Effect(base.Battle.Player, "JinziMirror", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				args.PlayTwice = true;
				args.AddModifier(this);
			}
			yield break;
		}

		// Token: 0x06000125 RID: 293 RVA: 0x000042C8 File Offset: 0x000024C8
		private void OnCardUsed(CardUsingEventArgs args)
		{
			int count = base.Battle.TurnCardUsageHistory.Count;
			base.Count = Math.Max(0, base.Level - count);
			if (count == 0)
			{
				base.Highlight = false;
			}
		}
	}
}
