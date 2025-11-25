using System;
using System.Collections.Generic;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards
{
	public abstract class OptionCard : Card
	{
		public virtual IEnumerable<BattleAction> TakeEffectActions()
		{
			yield break;
		}
	}
}
