using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x02000396 RID: 918
	[UsedImplicitly]
	public sealed class LightSpeed : Card
	{
		// Token: 0x17000175 RID: 373
		// (get) Token: 0x06000D11 RID: 3345 RVA: 0x00018EDA File Offset: 0x000170DA
		public override bool Triggered
		{
			get
			{
				return base.Battle != null && base.Battle.TurnDiscardHistory.NotEmpty<Card>();
			}
		}

		// Token: 0x06000D12 RID: 3346 RVA: 0x00018EF6 File Offset: 0x000170F6
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.TriggeredAnyhow)
			{
				yield return new GainManaAction(base.Mana);
			}
			yield break;
		}
	}
}
