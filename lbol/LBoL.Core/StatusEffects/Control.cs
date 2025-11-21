using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x02000092 RID: 146
	[UsedImplicitly]
	public sealed class Control : StatusEffect, IOpposing<ControlNegative>
	{
		// Token: 0x06000745 RID: 1861 RVA: 0x00015931 File Offset: 0x00013B31
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DollValueArgs>(base.Owner.DollValueGenerating, delegate(DollValueArgs args)
			{
				args.Value += base.Level;
				args.AddModifier(this);
			});
		}

		// Token: 0x06000746 RID: 1862 RVA: 0x00015950 File Offset: 0x00013B50
		public OpposeResult Oppose(ControlNegative other)
		{
			if (base.Level < other.Level)
			{
				other.Level -= base.Level;
				return OpposeResult.KeepOther;
			}
			if (base.Level == other.Level)
			{
				return OpposeResult.Neutralize;
			}
			base.Level -= other.Level;
			return OpposeResult.KeepSelf;
		}
	}
}
