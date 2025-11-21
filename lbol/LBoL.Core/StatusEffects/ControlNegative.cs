using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x02000093 RID: 147
	[UsedImplicitly]
	public sealed class ControlNegative : StatusEffect, IOpposing<Control>
	{
		// Token: 0x06000749 RID: 1865 RVA: 0x000159C8 File Offset: 0x00013BC8
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DollValueArgs>(base.Owner.DollValueGenerating, delegate(DollValueArgs args)
			{
				args.Value -= base.Level;
				args.AddModifier(this);
			});
		}

		// Token: 0x0600074A RID: 1866 RVA: 0x000159E8 File Offset: 0x00013BE8
		public OpposeResult Oppose(Control other)
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
