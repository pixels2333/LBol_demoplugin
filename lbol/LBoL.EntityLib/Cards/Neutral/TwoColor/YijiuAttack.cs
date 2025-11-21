using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x020002B9 RID: 697
	[UsedImplicitly]
	public sealed class YijiuAttack : Card
	{
		// Token: 0x06000AB4 RID: 2740 RVA: 0x000160A6 File Offset: 0x000142A6
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<DamageEventArgs>(base.Battle.Player.DamageDealt, new EventSequencedReactor<DamageEventArgs>(this.OnPlayerDamageDealt));
		}

		// Token: 0x06000AB5 RID: 2741 RVA: 0x000160CA File Offset: 0x000142CA
		private IEnumerable<BattleAction> OnPlayerDamageDealt(DamageEventArgs args)
		{
			if (args.ActionSource == this && args.Kill)
			{
				yield return new ExileCardAction(this);
				yield return base.BuffAction<Invincible>(0, base.Value1, 0, 0, 0.2f);
			}
			yield break;
		}
	}
}
