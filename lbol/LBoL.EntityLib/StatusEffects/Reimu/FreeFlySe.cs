using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Reimu
{
	// Token: 0x0200002B RID: 43
	[UsedImplicitly]
	public sealed class FreeFlySe : StatusEffect
	{
		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000074 RID: 116 RVA: 0x00002C60 File Offset: 0x00000E60
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Single(ManaColor.Philosophy);
			}
		}

		// Token: 0x06000075 RID: 117 RVA: 0x00002C68 File Offset: 0x00000E68
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted));
		}

		// Token: 0x06000076 RID: 118 RVA: 0x00002C8C File Offset: 0x00000E8C
		private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
		{
			if (!base.Battle.BattleShouldEnd)
			{
				base.NotifyActivating();
				yield return new GainManaAction(ManaGroup.Philosophies(base.Level));
			}
			yield break;
		}
	}
}
