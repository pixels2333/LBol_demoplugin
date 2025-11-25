using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.Black
{
	[UsedImplicitly]
	public sealed class CloseEyeShoot : Card
	{
		protected override void SetGuns()
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
		}
	}
}
