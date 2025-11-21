using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003DE RID: 990
	[UsedImplicitly]
	public sealed class KuosanLingfu : Card
	{
		// Token: 0x06000DE2 RID: 3554 RVA: 0x00019D8A File Offset: 0x00017F8A
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<DamageEventArgs>(base.Battle.Player.DamageDealt, new EventSequencedReactor<DamageEventArgs>(this.OnPlayerDamageDealt));
		}

		// Token: 0x06000DE3 RID: 3555 RVA: 0x00019DAE File Offset: 0x00017FAE
		private IEnumerable<BattleAction> OnPlayerDamageDealt(DamageEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (args.Cause == ActionCause.Card && args.ActionSource == this)
			{
				DamageInfo damageInfo = args.DamageInfo;
				if (damageInfo.Damage > 0f)
				{
					yield return new CastBlockShieldAction(base.Battle.Player, 0, (int)damageInfo.Damage, BlockShieldType.Normal, false);
				}
			}
			yield break;
		}
	}
}
