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
	[UsedImplicitly]
	public sealed class Fengyin : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			if (!(unit is PlayerUnit))
			{
				Debug.LogWarning("Fengyin is added to non-player unit " + unit.DebugName);
			}
		}
		public override bool ShouldPreventCardUsage(Card card)
		{
			return card.CardType == CardType.Attack;
		}
		public override string PreventCardUsageMessage
		{
			get
			{
				return "ErrorChat.CardFengyin".Localize(true);
			}
		}
		public override string UnitEffectName
		{
			get
			{
				return "FengyinLoop";
			}
		}
	}
}
