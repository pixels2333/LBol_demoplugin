using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;

namespace LBoL.EntityLib.Exhibits.Mythic
{
	// Token: 0x0200014F RID: 335
	[UsedImplicitly]
	public sealed class FoyushiBo : MythicExhibit
	{
		// Token: 0x06000491 RID: 1169 RVA: 0x0000BE52 File Offset: 0x0000A052
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<CardUsingEventArgs>(base.Battle.CardUsing, new GameEventHandler<CardUsingEventArgs>(this.OnCardUsing));
		}

		// Token: 0x06000492 RID: 1170 RVA: 0x0000BE71 File Offset: 0x0000A071
		private void OnCardUsing(CardUsingEventArgs args)
		{
			if (args.Card.CardType == CardType.Defense)
			{
				base.NotifyActivating();
				args.PlayTwice = true;
				args.AddModifier(this);
			}
		}
	}
}
