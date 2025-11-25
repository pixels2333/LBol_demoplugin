using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Marisa
{
	[UsedImplicitly]
	public sealed class CollectionDefense : Card
	{
		protected override int AdditionalBlock
		{
			get
			{
				if (base.Battle == null || this.IsUpgraded)
				{
					return 0;
				}
				return base.Battle.DrawZone.Count;
			}
		}
		protected override int AdditionalShield
		{
			get
			{
				if (base.Battle == null || !this.IsUpgraded)
				{
					return 0;
				}
				return base.Battle.DrawZone.Count;
			}
		}
	}
}
