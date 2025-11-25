using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class AddDollAction : EventBattleAction<DollEventArgs>
	{
		public AddDollAction(Doll doll)
		{
			base.Args = new DollEventArgs
			{
				Doll = doll
			};
		}
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreateEventPhase<DollEventArgs>("DollAdding", base.Args, base.Battle.DollAdding);
			PlayerUnit player = base.Battle.Player;
			if (player.DollSlotCount == 0)
			{
				base.Battle.NotifyMessage(BattleMessage.DollSlotEmpty);
				base.Args.ForceCancelBecause(CancelCause.DollSlotEmpty);
				yield break;
			}
			int retryCount = 0;
			while (player.Dolls.Count >= player.DollSlotCount)
			{
				yield return base.CreatePhase("TriggerFirst", delegate
				{
					this.Battle.React(new TriggerDollActiveAction(Enumerable.First<Doll>(player.Dolls), true), null, ActionCause.None);
				}, false);
				int num = retryCount + 1;
				retryCount = num;
				if (retryCount > 4)
				{
					base.Args.ForceCancelBecause(CancelCause.DollSlotFull);
					yield break;
				}
			}
			yield return base.CreatePhase("Main", delegate
			{
				if (this.Args.IsCanceled)
				{
					return;
				}
				player.AddDoll(this.Args.Doll);
			}, true);
			yield return base.CreateEventPhase<DollEventArgs>("DollAdded", base.Args, base.Battle.DollAdded);
			yield break;
		}
	}
}
