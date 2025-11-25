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
using LBoL.EntityLib.Cards.Neutral.Colorless.YijiSkills;
namespace LBoL.EntityLib.Cards.Neutral.Colorless
{
	[UsedImplicitly]
	public sealed class YijiSkill : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			List<Card> list = new List<Card>();
			List<Type> list2 = Enumerable.ToList<Type>(this._types);
			if (this.IsUpgraded)
			{
				list2.AddRange(this._upgradedTypes);
			}
			list2.Shuffle(base.GameRun.BattleCardRng);
			int num = 0;
			while (num < base.Value1 && list2.Count > num)
			{
				list.Add(Library.CreateCard(list2[num]));
				num++;
			}
			SelectCardInteraction interaction = new SelectCardInteraction(1, 1, list, SelectedCardHandling.DoNothing)
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
		public YijiSkill()
		{
			List<Type> list = new List<Type>();
			list.Add(typeof(YijiEarthSpike));
			list.Add(typeof(YijiPurify));
			list.Add(typeof(YijiGraze));
			this._types = list;
			List<Type> list2 = new List<Type>();
			list2.Add(typeof(YijiFirepower));
			this._upgradedTypes = list2;
			base..ctor();
		}
		private readonly List<Type> _types;
		private readonly List<Type> _upgradedTypes;
	}
}
