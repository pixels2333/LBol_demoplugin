using System;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Basic;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x020003C4 RID: 964
	[UsedImplicitly]
	public sealed class VampireHunter : Card
	{
		// Token: 0x06000D96 RID: 3478 RVA: 0x00019785 File Offset: 0x00017985
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<DamageDealingEventArgs>(base.Battle.Player.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnPlayerDamageDealing), (GameEventPriority)0);
		}

		// Token: 0x06000D97 RID: 3479 RVA: 0x000197AC File Offset: 0x000179AC
		private void OnPlayerDamageDealing(DamageDealingEventArgs args)
		{
			if (args.ActionSource == this && args.Targets != null)
			{
				if (Enumerable.Any<Unit>(args.Targets, (Unit target) => target.HasStatusEffect<Vampire>()))
				{
					args.DamageInfo = args.DamageInfo.MultiplyBy(base.Value1);
				}
			}
		}
	}
}
