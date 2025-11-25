using System;
using JetBrains.Annotations;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.EnemyUnits.Normal.Maoyus
{
	[UsedImplicitly]
	public sealed class MaoyuBlue : MaoyuOrigin
	{
		protected override Type DebuffType
		{
			get
			{
				return typeof(Weak);
			}
		}
	}
}
