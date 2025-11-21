using System;
using System.Collections;
using System.Collections.Generic;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x020001B5 RID: 437
	public class WaitForCoroutineAction : SimpleAction
	{
		// Token: 0x17000540 RID: 1344
		// (get) Token: 0x06000F8B RID: 3979 RVA: 0x00029978 File Offset: 0x00027B78
		public IEnumerator Coroutine { get; }

		// Token: 0x06000F8C RID: 3980 RVA: 0x00029980 File Offset: 0x00027B80
		public WaitForCoroutineAction(IEnumerator coroutine)
		{
			this.Coroutine = coroutine;
		}

		// Token: 0x06000F8D RID: 3981 RVA: 0x0002998F File Offset: 0x00027B8F
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreatePhase("Wait", delegate
			{
			}, true);
			yield break;
		}
	}
}
