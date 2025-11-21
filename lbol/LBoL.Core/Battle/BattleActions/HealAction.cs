using System;
using System.Collections.Generic;
using LBoL.Base.Extensions;
using LBoL.Core.Units;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000188 RID: 392
	public sealed class HealAction : EventBattleAction<HealEventArgs>
	{
		// Token: 0x17000511 RID: 1297
		// (get) Token: 0x06000EA9 RID: 3753 RVA: 0x00027D23 File Offset: 0x00025F23
		public float WaitTime { get; }

		// Token: 0x06000EAA RID: 3754 RVA: 0x00027D2B File Offset: 0x00025F2B
		public HealAction(Unit source, Unit target, int amount, HealType type = HealType.Normal, float waitTime = 0.2f)
		{
			base.Args = new HealEventArgs
			{
				Source = source,
				Target = target,
				Amount = (float)amount,
				HealType = type
			};
			this.WaitTime = waitTime;
		}

		// Token: 0x06000EAB RID: 3755 RVA: 0x00027D64 File Offset: 0x00025F64
		internal override IEnumerable<Phase> GetPhases()
		{
			base.Args.ActionSource = base.Source;
			yield return base.CreateEventPhase<HealEventArgs>("HealingGiving", base.Args, base.Args.Source.HealingGiving);
			yield return base.CreateEventPhase<HealEventArgs>("HealingReceiving", base.Args, base.Args.Target.HealingReceiving);
			yield return base.CreatePhase("Main", delegate
			{
				if (!base.Args.IsCanceled)
				{
					base.Args.Amount = base.Args.Amount.Round();
					base.Args.Amount = (float)base.Battle.Heal(base.Args.Target, base.Args.Amount.ToInt());
					base.Args.IsModified = true;
				}
			}, true);
			yield return base.CreateEventPhase<HealEventArgs>("HealingGiven", base.Args, base.Args.Source.HealingGiven);
			yield return base.CreateEventPhase<HealEventArgs>("HealingReceived", base.Args, base.Args.Target.HealingReceived);
			yield break;
		}
	}
}
