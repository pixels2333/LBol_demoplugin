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
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004C6 RID: 1222
	[UsedImplicitly]
	public sealed class IceSaber : Card
	{
		// Token: 0x170001C9 RID: 457
		// (get) Token: 0x06001038 RID: 4152 RVA: 0x0001CBE4 File Offset: 0x0001ADE4
		public override bool DiscardCard
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06001039 RID: 4153 RVA: 0x0001CBE8 File Offset: 0x0001ADE8
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card hand) => hand != this), (Card c) => Enumerable.Contains<ManaColor>(c.Config.Colors, ManaColor.Blue)));
			if (!list.Empty<Card>())
			{
				return new SelectHandInteraction(0, list.Count, list);
			}
			return null;
		}

		// Token: 0x0600103A RID: 4154 RVA: 0x0001CC52 File Offset: 0x0001AE52
		protected override void SetGuns()
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
			base.DeltaValue1 = 0;
		}

		// Token: 0x0600103B RID: 4155 RVA: 0x0001CC73 File Offset: 0x0001AE73
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			SelectHandInteraction selectHandInteraction = (SelectHandInteraction)precondition;
			if (selectHandInteraction != null)
			{
				base.DeltaValue1 = selectHandInteraction.SelectedCards.Count;
				yield return new DiscardManyAction(selectHandInteraction.SelectedCards);
			}
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
			foreach (GunPair gunPair in base.CardGuns.GunPairs)
			{
				yield return base.AttackAction(selector, gunPair);
			}
			List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			base.DeltaValue1 = 0;
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			EnemyUnit selectedEnemy = selector.SelectedEnemy;
			if (selectedEnemy.IsAlive)
			{
				yield return base.DebuffAction<Cold>(selectedEnemy, 0, 0, 0, 0, true, 0.1f);
			}
			yield break;
			yield break;
		}
	}
}
