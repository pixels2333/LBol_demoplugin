using System;
using JetBrains.Annotations;
using LBoL.Core.Units;
namespace LBoL.Core.Intentions
{
	[UsedImplicitly]
	public sealed class SpawnDroneIntention : Intention
	{
		public override IntentionType Type
		{
			get
			{
				return IntentionType.SpawnDrone;
			}
		}
	}
}
