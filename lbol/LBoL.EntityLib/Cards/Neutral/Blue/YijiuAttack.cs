using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Neutral.Blue
{
	// Token: 0x02000326 RID: 806
	[UsedImplicitly]
	public sealed class YijiuAttack : Card
	{
		// Token: 0x06000BDB RID: 3035 RVA: 0x000177CE File Offset: 0x000159CE
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<DamageEventArgs>(base.Battle.Player.DamageDealt, new EventSequencedReactor<DamageEventArgs>(this.OnPlayerDamageDealt));
		}

		// Token: 0x06000BDC RID: 3036 RVA: 0x000177F2 File Offset: 0x000159F2
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
