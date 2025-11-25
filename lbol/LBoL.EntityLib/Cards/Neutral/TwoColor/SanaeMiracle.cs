using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	[UsedImplicitly]
	public sealed class SanaeMiracle : Card
	{
		public override bool Triggered
		{
			get
			{
				return this.IsForceCost;
			}
		}
		public override bool IsForceCost
		{
			get
			{
				return base.Battle != null && base.Battle.DrawZone.Count == 0;
			}
		}
		protected override void SetGuns()
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
		}
	}
}
