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

namespace LBoL.EntityLib.EnemyUnits.Character
{
	// Token: 0x02000241 RID: 577
	[UsedImplicitly]
	public sealed class Luna : LightFairy
	{
		// Token: 0x060008F5 RID: 2293 RVA: 0x0001360A File Offset: 0x0001180A
		protected override IEnumerable<BattleAction> LightActions()
		{
			yield return new EnemyMoveAction(this, base.LightMove, true);
			yield return PerformAction.Animation(this, "shoot3", 0.5f, null, 0f, -1);
			yield return new AddCardsToDiscardAction(new Card[] { Library.CreateCard<Yueguang>() });
			yield break;
		}

		// Token: 0x060008F6 RID: 2294 RVA: 0x0001361A File Offset: 0x0001181A
		protected override IEnumerable<BattleAction> SpellActions()
		{
			yield return PerformAction.Spell(this, "寂静之月");
			yield return new EnemyMoveAction(this, base.SpellCard, true);
			bool anime = true;
			foreach (EnemyUnit enemy in base.AllAliveEnemies)
			{
				Unit unit = enemy;
				int? num = new int?(base.Count1);
				bool flag = enemy.RootIndex <= base.RootIndex;
				yield return new ApplyStatusEffectAction<Graze>(unit, num, default(int?), default(int?), default(int?), 0f, flag);
				yield return new CastBlockShieldAction(this, enemy, 0, base.Defend, BlockShieldType.Normal, anime);
				anime = false;
				enemy = null;
			}
			IEnumerator<EnemyUnit> enumerator = null;
			yield return PerformAction.Chat(this, "Chat.LunaSpell".Localize(true), 2.5f, 0.2f, 0f, true);
			yield break;
			yield break;
		}
	}
}
