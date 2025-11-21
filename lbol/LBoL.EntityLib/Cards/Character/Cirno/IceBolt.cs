using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004C2 RID: 1218
	[UsedImplicitly]
	public sealed class IceBolt : Card
	{
		// Token: 0x06001029 RID: 4137 RVA: 0x0001C9E6 File Offset: 0x0001ABE6
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<DamageEventArgs>(base.Battle.Player.DamageDealt, new GameEventHandler<DamageEventArgs>(this.OnPlayerDamageDealt), (GameEventPriority)0);
			this._coldingEnemies = new List<Unit>();
		}

		// Token: 0x0600102A RID: 4138 RVA: 0x0001CA18 File Offset: 0x0001AC18
		private void OnPlayerDamageDealt(DamageEventArgs args)
		{
			if (args.ActionSource == this && !args.DamageInfo.IsGrazed)
			{
				Unit target = args.Target;
				this._coldingEnemies.Add(target);
			}
		}

		// Token: 0x0600102B RID: 4139 RVA: 0x0001CA51 File Offset: 0x0001AC51
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd || this._coldingEnemies.Count == 0)
			{
				yield break;
			}
			foreach (Unit unit in this._coldingEnemies)
			{
				if (unit.IsAlive)
				{
					yield return base.DebuffAction<Cold>(unit, 0, 0, 0, 0, true, 0.03f);
				}
			}
			List<Unit>.Enumerator enumerator = default(List<Unit>.Enumerator);
			this._coldingEnemies.Clear();
			yield break;
			yield break;
		}

		// Token: 0x0400010E RID: 270
		private List<Unit> _coldingEnemies;
	}
}
