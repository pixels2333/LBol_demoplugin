using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Adventure
{
	[UsedImplicitly]
	public sealed class FirstAidBook : Exhibit
	{
		protected override void OnAdded(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.RemoveBadCardForbidden + 1;
			gameRun.RemoveBadCardForbidden = num;
		}
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
