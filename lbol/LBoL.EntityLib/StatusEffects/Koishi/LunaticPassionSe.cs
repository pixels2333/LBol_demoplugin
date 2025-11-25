using System;
using JetBrains.Annotations;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.StatusEffects.Koishi
{
	[UsedImplicitly]
	public sealed class LunaticPassionSe : StatusEffect
	{
		[UsedImplicitly]
		public int Percentage
		{
			get
			{
				return 150;
			}
		}
	}
}
