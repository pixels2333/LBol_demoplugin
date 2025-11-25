using System;
using JetBrains.Annotations;
using LBoL.Core.Units;
namespace LBoL.Core.Intentions
{
	[UsedImplicitly]
	public sealed class CountDownIntention : Intention
	{
		public override IntentionType Type
		{
			get
			{
				return IntentionType.CountDown;
			}
		}
		public int Counter { get; internal set; }
	}
}
