using System;
using System.Collections.Generic;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Character.Cirno;
using LBoL.EntityLib.Cards.Character.Marisa;
using LBoL.EntityLib.Cards.Character.Reimu;
using LBoL.EntityLib.Cards.Neutral.TwoColor;
using LBoL.EntityLib.Cards.Neutral.White;
namespace LBoL.EntityLib.Cards
{
	public abstract class LimitedStopTimeCard : Card
	{
		public bool Limited { get; set; } = true;
		protected override string GetBaseDescription()
		{
			if (!this.Limited)
			{
				return base.GetExtraDescription1;
			}
			return base.GetBaseDescription();
		}
		// Note: this type is marked as 'beforefieldinit'.
		static LimitedStopTimeCard()
		{
			List<Type> list = new List<Type>();
			list.Add(typeof(ReimuInstant));
			list.Add(typeof(BlazingStar));
			list.Add(typeof(InstantFreeze));
			list.Add(typeof(HuiyinRetain));
			list.Add(typeof(AyaExtraTurn));
			LimitedStopTimeCard.All = list;
		}
		public static readonly List<Type> All;
	}
}
