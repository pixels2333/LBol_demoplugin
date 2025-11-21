using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.Green
{
	// Token: 0x0200005B RID: 91
	public sealed class HuiyeManaSe : StatusEffect
	{
		// Token: 0x0600013D RID: 317 RVA: 0x0000470E File Offset: 0x0000290E
		protected override void OnAdded(Unit unit)
		{
			base.Count = 0;
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerStarted));
		}

		// Token: 0x0600013E RID: 318 RVA: 0x00004734 File Offset: 0x00002934
		private IEnumerable<BattleAction> OnOwnerStarted(UnitEventArgs args)
		{
			base.NotifyActivating();
			base.Count += base.Level;
			ManaGroup manaGroup = ManaGroup.Empty;
			for (int i = 0; i < base.Count; i++)
			{
				manaGroup += ManaGroup.Single(ManaColors.Colors.Sample(base.GameRun.BattleRng));
			}
			yield return new GainManaAction(manaGroup);
			yield break;
		}
	}
}
