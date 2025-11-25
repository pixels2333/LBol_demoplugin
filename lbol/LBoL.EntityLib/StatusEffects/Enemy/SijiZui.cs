using System;
using JetBrains.Annotations;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.StatusEffects.Enemy
{
	[UsedImplicitly]
	public sealed class SijiZui : StatusEffect
	{
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
