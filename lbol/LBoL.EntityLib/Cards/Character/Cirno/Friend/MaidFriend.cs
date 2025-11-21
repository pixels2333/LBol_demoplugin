using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Sakuya;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace LBoL.EntityLib.Cards.Character.Cirno.Friend
{
	// Token: 0x020004DD RID: 1245
	[UsedImplicitly]
	public sealed class MaidFriend : Card
	{
		// Token: 0x170001D3 RID: 467
		// (get) Token: 0x06001084 RID: 4228 RVA: 0x0001D0D3 File Offset: 0x0001B2D3
		protected override int AdditionalDamage
		{
			get
			{
				return base.GetSeLevel<MaidFriendSe>();
			}
		}

		// Token: 0x06001085 RID: 4229 RVA: 0x0001D0DB File Offset: 0x0001B2DB
		public override IEnumerable<BattleAction> OnTurnEndingInHand()
		{
			return this.GetPassiveActions();
		}

		// Token: 0x06001086 RID: 4230 RVA: 0x0001D0E3 File Offset: 0x0001B2E3
		public override IEnumerable<BattleAction> GetPassiveActions()
		{
			if (!base.Summoned || base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			base.Loyalty += base.PassiveCost;
			int num2;
			for (int i = 0; i < base.Battle.FriendPassiveTimes; i = num2 + 1)
			{
				if (base.Battle.BattleShouldEnd)
				{
					yield break;
				}
				yield return PerformAction.Sfx("FairySupport", 0f);
				yield return PerformAction.Effect(base.Battle.Player, "MaidFairy", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				EnemyUnit enemyUnit = base.Battle.EnemyGroup.Alives.MinBy((EnemyUnit unit) => unit.Hp);
				int num = Math.Max(0, this.AdditionalDamage / base.ConfigValue1);
				string text = "女仆妖精" + Math.Min(9, num).ToString();
				yield return base.AttackAction(enemyUnit, this.Damage, text);
				num2 = i;
			}
			yield break;
		}

		// Token: 0x06001087 RID: 4231 RVA: 0x0001D0F3 File Offset: 0x0001B2F3
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition == null || ((MiniSelectCardInteraction)precondition).SelectedCard.FriendToken == FriendToken.Active)
			{
				base.Loyalty += base.ActiveCost;
				EnemyUnit enemyUnit = base.Battle.EnemyGroup.Alives.MinBy((EnemyUnit unit) => unit.Hp);
				int num = Math.Max(0, this.AdditionalDamage / base.ConfigValue1);
				string text = "女仆妖精" + Math.Min(9, num).ToString() + "NoAni";
				yield return base.AttackAction(enemyUnit, this.Damage, text);
				if (base.Battle.BattleShouldEnd)
				{
					yield break;
				}
				yield return new AddCardsToHandAction(Library.CreateCards<Knife>(base.Value2, false), AddCardsType.Normal);
			}
			else
			{
				base.Loyalty += base.UltimateCost;
				base.UltimateUsed = true;
				EnemyUnit enemyUnit2 = base.Battle.EnemyGroup.Alives.MinBy((EnemyUnit unit) => unit.Hp);
				int num2 = Math.Max(0, this.AdditionalDamage / base.ConfigValue1);
				string text2 = "女仆妖精" + Math.Min(9, num2).ToString() + "NoAni";
				yield return base.AttackAction(enemyUnit2, this.Damage, text2);
				if (base.Battle.BattleShouldEnd)
				{
					yield break;
				}
				yield return base.BuffAction<MaidFriendSe>(base.Value1, 0, 0, 0, 0.2f);
			}
			yield break;
		}
	}
}
