using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.TwoColor
{
	// Token: 0x02000048 RID: 72
	[UsedImplicitly]
	public sealed class SanaeSummonGodSe : StatusEffect
	{
		// Token: 0x060000DF RID: 223 RVA: 0x00003927 File Offset: 0x00001B27
		protected override string GetBaseDescription()
		{
			if (base.Count != 1)
			{
				return base.GetBaseDescription();
			}
			return base.ExtraDescription;
		}

		// Token: 0x060000E0 RID: 224 RVA: 0x0000393F File Offset: 0x00001B3F
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x060000E1 RID: 225 RVA: 0x00003963 File Offset: 0x00001B63
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			int num = base.Count - 1;
			base.Count = num;
			if (base.Count <= 0)
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<Firepower>(base.Battle.Player, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
				yield return new ApplyStatusEffectAction<Spirit>(base.Battle.Player, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}
	}
}
