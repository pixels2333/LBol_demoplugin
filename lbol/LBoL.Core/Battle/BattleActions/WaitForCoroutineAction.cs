using System;
using System.Collections;
using System.Collections.Generic;
namespace LBoL.Core.Battle.BattleActions
{
	public class WaitForCoroutineAction : SimpleAction
	{
		public IEnumerator Coroutine { get; }
		public WaitForCoroutineAction(IEnumerator coroutine)
		{
			this.Coroutine = coroutine;
		}
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreatePhase("Wait", delegate
			{
			}, true);
			yield break;
		}
	}
}
