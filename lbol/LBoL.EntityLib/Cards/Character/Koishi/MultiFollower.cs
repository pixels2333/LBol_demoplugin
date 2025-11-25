using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Koishi
{
	[UsedImplicitly]
	public sealed class MultiFollower : Card
	{
		public override bool ShuffleToBottom
		{
			get
			{
				return true;
			}
		}
		protected override void SetGuns()
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, false);
		}
	}
}
