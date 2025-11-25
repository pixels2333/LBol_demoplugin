using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Reimu;
namespace LBoL.EntityLib.StatusEffects.Reimu
{
	[UsedImplicitly]
	public sealed class BoliPhantomSe : StatusEffect
	{
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Empty;
			}
		}
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardEventArgs>(base.Battle.CardExiled, new EventSequencedReactor<CardEventArgs>(this.OnCardExiled));
		}
		private IEnumerable<BattleAction> OnCardExiled(CardEventArgs args)
		{
			if (args.Cause != ActionCause.AutoExile && !(args.Card is Fengmozhen))
			{
				List<Card> list = new List<Card>();
				for (int i = 0; i < base.Level; i++)
				{
					Fengmozhen fengmozhen = Library.CreateCard<Fengmozhen>();
					fengmozhen.IsPhantom = true;
					fengmozhen.SetTurnCost(this.Mana);
					list.Add(fengmozhen);
				}
				base.NotifyActivating();
				yield return new AddCardsToHandAction(list, AddCardsType.Normal);
			}
			yield break;
		}
	}
}
