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
	// Token: 0x02000259 RID: 601
	public abstract class LimitedStopTimeCard : Card
	{
		// Token: 0x17000127 RID: 295
		// (get) Token: 0x060009BB RID: 2491 RVA: 0x00014DD5 File Offset: 0x00012FD5
		// (set) Token: 0x060009BC RID: 2492 RVA: 0x00014DDD File Offset: 0x00012FDD
		public bool Limited { get; set; } = true;

		// Token: 0x060009BD RID: 2493 RVA: 0x00014DE6 File Offset: 0x00012FE6
		protected override string GetBaseDescription()
		{
			if (!this.Limited)
			{
				return base.GetExtraDescription1;
			}
			return base.GetBaseDescription();
		}

		// Token: 0x060009BF RID: 2495 RVA: 0x00014E0C File Offset: 0x0001300C
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

		// Token: 0x040000E9 RID: 233
		public static readonly List<Type> All;
	}
}
