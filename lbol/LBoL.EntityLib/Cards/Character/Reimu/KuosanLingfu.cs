using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Reimu
{
	[UsedImplicitly]
	public sealed class KuosanLingfu : Card
	{
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<DamageEventArgs>(base.Battle.Player.DamageDealt, new EventSequencedReactor<DamageEventArgs>(this.OnPlayerDamageDealt));
		}
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
