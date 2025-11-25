using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Misfortune;
namespace LBoL.EntityLib.Exhibits.Shining
{
	[UsedImplicitly]
	public sealed class Yuerang : ShiningExhibit
	{
		protected override void OnGain(PlayerUnit player)
		{
			base.OnGain(player);
			List<Card> list = new List<Card>();
			list.Add(Library.CreateCard<Yuebiao>());
			List<Card> list2 = list;
			base.GameRun.AddDeckCards(list2, false, null);
		}
	}
}
