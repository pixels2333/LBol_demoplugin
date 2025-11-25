using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Character.Reimu;
namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	[UsedImplicitly]
	public sealed class FengleiCard : YinyangCardBase
	{
		protected override void SetGuns()
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
		}
	}
}
