using System;
using JetBrains.Annotations;
namespace LBoL.EntityLib.EnemyUnits.Normal.Ravens
{
	[UsedImplicitly]
	public sealed class RavenWen3 : Raven
	{
		protected override bool HasGraze
		{
			get
			{
				return false;
			}
		}
	}
}
