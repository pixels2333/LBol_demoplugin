using System;
using JetBrains.Annotations;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000C8 RID: 200
	[UsedImplicitly]
	public sealed class SijiZui : StatusEffect
	{
		// Token: 0x060002B5 RID: 693 RVA: 0x00007670 File Offset: 0x00005870
		protected override string GetBaseDescription()
		{
			int level = base.Level;
			string text;
			if (level < 3)
			{
				if (level != 2)
				{
					text = base.GetBaseDescription();
				}
				else
				{
					text = base.ExtraDescription;
				}
			}
			else
			{
				text = base.ExtraDescription2;
			}
			return text;
		}
	}
}
