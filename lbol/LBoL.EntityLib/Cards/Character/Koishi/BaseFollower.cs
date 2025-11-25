using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Koishi
{
	[UsedImplicitly]
	public sealed class BaseFollower : Card
	{
		public override bool ShuffleToBottom
		{
			get
			{
				return true;
			}
		}
	}
}
