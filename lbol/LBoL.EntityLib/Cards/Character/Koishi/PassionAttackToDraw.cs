using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Koishi;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000488 RID: 1160
	[UsedImplicitly]
	public sealed class PassionAttackToDraw : Card
	{
		// Token: 0x06000F83 RID: 3971 RVA: 0x0001BB5E File Offset: 0x00019D5E
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
			foreach (GunPair gunPair in base.CardGuns.GunPairs)
			{
				yield return base.AttackAction(selector, gunPair);
			}
			List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return base.BuffAction<MoodPassion>(0, 0, 0, 0, 0.2f);
			yield break;
			yield break;
		}

		// Token: 0x06000F84 RID: 3972 RVA: 0x0001BB75 File Offset: 0x00019D75
		public override IEnumerable<BattleAction> AfterUseAction()
		{
			yield return base.EchoCloneAction();
			if (base.IsExile)
			{
				yield return new ExileCardAction(this);
			}
			if (base.Zone == CardZone.PlayArea)
			{
				yield return new MoveCardToDrawZoneAction(this, DrawZoneTarget.Random);
			}
			int num = base.PlayCount + 1;
			base.PlayCount = num;
			yield break;
		}
	}
}
