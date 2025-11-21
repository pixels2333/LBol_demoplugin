using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Green
{
	// Token: 0x020002F7 RID: 759
	[UsedImplicitly]
	public sealed class HuiyeAttack : Card
	{
		// Token: 0x06000B50 RID: 2896 RVA: 0x00016C30 File Offset: 0x00014E30
		public override ManaGroup GetXCostFromPooled(ManaGroup pooledMana)
		{
			ManaGroup manaGroup = ManaGroup.Empty;
			foreach (ManaColor manaColor in ManaColors.SingleColors)
			{
				if (pooledMana.GetValue(manaColor) > 0 && base.XCostRequiredMana.GetValue(manaColor) == 0)
				{
					manaGroup += ManaGroup.Single(manaColor);
				}
			}
			manaGroup.Philosophy = pooledMana.Philosophy;
			int num = 6;
			if (manaGroup.Amount > num)
			{
				manaGroup -= ManaGroup.Philosophies(manaGroup.Amount - num);
			}
			return manaGroup;
		}

		// Token: 0x06000B51 RID: 2897 RVA: 0x00016CD8 File Offset: 0x00014ED8
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			List<ManaColor> list = Enumerable.ToList<ManaColor>(Enumerable.Where<ManaColor>(ManaColors.SingleColors, (ManaColor color) => consumingMana.GetValue(color) > 0));
			int num = consumingMana.Philosophy + base.GameRun.SynergyAdditionalCount;
			if (num >= 1)
			{
				for (int i = 1; i < num; i++)
				{
					foreach (ManaColor manaColor in ManaColors.SingleColors)
					{
						if (!list.Contains(manaColor))
						{
							list.Add(manaColor);
							break;
						}
					}
				}
				list.Add(ManaColor.Philosophy);
			}
			if (Enumerable.Any<ManaColor>(list))
			{
				base.CardGuns = new Guns();
				foreach (ManaColor manaColor2 in list)
				{
					base.CardGuns.Add("蓬莱的树海" + manaColor2.ToShortName().ToString());
				}
				foreach (GunPair gunPair in base.CardGuns.GunPairs)
				{
					yield return base.AttackAction(selector, gunPair);
				}
				List<GunPair>.Enumerator enumerator3 = default(List<GunPair>.Enumerator);
				if (base.Battle.BattleShouldEnd || consumingMana.Amount < base.Value2)
				{
					yield break;
				}
				if (base.Battle.ExileZone.Count > 0)
				{
					SelectCardInteraction interaction = new SelectCardInteraction(0, base.Value1, base.Battle.ExileZone, SelectedCardHandling.DoNothing)
					{
						Source = this
					};
					yield return new InteractionAction(interaction, false);
					if (interaction.SelectedCards.Count > 0)
					{
						foreach (Card card in interaction.SelectedCards)
						{
							yield return new MoveCardAction(card, CardZone.Hand);
						}
						IEnumerator<Card> enumerator4 = null;
					}
					interaction = null;
				}
			}
			yield break;
			yield break;
		}

		// Token: 0x040000F7 RID: 247
		private const string RawGunName = "蓬莱的树海";
	}
}
