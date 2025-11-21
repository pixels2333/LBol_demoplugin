using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000435 RID: 1077
	[UsedImplicitly]
	public sealed class PatchouliFriend : Card
	{
		// Token: 0x1700019C RID: 412
		// (get) Token: 0x06000EB3 RID: 3763 RVA: 0x0001AD79 File Offset: 0x00018F79
		[UsedImplicitly]
		public int Power
		{
			get
			{
				if (!this.IsUpgraded)
				{
					return 3;
				}
				return 5;
			}
		}

		// Token: 0x1700019D RID: 413
		// (get) Token: 0x06000EB4 RID: 3764 RVA: 0x0001AD86 File Offset: 0x00018F86
		[UsedImplicitly]
		public int PassiveColor
		{
			get
			{
				return base.PassiveCost;
			}
		}

		// Token: 0x06000EB5 RID: 3765 RVA: 0x0001AD8E File Offset: 0x00018F8E
		public override IEnumerable<BattleAction> OnTurnStartedInHand()
		{
			return this.GetPassiveActions();
		}

		// Token: 0x06000EB6 RID: 3766 RVA: 0x0001AD96 File Offset: 0x00018F96
		public override IEnumerable<BattleAction> GetPassiveActions()
		{
			if (!base.Summoned || base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			base.Loyalty += base.PassiveCost;
			int num;
			for (int i = 0; i < base.Battle.FriendPassiveTimes; i = num + 1)
			{
				if (base.Battle.BattleShouldEnd)
				{
					yield break;
				}
				yield return PerformAction.Sfx("FairySupport", 0f);
				yield return ConvertManaAction.PhilosophyRandomMana(base.Battle.BattleMana, this.PassiveColor, base.GameRun.BattleRng);
				num = i;
			}
			yield break;
		}

		// Token: 0x06000EB7 RID: 3767 RVA: 0x0001ADA6 File Offset: 0x00018FA6
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition == null || ((MiniSelectCardInteraction)precondition).SelectedCard.FriendToken == FriendToken.Active)
			{
				base.Loyalty += base.ActiveCost;
				yield return base.SkillAnime;
				yield return base.BuffAction<Firepower>(base.Value1, 0, 0, 0, 0.2f);
				yield return base.BuffAction<Spirit>(base.Value1, 0, 0, 0, 0.2f);
				yield return new GainPowerAction(this.Power);
			}
			else
			{
				base.Loyalty += base.ActiveCost2;
				yield return base.SkillAnime;
				yield return new AddCardsToHandAction(Library.CreateCards<Potion>(base.Value2, false), AddCardsType.Normal);
			}
			yield break;
		}
	}
}
