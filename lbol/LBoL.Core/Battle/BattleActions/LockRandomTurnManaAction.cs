using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Base.Extensions;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200018B RID: 395
	public class LockRandomTurnManaAction : SimpleAction
	{
		// Token: 0x06000EB8 RID: 3768 RVA: 0x00027E67 File Offset: 0x00026067
		public LockRandomTurnManaAction(int count)
		{
			this._count = count;
		}

		// Token: 0x06000EB9 RID: 3769 RVA: 0x00027E78 File Offset: 0x00026078
		protected override void ResolvePhase()
		{
			base.React(new Reactor(this.ResolvePhaseEnumerator()), null, default(ActionCause?));
		}

		// Token: 0x06000EBA RID: 3770 RVA: 0x00027EA0 File Offset: 0x000260A0
		private IEnumerable<BattleAction> ResolvePhaseEnumerator()
		{
			ManaColor[] array = (base.Battle.BaseTurnMana - base.Battle.LockedTurnMana).EnumerateComponents().SampleManyOrAll(this._count, base.Battle.GameRun.BattleRng);
			yield return new LockTurnManaAction(ManaGroup.FromComponents(array));
			yield break;
		}

		// Token: 0x17000517 RID: 1303
		// (get) Token: 0x06000EBB RID: 3771 RVA: 0x00027EB0 File Offset: 0x000260B0
		public override bool IsCanceled
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0400068A RID: 1674
		private readonly int _count;
	}
}
