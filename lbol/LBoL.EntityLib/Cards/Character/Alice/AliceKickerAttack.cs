using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Alice
{
	[UsedImplicitly]
	public sealed class AliceKickerAttack : Card
	{
		protected override void SetGuns()
		{
			base.CardGuns = (base.KickerPlaying ? new Guns(base.GunName, 2, true) : new Guns(base.GunName));
		}
	}
}
