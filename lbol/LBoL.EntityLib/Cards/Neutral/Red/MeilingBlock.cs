using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.Red
{
	[UsedImplicitly]
	public sealed class MeilingBlock : Card
	{
		protected override int AdditionalBlock
		{
			get
			{
				if (base.Battle != null)
				{
					return this.PlayerFirepowerPositive;
				}
				return 0;
			}
		}
		protected override int AdditionalShield
		{
			get
			{
				if (base.Battle != null)
				{
					return this.PlayerFirepowerPositive;
				}
				return 0;
			}
		}
		private int PlayerFirepowerPositive
		{
			get
			{
				return Math.Max(0, base.Battle.Player.TotalFirepower);
			}
		}
	}
}
