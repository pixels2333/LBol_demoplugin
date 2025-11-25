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
	[UsedImplicitly]
	public sealed class DiyinLeigu : Exhibit
	{
		[UsedImplicitly]
		public ManaGroup LastColor { get; set; }
		protected override string GetBaseDescription()
		{
			if (!(this.LastColor != ManaGroup.Empty))
			{
				return base.GetBaseDescription();
			}
			return base.ExtraDescription;
		}
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new GameEventHandler<UnitEventArgs>(this.OnPlayerTurnEnding));
		}
		protected override void OnLeaveBattle()
		{
			this.LastColor = ManaGroup.Empty;
		}
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
		private void OnPlayerTurnEnding(UnitEventArgs args)
		{
			this.LastColor = ManaGroup.Empty;
		}
	}
}
