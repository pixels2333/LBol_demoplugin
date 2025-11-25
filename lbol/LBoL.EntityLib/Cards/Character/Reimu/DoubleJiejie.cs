using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Reimu
{
	[UsedImplicitly]
	public sealed class DoubleJiejie : Card
	{
		private Guns Guns1
		{
			get
			{
				return new Guns(base.Config.GunName);
			}
		}
		private Guns Guns2
		{
			get
			{
				return new Guns(new string[]
				{
					base.Config.GunName,
					base.Config.GunNameBurst
				});
			}
		}
		protected override void SetGuns()
		{
			base.CardGuns = ((base.Battle.Player.Shield > 0) ? this.Guns2 : this.Guns1);
		}
	}
}
