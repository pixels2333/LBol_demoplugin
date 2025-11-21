using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x02000128 RID: 296
	[UsedImplicitly]
	public sealed class Ganggenier : ShiningExhibit
	{
		// Token: 0x0600040F RID: 1039 RVA: 0x0000B17A File Offset: 0x0000937A
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<DamageDealingEventArgs>(base.Owner.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnDamageDealing));
		}

		// Token: 0x06000410 RID: 1040 RVA: 0x0000B19C File Offset: 0x0000939C
		private void OnDamageDealing(DamageDealingEventArgs args)
		{
			Card card = args.ActionSource as Card;
			if (card != null && card.Config.Type == CardType.Attack)
			{
				DamageInfo damageInfo = args.DamageInfo;
				damageInfo.IsAccuracy = true;
				args.DamageInfo = damageInfo;
				args.AddModifier(this);
			}
		}
	}
}
