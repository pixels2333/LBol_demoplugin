using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x02000098 RID: 152
	[UsedImplicitly]
	public sealed class DreamServant : StatusEffect
	{
		// Token: 0x17000033 RID: 51
		// (get) Token: 0x06000221 RID: 545 RVA: 0x000066CF File Offset: 0x000048CF
		// (set) Token: 0x06000222 RID: 546 RVA: 0x000066D7 File Offset: 0x000048D7
		private int TurnCounter { get; set; }

		// Token: 0x06000223 RID: 547 RVA: 0x000066E0 File Offset: 0x000048E0
		protected override void OnAdded(Unit unit)
		{
			this.TurnCounter = 0;
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageTaking, new GameEventHandler<DamageEventArgs>(this.OnDamageTaking));
			base.ReactOwnerEvent<UnitEventArgs>(unit.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnded));
		}

		// Token: 0x06000224 RID: 548 RVA: 0x0000671C File Offset: 0x0000491C
		private void OnDamageTaking(DamageEventArgs args)
		{
			args.DamageInfo = args.DamageInfo.MultiplyBy(0);
		}

		// Token: 0x06000225 RID: 549 RVA: 0x0000673E File Offset: 0x0000493E
		private IEnumerable<BattleAction> OnTurnEnded(UnitEventArgs args)
		{
			int num = this.TurnCounter + 1;
			this.TurnCounter = num;
			if (this.TurnCounter >= 2)
			{
				yield return new EscapeAction(base.Owner);
			}
			yield break;
		}

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x06000226 RID: 550 RVA: 0x0000674E File Offset: 0x0000494E
		public override string UnitEffectName
		{
			get
			{
				return "DreamLoop";
			}
		}
	}
}
