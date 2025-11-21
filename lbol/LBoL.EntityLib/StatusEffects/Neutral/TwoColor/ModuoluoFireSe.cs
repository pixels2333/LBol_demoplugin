using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.TwoColor
{
	// Token: 0x02000045 RID: 69
	public sealed class ModuoluoFireSe : StatusEffect
	{
		// Token: 0x060000D6 RID: 214 RVA: 0x0000386D File Offset: 0x00001A6D
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<ManaEventArgs>(base.Battle.ManaConsumed, new EventSequencedReactor<ManaEventArgs>(this.OnManaConsumed));
		}

		// Token: 0x060000D7 RID: 215 RVA: 0x0000388C File Offset: 0x00001A8C
		private IEnumerable<BattleAction> OnManaConsumed(ManaEventArgs args)
		{
			ManaGroup value = args.Value;
			if (value.Philosophy > 0)
			{
				base.NotifyActivating();
				int num = base.Level * value.Philosophy;
				string text = "秽火";
				if (num > 10)
				{
					text = "秽火B";
				}
				if (num > 20)
				{
					text = "秽火C";
				}
				yield return new DamageAction(base.Battle.Player, base.Battle.EnemyGroup.Alives, DamageInfo.Reaction((float)num, false), text, GunType.Single);
			}
			yield break;
		}
	}
}
