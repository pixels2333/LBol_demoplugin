using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Adventure
{
	// Token: 0x020001BD RID: 445
	[UsedImplicitly]
	public sealed class FirstAidBook : Exhibit
	{
		// Token: 0x0600066A RID: 1642 RVA: 0x0000EC0C File Offset: 0x0000CE0C
		protected override void OnAdded(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.RemoveBadCardForbidden + 1;
			gameRun.RemoveBadCardForbidden = num;
		}

		// Token: 0x0600066B RID: 1643 RVA: 0x0000EC30 File Offset: 0x0000CE30
		protected override void OnRemoved(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.RemoveBadCardForbidden - 1;
			gameRun.RemoveBadCardForbidden = num;
			if (base.Battle != null && base.GameRun.RemoveBadCardForbidden <= 0)
			{
				foreach (Card card in base.Battle.EnumerateAllCards())
				{
					CardType cardType = card.CardType;
					if ((cardType == CardType.Misfortune || cardType == CardType.Status) && (card.Config.Keywords & Keyword.Forbidden) != Keyword.None)
					{
						card.IsForbidden = true;
					}
				}
			}
		}
	}
}
