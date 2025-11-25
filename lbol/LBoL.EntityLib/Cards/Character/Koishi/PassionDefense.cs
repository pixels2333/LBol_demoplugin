using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Koishi;
namespace LBoL.EntityLib.Cards.Character.Koishi
{
	[UsedImplicitly]
	public sealed class PassionDefense : Card
	{
		public override bool Triggered
		{
			get
			{
				return base.Battle != null && base.Battle.Player.HasStatusEffect<MoodPassion>();
			}
		}
		protected override int AdditionalBlock
		{
			get
			{
				if (base.Battle == null || !base.Battle.Player.HasStatusEffect<MoodPassion>())
				{
					return 0;
				}
				return base.Value1;
			}
		}
	}
}
