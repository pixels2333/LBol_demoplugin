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
	public sealed class QiannianShenqiSe : SeijaSe
	{
		protected override Type ExhibitType
		{
			get
			{
				return typeof(QiannianShenqi);
			}
		}
		public override bool ForceNotShowDownText { get; set; } = true;
		protected override string GetBaseDescription()
		{
			if (!this.LoseLifeVersion)
			{
				return base.GetBaseDescription();
			}
			return base.ExtraDescription;
		}
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
		private void OnPredraw(CardEventArgs args)
		{
			if (!this.LoseLifeVersion && base.Count <= 0)
			{
				base.NotifyActivating();
				args.CancelBy(this);
			}
		}
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
		private bool _loseLifeVersion;
	}
}
