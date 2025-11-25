using System;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	[ExhibitInfo(WeighterType = typeof(JingboliJing.JingboliJingWeighter))]
	public sealed class JingboliJing : Exhibit
	{
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<ScryEventArgs>(base.Battle.Scrying, delegate(ScryEventArgs args)
			{
				args.ScryInfo = args.ScryInfo.IncreasedBy(base.Value1);
				args.AddModifier(this);
			});
		}
		private class JingboliJingWeighter : IExhibitWeighter
		{
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)(Enumerable.Any<Card>(gameRun.BaseDeck, (Card card) => card.ConfigRelativeKeywords.HasFlag(Keyword.Scry)) ? 1 : 0);
			}
		}
	}
}
