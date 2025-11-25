using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Reimu
{
	[UsedImplicitly]
	public sealed class JiejieAttack : Card
	{
		private string Gun1
		{
			get
			{
				if (!this.IsUpgraded)
				{
					return "结界猛击";
				}
				return "结界猛击B";
			}
		}
		private string Gun2
		{
			get
			{
				if (!this.IsUpgraded)
				{
					return "结界猛击2";
				}
				return "结界猛击2B";
			}
		}
		[UsedImplicitly]
		public int ShieldDamage
		{
			get
			{
				if (base.Battle != null)
				{
					return base.Battle.Player.Shield;
				}
				return 0;
			}
		}
		protected override int AdditionalDamage
		{
			get
			{
				return this.ShieldDamage;
			}
		}
		protected override void SetGuns()
		{
			base.CardGuns = ((this.ShieldDamage > 0) ? new Guns(this.Gun2) : new Guns(this.Gun1));
		}
	}
}
