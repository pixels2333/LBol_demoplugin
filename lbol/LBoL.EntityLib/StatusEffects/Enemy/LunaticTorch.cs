using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000BA RID: 186
	[UsedImplicitly]
	public sealed class LunaticTorch : StatusEffect
	{
		// Token: 0x17000042 RID: 66
		// (get) Token: 0x0600028D RID: 653 RVA: 0x0000725D File Offset: 0x0000545D
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Philosophies(1);
			}
		}

		// Token: 0x0600028E RID: 654 RVA: 0x00007265 File Offset: 0x00005465
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x0600028F RID: 655 RVA: 0x00007289 File Offset: 0x00005489
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			yield return new GainManaAction(this.Mana);
			yield break;
		}
	}
}
