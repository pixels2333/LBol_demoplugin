using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.Exhibits.Seija;

namespace LBoL.EntityLib.StatusEffects.Enemy.Seija
{
	// Token: 0x020000D0 RID: 208
	public sealed class QiannianShenqiSe : SeijaSe
	{
		// Token: 0x17000049 RID: 73
		// (get) Token: 0x060002D4 RID: 724 RVA: 0x00007B85 File Offset: 0x00005D85
		protected override Type ExhibitType
		{
			get
			{
				return typeof(QiannianShenqi);
			}
		}

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x060002D5 RID: 725 RVA: 0x00007B91 File Offset: 0x00005D91
		// (set) Token: 0x060002D6 RID: 726 RVA: 0x00007B99 File Offset: 0x00005D99
		public override bool ForceNotShowDownText { get; set; } = true;

		// Token: 0x060002D7 RID: 727 RVA: 0x00007BA2 File Offset: 0x00005DA2
		protected override string GetBaseDescription()
		{
			if (!this.LoseLifeVersion)
			{
				return base.GetBaseDescription();
			}
			return base.ExtraDescription;
		}

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x060002D8 RID: 728 RVA: 0x00007BB9 File Offset: 0x00005DB9
		// (set) Token: 0x060002D9 RID: 729 RVA: 0x00007BC1 File Offset: 0x00005DC1
		public bool LoseLifeVersion
		{
			get
			{
				return this._loseLifeVersion;
			}
			set
			{
				this._loseLifeVersion = value;
				this.ForceNotShowDownText = !value;
				this.NotifyChanged();
			}
		}

		// Token: 0x060002DA RID: 730 RVA: 0x00007BDC File Offset: 0x00005DDC
		protected override void OnAdded(Unit unit)
		{
			base.OnAdded(unit);
			base.HandleOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, delegate(UnitEventArgs _)
			{
				base.Count = base.Limit;
				base.Highlight = false;
			});
			base.HandleOwnerEvent<CardEventArgs>(base.Battle.Predraw, new GameEventHandler<CardEventArgs>(this.OnPredraw));
			base.ReactOwnerEvent<CardEventArgs>(base.Battle.CardDrawn, new EventSequencedReactor<CardEventArgs>(this.OnCardDrawn));
			this.React(PerformAction.Sfx("GuirenItem", 0f));
			this.React(PerformAction.EffectMessage(unit, "SeijaExhibitManager", "AddExhibit", this));
			IEnumerable<QiannianShenqiCard> enumerable = Library.CreateCards<QiannianShenqiCard>(1, false);
			if (base.Battle.HandIsFull)
			{
				this.React(new AddCardsToDrawZoneAction(enumerable, DrawZoneTarget.Top, AddCardsType.Normal));
				return;
			}
			this.React(new AddCardsToHandAction(enumerable, AddCardsType.Normal));
		}

		// Token: 0x060002DB RID: 731 RVA: 0x00007CBD File Offset: 0x00005EBD
		private void OnPredraw(CardEventArgs args)
		{
			if (!this.LoseLifeVersion && base.Count <= 0)
			{
				base.NotifyActivating();
				args.CancelBy(this);
			}
		}

		// Token: 0x060002DC RID: 732 RVA: 0x00007CDD File Offset: 0x00005EDD
		private IEnumerable<BattleAction> OnCardDrawn(CardEventArgs args)
		{
			if (base.Count > 0)
			{
				int num = base.Count - 1;
				base.Count = num;
				if (base.Count <= 0)
				{
					base.Highlight = true;
				}
			}
			else if (this.LoseLifeVersion)
			{
				base.NotifyActivating();
				yield return DamageAction.LoseLife(base.Battle.Player, base.Level, "Poison");
			}
			yield break;
		}

		// Token: 0x04000024 RID: 36
		private bool _loseLifeVersion;
	}
}
