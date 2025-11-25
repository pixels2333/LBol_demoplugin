using System;
using JetBrains.Annotations;
using LBoL.Core.Units;
namespace LBoL.Core.Intentions
{
	[UsedImplicitly]
	public sealed class DefendIntention : Intention
	{
		public override IntentionType Type
		{
			get
			{
				return IntentionType.Defend;
			}
		}
	}
}
