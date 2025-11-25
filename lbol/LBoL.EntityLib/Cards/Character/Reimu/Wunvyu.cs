using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Reimu
{
	[UsedImplicitly]
	public sealed class Wunvyu : YinyangCardBase
	{
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<CardEventArgs>(base.Battle.CardDrawn, new GameEventHandler<CardEventArgs>(this.OnCardDrawn), (GameEventPriority)0);
		}
		private void OnCardDrawn(CardEventArgs args)
		{
			if (base.Zone == CardZone.Hand && args.Cause != ActionCause.TurnStart)
			{
				base.DeltaDamage += base.Value2;
				this.NotifyChanged();
			}
		}
		protected override void SetGuns()
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
		}
	}
}
