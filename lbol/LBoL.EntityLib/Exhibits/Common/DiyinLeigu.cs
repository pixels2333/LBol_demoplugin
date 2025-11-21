using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000161 RID: 353
	[UsedImplicitly]
	public sealed class DiyinLeigu : Exhibit
	{
		// Token: 0x17000074 RID: 116
		// (get) Token: 0x060004DA RID: 1242 RVA: 0x0000C649 File Offset: 0x0000A849
		// (set) Token: 0x060004DB RID: 1243 RVA: 0x0000C651 File Offset: 0x0000A851
		[UsedImplicitly]
		public ManaGroup LastColor { get; set; }

		// Token: 0x060004DC RID: 1244 RVA: 0x0000C65A File Offset: 0x0000A85A
		protected override string GetBaseDescription()
		{
			if (!(this.LastColor != ManaGroup.Empty))
			{
				return base.GetBaseDescription();
			}
			return base.ExtraDescription;
		}

		// Token: 0x060004DD RID: 1245 RVA: 0x0000C67C File Offset: 0x0000A87C
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new GameEventHandler<UnitEventArgs>(this.OnPlayerTurnEnding));
		}

		// Token: 0x060004DE RID: 1246 RVA: 0x0000C6C8 File Offset: 0x0000A8C8
		protected override void OnLeaveBattle()
		{
			this.LastColor = ManaGroup.Empty;
		}

		// Token: 0x060004DF RID: 1247 RVA: 0x0000C6D5 File Offset: 0x0000A8D5
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			IReadOnlyList<ManaColor> colors = args.Card.Config.Colors;
			ManaGroup newColor = (colors.Empty<ManaColor>() ? ManaGroup.Single(ManaColor.Colorless) : ManaGroup.FromComponents(colors));
			if (!ManaGroup.Intersect(this.LastColor, newColor).IsEmpty)
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<TempFirepower>(base.Battle.Player, new int?(base.Value1), default(int?), default(int?), default(int?), 0f, true);
			}
			this.LastColor = newColor;
			yield break;
		}

		// Token: 0x060004E0 RID: 1248 RVA: 0x0000C6EC File Offset: 0x0000A8EC
		private void OnPlayerTurnEnding(UnitEventArgs args)
		{
			this.LastColor = ManaGroup.Empty;
		}
	}
}
