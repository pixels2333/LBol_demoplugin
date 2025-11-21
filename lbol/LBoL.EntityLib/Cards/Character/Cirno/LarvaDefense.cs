using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.Cards.Character.Cirno.Friend;
using LBoL.EntityLib.StatusEffects.Others;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004C9 RID: 1225
	[UsedImplicitly]
	public sealed class LarvaDefense : Card
	{
		// Token: 0x170001CB RID: 459
		// (get) Token: 0x06001044 RID: 4164 RVA: 0x0001CD32 File Offset: 0x0001AF32
		public override bool Triggered
		{
			get
			{
				if (base.Battle != null)
				{
					return Enumerable.Any<Card>(base.Battle.HandZone, (Card card) => card is LarvaFriend && card.Summoned);
				}
				return false;
			}
		}

		// Token: 0x06001045 RID: 4165 RVA: 0x0001CD6D File Offset: 0x0001AF6D
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			foreach (BattleAction battleAction in base.DebuffAction<Weak>(base.Battle.AllAliveEnemies, 0, base.Value1, 0, 0, true, 0.1f))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			if (base.TriggeredAnyhow)
			{
				foreach (BattleAction battleAction2 in base.DebuffAction<Poison>(base.Battle.AllAliveEnemies, base.Value2, 0, 0, 0, true, 0.1f))
				{
					yield return battleAction2;
				}
				enumerator = null;
			}
			yield break;
			yield break;
		}
	}
}
