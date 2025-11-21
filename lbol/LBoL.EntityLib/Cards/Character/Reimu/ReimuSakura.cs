using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003F5 RID: 1013
	[UsedImplicitly]
	public sealed class ReimuSakura : Card
	{
		// Token: 0x06000E16 RID: 3606 RVA: 0x0001A1C3 File Offset: 0x000183C3
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new GameEventHandler<GameEventArgs>(this.OnBattleStarted), (GameEventPriority)0);
		}

		// Token: 0x06000E17 RID: 3607 RVA: 0x0001A1E3 File Offset: 0x000183E3
		private void OnBattleStarted(GameEventArgs args)
		{
			if (base.Battle.CardExtraGrowAmount > 0)
			{
				base.DecreaseBaseCost(base.Mana * base.Battle.CardExtraGrowAmount);
			}
		}

		// Token: 0x06000E18 RID: 3608 RVA: 0x0001A20F File Offset: 0x0001840F
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			yield return base.HealAction(base.Value1);
			yield break;
		}

		// Token: 0x06000E19 RID: 3609 RVA: 0x0001A226 File Offset: 0x00018426
		public override IEnumerable<BattleAction> AfterUseAction()
		{
			base.DecreaseBaseCost(base.Mana);
			return base.AfterUseAction();
		}
	}
}
