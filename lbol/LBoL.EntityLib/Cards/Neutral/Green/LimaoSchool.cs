using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Neutral.Green.Morphs;
namespace LBoL.EntityLib.Cards.Neutral.Green
{
	[UsedImplicitly]
	public sealed class LimaoSchool : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			List<Card> list = new List<Card>();
			List<Type> list2 = Enumerable.ToList<Type>(this._types);
			if (base.Battle.LimaoSchoolFrogTimes > 0)
			{
				list2.Remove(typeof(MorphFrog));
			}
			list2.Shuffle(base.GameRun.BattleCardRng);
			int num = 0;
			while (num < base.Value1 && list2.Count > num)
			{
				list.Add(Library.CreateCard(list2[num]));
				num++;
			}
			SelectCardInteraction interaction = new SelectCardInteraction(base.Value2, base.Value2, list, SelectedCardHandling.DoNothing)
			{
				Source = this
			};
			yield return new InteractionAction(interaction, false);
			foreach (Card card in interaction.SelectedCards)
			{
				OptionCard optionCard = card as OptionCard;
				if (optionCard != null)
				{
					optionCard.SetBattle(base.Battle);
					foreach (BattleAction battleAction in optionCard.TakeEffectActions())
					{
						yield return battleAction;
					}
					IEnumerator<BattleAction> enumerator2 = null;
				}
			}
			IEnumerator<Card> enumerator = null;
			yield break;
			yield break;
		}
		public LimaoSchool()
		{
			List<Type> list = new List<Type>();
			list.Add(typeof(MorphBird));
			list.Add(typeof(MorphDog));
			list.Add(typeof(MorphFrog));
			list.Add(typeof(MorphMan));
			this._types = list;
			base..ctor();
		}
		private readonly List<Type> _types;
	}
}
