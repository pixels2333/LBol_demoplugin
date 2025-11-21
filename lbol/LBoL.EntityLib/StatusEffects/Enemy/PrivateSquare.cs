using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000BE RID: 190
	[UsedImplicitly]
	public sealed class PrivateSquare : StatusEffect
	{
		// Token: 0x0600029A RID: 666 RVA: 0x0000736C File Offset: 0x0000556C
		protected override void OnAdded(Unit unit)
		{
			base.Count = base.Limit;
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardPlayed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardPlayed));
		}

		// Token: 0x0600029B RID: 667 RVA: 0x000073BF File Offset: 0x000055BF
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Battle.BattleShouldEnd && base.Battle.Player.IsInTurn)
			{
				yield break;
			}
			if (args.Card is SakuyaLock)
			{
				base.NotifyActivating();
				string text = ((base.Level >= 15) ? "ESakuyaSE1" : "ESakuyaSE");
				DamageAction da = new DamageAction(base.Owner, base.Battle.Player, DamageInfo.Attack((float)base.Level, false), text, GunType.Single);
				yield return da;
				yield return new StatisticalTotalDamageAction(new DamageAction[] { da });
				da = null;
			}
			else if (base.Count > 1)
			{
				int num = base.Count - 1;
				base.Count = num;
				if (base.Count == 1)
				{
					base.Highlight = true;
				}
			}
			else
			{
				base.Highlight = false;
				base.Count = base.Limit;
				base.NotifyActivating();
				string text2 = ((base.Level >= 15) ? "ESakuyaSE1" : "ESakuyaSE");
				DamageAction da = new DamageAction(base.Owner, base.Battle.Player, DamageInfo.Attack((float)base.Level, false), text2, GunType.Single);
				yield return da;
				yield return new StatisticalTotalDamageAction(new DamageAction[] { da });
				da = null;
			}
			yield break;
		}

		// Token: 0x0600029C RID: 668 RVA: 0x000073D6 File Offset: 0x000055D6
		private IEnumerable<BattleAction> OnCardPlayed(CardUsingEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (args.Card is SakuyaLock)
			{
				base.NotifyActivating();
				string text = ((base.Level >= 15) ? "ESakuyaSE1" : "ESakuyaSE");
				DamageAction da = new DamageAction(base.Owner, base.Battle.Player, DamageInfo.Attack((float)base.Level, false), text, GunType.Single);
				yield return da;
				yield return new StatisticalTotalDamageAction(new DamageAction[] { da });
				da = null;
			}
			yield break;
		}
	}
}
