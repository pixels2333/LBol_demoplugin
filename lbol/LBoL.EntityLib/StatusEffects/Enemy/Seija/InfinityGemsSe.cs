using System;
using System.Collections.Generic;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.Exhibits.Seija;

namespace LBoL.EntityLib.StatusEffects.Enemy.Seija
{
	// Token: 0x020000CE RID: 206
	public sealed class InfinityGemsSe : SeijaSe
	{
		// Token: 0x17000047 RID: 71
		// (get) Token: 0x060002CC RID: 716 RVA: 0x00007992 File Offset: 0x00005B92
		protected override Type ExhibitType
		{
			get
			{
				return typeof(InfinityGems);
			}
		}

		// Token: 0x060002CD RID: 717 RVA: 0x000079A0 File Offset: 0x00005BA0
		protected override void OnAdded(Unit unit)
		{
			base.OnAdded(unit);
			base.Count = 2;
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
			this.React(PerformAction.Sfx("GuirenItem", 0f));
			this.React(PerformAction.EffectMessage(unit, "SeijaExhibitManager", "AddExhibit", this));
			base.Highlight = true;
		}

		// Token: 0x060002CE RID: 718 RVA: 0x00007A15 File Offset: 0x00005C15
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd || base.Owner.HasStatusEffect<DragonBallSe>() || !base.Owner.IsAlive || base.Count == 0)
			{
				yield break;
			}
			int num = base.Count - 1;
			base.Count = num;
			if (base.Count == 0)
			{
				base.Highlight = false;
			}
			base.NotifyActivating();
			List<Card> list = new List<Card>();
			list.AddRange(base.Battle.DrawZone);
			list.AddRange(base.Battle.DiscardZone);
			list.AddRange(base.Battle.HandZone);
			Card[] exileCards = list.SampleMany(list.Count / 2, base.GameRun.EnemyBattleRng);
			yield return PerformAction.Animation(base.Owner, "skill1", 0.8f, null, 0f, -1);
			yield return new ExileManyCardAction(exileCards);
			yield break;
		}
	}
}
