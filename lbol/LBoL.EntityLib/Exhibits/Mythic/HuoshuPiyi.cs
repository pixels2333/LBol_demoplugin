using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Exhibits.Mythic
{
	// Token: 0x02000150 RID: 336
	[UsedImplicitly]
	public sealed class HuoshuPiyi : MythicExhibit
	{
		// Token: 0x06000494 RID: 1172 RVA: 0x0000BE9D File Offset: 0x0000A09D
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
		}

		// Token: 0x06000495 RID: 1173 RVA: 0x0000BEBC File Offset: 0x0000A0BC
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			base.NotifyActivating();
			yield return new CastBlockShieldAction(base.Owner, 0, base.Value1, BlockShieldType.Normal, true);
			yield return new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(base.Value2), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
	}
}
