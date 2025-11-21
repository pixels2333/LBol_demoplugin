using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Misfortune;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x02000144 RID: 324
	[UsedImplicitly]
	public sealed class Yuerang : ShiningExhibit
	{
		// Token: 0x06000473 RID: 1139 RVA: 0x0000BC94 File Offset: 0x00009E94
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
