using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.Exhibits.Seija;

namespace LBoL.EntityLib.StatusEffects.Enemy.Seija
{
	// Token: 0x020000D5 RID: 213
	public sealed class SingleJiandaoSe : SeijaSe
	{
		// Token: 0x17000054 RID: 84
		// (get) Token: 0x060002F5 RID: 757 RVA: 0x00008102 File Offset: 0x00006302
		protected override Type ExhibitType
		{
			get
			{
				return typeof(SingleJiandao);
			}
		}

		// Token: 0x060002F6 RID: 758 RVA: 0x00008110 File Offset: 0x00006310
		protected override void OnAdded(Unit unit)
		{
			base.OnAdded(unit);
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
			this.React(PerformAction.Sfx("GuirenItem", 0f));
			this.React(PerformAction.EffectMessage(unit, "SeijaExhibitManager", "AddExhibit", this));
		}

		// Token: 0x060002F7 RID: 759 RVA: 0x00008177 File Offset: 0x00006377
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Battle.Player.IsInTurn)
			{
				base.NotifyActivating();
				yield return new DamageAction(base.Owner, base.Battle.Player, DamageInfo.Reaction((float)base.Level, false), "ZhengxieHit", GunType.Single);
			}
			yield break;
		}
	}
}
