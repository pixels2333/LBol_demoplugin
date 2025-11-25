using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Reimu
{
	[UsedImplicitly]
	public sealed class Fengmozhen : Card
	{
		public bool IsPhantom { get; set; }
		protected override void SetGuns()
		{
			base.CardGuns = new Guns(this.IsPhantom ? (this.IsUpgraded ? "幻影针" : "幻影针B") : base.GunName, 1, true);
		}
	}
}
