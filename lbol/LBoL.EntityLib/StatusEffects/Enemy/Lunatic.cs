using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Normal;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000B9 RID: 185
	public sealed class Lunatic : StatusEffect
	{
		// Token: 0x17000040 RID: 64
		// (get) Token: 0x06000287 RID: 647 RVA: 0x000071CB File Offset: 0x000053CB
		// (set) Token: 0x06000288 RID: 648 RVA: 0x000071D3 File Offset: 0x000053D3
		[UsedImplicitly]
		public ManaColor Color { get; set; } = ManaColor.Colorless;

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x06000289 RID: 649 RVA: 0x000071DC File Offset: 0x000053DC
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Single(this.Color);
			}
		}

		// Token: 0x0600028A RID: 650 RVA: 0x000071EC File Offset: 0x000053EC
		protected override void OnAdded(Unit unit)
		{
			ManaColor manaColor;
			if (!(unit is BlackFairy))
			{
				if (!(unit is WhiteFairy))
				{
					manaColor = ManaColor.Colorless;
				}
				else
				{
					manaColor = ManaColor.White;
				}
			}
			else
			{
				manaColor = ManaColor.Black;
			}
			this.Color = manaColor;
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x0600028B RID: 651 RVA: 0x0000723E File Offset: 0x0000543E
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			yield return new GainManaAction(this.Mana);
			yield break;
		}
	}
}
