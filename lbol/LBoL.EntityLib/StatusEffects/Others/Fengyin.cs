using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using UnityEngine;

namespace LBoL.EntityLib.StatusEffects.Others
{
	// Token: 0x0200003A RID: 58
	[UsedImplicitly]
	public sealed class Fengyin : StatusEffect
	{
		// Token: 0x060000AC RID: 172 RVA: 0x0000334E File Offset: 0x0000154E
		protected override void OnAdded(Unit unit)
		{
			if (!(unit is PlayerUnit))
			{
				Debug.LogWarning("Fengyin is added to non-player unit " + unit.DebugName);
			}
		}

		// Token: 0x060000AD RID: 173 RVA: 0x0000336D File Offset: 0x0000156D
		public override bool ShouldPreventCardUsage(Card card)
		{
			return card.CardType == CardType.Attack;
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x060000AE RID: 174 RVA: 0x00003378 File Offset: 0x00001578
		public override string PreventCardUsageMessage
		{
			get
			{
				return "ErrorChat.CardFengyin".Localize(true);
			}
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x060000AF RID: 175 RVA: 0x00003385 File Offset: 0x00001585
		public override string UnitEffectName
		{
			get
			{
				return "FengyinLoop";
			}
		}
	}
}
