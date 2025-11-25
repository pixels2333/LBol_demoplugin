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
	public sealed class SingleJiandaoSe : SeijaSe
	{
		protected override Type ExhibitType
		{
			get
			{
				return typeof(SingleJiandao);
			}
		}
		protected override void OnAdded(Unit unit)
		{
			base.OnAdded(unit);
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
			this.React(PerformAction.Sfx("GuirenItem", 0f));
			this.React(PerformAction.EffectMessage(unit, "SeijaExhibitManager", "AddExhibit", this));
		}
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
