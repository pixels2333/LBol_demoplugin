using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.Cards.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Character
{
	// Token: 0x02000249 RID: 585
	[UsedImplicitly]
	public sealed class Star : LightFairy
	{
		// Token: 0x1700010E RID: 270
		// (get) Token: 0x06000951 RID: 2385 RVA: 0x000141DF File Offset: 0x000123DF
		protected override int AttackTimes
		{
			get
			{
				return 2;
			}
		}

		// Token: 0x06000952 RID: 2386 RVA: 0x000141E2 File Offset: 0x000123E2
		protected override IEnumerable<BattleAction> LightActions()
		{
			yield return new EnemyMoveAction(this, base.LightMove, true);
			yield return PerformAction.Animation(this, "shoot3", 0f, null, 0f, -1);
			yield return new AddCardsToDiscardAction(Library.CreateCards<Xingguang>(2, false), AddCardsType.Normal);
			yield break;
		}

		// Token: 0x06000953 RID: 2387 RVA: 0x000141F2 File Offset: 0x000123F2
		protected override IEnumerable<BattleAction> SpellActions()
		{
			yield return PerformAction.Spell(this, "定位之星");
			yield return new EnemyMoveAction(this, base.SpellCard, true);
			yield return PerformAction.Animation(this, "shoot3", 0.3f, null, 0f, -1);
			yield return new ApplyStatusEffectAction<LockedOn>(base.Battle.Player, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, false);
			yield return PerformAction.Animation(base.Battle.Player, "Hit", 0.3f, null, 0f, -1);
			yield return PerformAction.Chat(this, "Chat.StarSpell".Localize(true), 2.5f, 0.2f, 0f, true);
			yield break;
		}
	}
}
