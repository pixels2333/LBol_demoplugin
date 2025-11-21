using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Basic;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003CC RID: 972
	[UsedImplicitly]
	public sealed class DanmuDuijue : Card
	{
		// Token: 0x17000184 RID: 388
		// (get) Token: 0x06000DAF RID: 3503 RVA: 0x000199C6 File Offset: 0x00017BC6
		protected override int AdditionalBlock
		{
			get
			{
				if (base.Battle != null && !this.IsUpgraded)
				{
					return this.PlayerFirepowerPositive;
				}
				return 0;
			}
		}

		// Token: 0x17000185 RID: 389
		// (get) Token: 0x06000DB0 RID: 3504 RVA: 0x000199E0 File Offset: 0x00017BE0
		protected override int AdditionalShield
		{
			get
			{
				if (base.Battle != null && this.IsUpgraded)
				{
					return this.PlayerFirepowerPositive;
				}
				return 0;
			}
		}

		// Token: 0x17000186 RID: 390
		// (get) Token: 0x06000DB1 RID: 3505 RVA: 0x000199FA File Offset: 0x00017BFA
		protected override int AdditionalValue1
		{
			get
			{
				if (base.Battle != null)
				{
					return this.PlayerFirepowerPositive;
				}
				return 0;
			}
		}

		// Token: 0x17000187 RID: 391
		// (get) Token: 0x06000DB2 RID: 3506 RVA: 0x00019A0C File Offset: 0x00017C0C
		private int PlayerFirepowerPositive
		{
			get
			{
				return Math.Max(0, base.Battle.Player.TotalFirepower);
			}
		}

		// Token: 0x06000DB3 RID: 3507 RVA: 0x00019A24 File Offset: 0x00017C24
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return base.BuffAction<Reflect>(base.Value1, 0, 0, 0, 0.2f);
			if (base.Battle.Player.HasStatusEffect<Reflect>())
			{
				base.Battle.Player.GetStatusEffect<Reflect>().Gun = (this.IsUpgraded ? "弹幕对决B" : "弹幕对决");
			}
			yield break;
		}
	}
}
