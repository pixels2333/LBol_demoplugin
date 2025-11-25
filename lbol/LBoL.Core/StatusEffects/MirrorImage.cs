using System;
using JetBrains.Annotations;
namespace LBoL.Core.StatusEffects
{
	[UsedImplicitly]
	public sealed class MirrorImage : StatusEffect
	{
		public override string UnitEffectName
		{
			get
			{
				return "MirrorImage";
			}
		}
		public const float LootRate = 0.25f;
	}
}
