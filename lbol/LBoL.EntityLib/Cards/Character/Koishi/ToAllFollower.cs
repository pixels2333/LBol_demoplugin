using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Koishi;
namespace LBoL.EntityLib.Cards.Character.Koishi
{
	[UsedImplicitly]
	public sealed class ToAllFollower : Card
	{
		public override bool Triggered
		{
			get
			{
				return base.Battle != null && base.Battle.Player.HasStatusEffect<MoodEpiphany>();
			}
		}
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
			base.CardGuns = (base.TriggeredAnyhow ? this.Guns2 : this.Guns1);
		}
	}
}
