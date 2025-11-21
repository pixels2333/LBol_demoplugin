using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using UnityEngine;

namespace LBoL.EntityLib.Dolls
{
	// Token: 0x02000254 RID: 596
	[UsedImplicitly]
	public sealed class ChargeDoll : Doll
	{
		// Token: 0x1700011C RID: 284
		// (get) Token: 0x0600099D RID: 2461 RVA: 0x00014BF8 File Offset: 0x00012DF8
		[UsedImplicitly]
		public new DamageInfo Damage
		{
			get
			{
				return DamageInfo.Reaction((float)this._counter, false);
			}
		}

		// Token: 0x1700011D RID: 285
		// (get) Token: 0x0600099E RID: 2462 RVA: 0x00014C07 File Offset: 0x00012E07
		public override int? UpCounter
		{
			get
			{
				return new int?(this._counter);
			}
		}

		// Token: 0x1700011E RID: 286
		// (get) Token: 0x0600099F RID: 2463 RVA: 0x00014C14 File Offset: 0x00012E14
		public override Color UpCounterColor
		{
			get
			{
				return Color.cyan;
			}
		}

		// Token: 0x1700011F RID: 287
		// (get) Token: 0x060009A0 RID: 2464 RVA: 0x00014C1B File Offset: 0x00012E1B
		public override int? DownCounter
		{
			get
			{
				return new int?(base.Value1);
			}
		}

		// Token: 0x060009A1 RID: 2465 RVA: 0x00014C28 File Offset: 0x00012E28
		public override void Initialize()
		{
			base.Initialize();
			this._counter = base.Config.Value1.GetValueOrDefault();
		}

		// Token: 0x060009A2 RID: 2466 RVA: 0x00014C54 File Offset: 0x00012E54
		protected override IEnumerable<BattleAction> PassiveActions()
		{
			base.NotifyPassiveActivating();
			this._counter += base.Value1;
			this.NotifyChanged();
			yield return PerformAction.Doll(this, null, "", 0.1f, "ChargeDoll charge");
			yield break;
		}

		// Token: 0x060009A3 RID: 2467 RVA: 0x00014C64 File Offset: 0x00012E64
		protected override IEnumerable<BattleAction> ActiveActions()
		{
			base.NotifyActiveActivating();
			yield return new DamageAction(base.Owner, base.Battle.RandomAliveEnemy, this.Damage, "Instant", GunType.Single);
			yield break;
		}

		// Token: 0x060009A4 RID: 2468 RVA: 0x00014C74 File Offset: 0x00012E74
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
		{
			base.NotifyActiveActivating();
			yield return new DamageAction(base.Owner, base.Battle.RandomAliveEnemy, this.Damage, "Instant", GunType.Single);
			yield break;
		}

		// Token: 0x040000E7 RID: 231
		private int _counter;
	}
}
