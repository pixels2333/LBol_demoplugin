using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x02000403 RID: 1027
	[UsedImplicitly]
	public sealed class Wunvyu : YinyangCardBase
	{
		// Token: 0x06000E36 RID: 3638 RVA: 0x0001A3E6 File Offset: 0x000185E6
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<CardEventArgs>(base.Battle.CardDrawn, new GameEventHandler<CardEventArgs>(this.OnCardDrawn), (GameEventPriority)0);
		}

		// Token: 0x06000E37 RID: 3639 RVA: 0x0001A406 File Offset: 0x00018606
		private void OnCardDrawn(CardEventArgs args)
		{
			if (base.Zone == CardZone.Hand && args.Cause != ActionCause.TurnStart)
			{
				base.DeltaDamage += base.Value2;
				this.NotifyChanged();
			}
		}

		// Token: 0x06000E38 RID: 3640 RVA: 0x0001A434 File Offset: 0x00018634
		protected override void SetGuns()
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
		}
	}
}
