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

namespace LBoL.EntityLib.Cards.Character.Cirno.Friend
{
	// Token: 0x020004DC RID: 1244
	[UsedImplicitly]
	public sealed class LunaFriend : Card
	{
		// Token: 0x170001D2 RID: 466
		// (get) Token: 0x0600107F RID: 4223 RVA: 0x0001D094 File Offset: 0x0001B294
		[UsedImplicitly]
		public new int Block
		{
			get
			{
				return base.ConfigBlock;
			}
		}

		// Token: 0x06001080 RID: 4224 RVA: 0x0001D09C File Offset: 0x0001B29C
		public override IEnumerable<BattleAction> OnTurnEndingInHand()
		{
			return this.GetPassiveActions();
		}

		// Token: 0x06001081 RID: 4225 RVA: 0x0001D0A4 File Offset: 0x0001B2A4
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
				yield return PerformAction.Effect(base.Battle.Player, "LunaFairy", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				yield return base.DefenseAction(this.Block, 0, BlockShieldType.Direct, false);
				num = i;
			}
			yield break;
		}

		// Token: 0x06001082 RID: 4226 RVA: 0x0001D0B4 File Offset: 0x0001B2B4
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition == null || ((MiniSelectCardInteraction)precondition).SelectedCard.FriendToken == FriendToken.Active)
			{
				base.Loyalty += base.ActiveCost;
				base.CardGuns = new Guns(base.GunName, base.Value1, true);
				foreach (GunPair gunPair in base.CardGuns.GunPairs)
				{
					yield return base.AttackRandomAliveEnemyAction(gunPair);
				}
				List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			}
			else
			{
				base.Loyalty += base.UltimateCost;
				base.UltimateUsed = true;
				yield return base.SpellAnime;
				yield return base.BuffAction<Invincible>(0, base.Value2, 0, 0, 0.2f);
			}
			yield break;
			yield break;
		}
	}
}
