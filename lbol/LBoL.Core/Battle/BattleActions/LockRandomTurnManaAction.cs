using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Base.Extensions;
namespace LBoL.Core.Battle.BattleActions
{
	public class LockRandomTurnManaAction : SimpleAction
	{
		public LockRandomTurnManaAction(int count)
		{
			this._count = count;
		}
		protected override void ResolvePhase()
		{
			base.React(new Reactor(this.ResolvePhaseEnumerator()), null, default(ActionCause?));
		}
		private IEnumerable<BattleAction> ResolvePhaseEnumerator()
		{
			ManaColor[] array = (base.Battle.BaseTurnMana - base.Battle.LockedTurnMana).EnumerateComponents().SampleManyOrAll(this._count, base.Battle.GameRun.BattleRng);
			yield return new LockTurnManaAction(ManaGroup.FromComponents(array));
			yield break;
		}
		public override bool IsCanceled
		{
			get
			{
				return false;
			}
		}
		private readonly int _count;
	}
}
