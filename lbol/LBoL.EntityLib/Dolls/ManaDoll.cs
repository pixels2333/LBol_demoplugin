using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Dolls
{
	// Token: 0x02000256 RID: 598
	[UsedImplicitly]
	public sealed class ManaDoll : Doll
	{
		// Token: 0x17000123 RID: 291
		// (get) Token: 0x060009AD RID: 2477 RVA: 0x00014CF3 File Offset: 0x00012EF3
		[UsedImplicitly]
		public ManaGroup Mana1
		{
			get
			{
				return ManaGroup.Philosophies(1);
			}
		}

		// Token: 0x17000124 RID: 292
		// (get) Token: 0x060009AE RID: 2478 RVA: 0x00014CFB File Offset: 0x00012EFB
		[UsedImplicitly]
		public ManaGroup Mana2
		{
			get
			{
				return ManaGroup.Philosophies(2);
			}
		}

		// Token: 0x060009AF RID: 2479 RVA: 0x00014D03 File Offset: 0x00012F03
		protected override IEnumerable<BattleAction> PassiveActions()
		{
			base.NotifyPassiveActivating();
			yield return new GainManaAction(this.Mana1);
			yield break;
		}

		// Token: 0x060009B0 RID: 2480 RVA: 0x00014D13 File Offset: 0x00012F13
		protected override IEnumerable<BattleAction> ActiveActions()
		{
			base.NotifyActiveActivating();
			yield return new GainManaAction(this.Mana2);
			yield break;
		}

		// Token: 0x060009B1 RID: 2481 RVA: 0x00014D23 File Offset: 0x00012F23
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
		{
			base.NotifyActiveActivating();
			yield return new GainManaAction(this.Mana2);
			yield break;
		}
	}
}
