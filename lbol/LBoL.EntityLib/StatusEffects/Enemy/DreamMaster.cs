using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Enemy
{
	[UsedImplicitly]
	public sealed class DreamMaster : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			this.React(PerformAction.Chat(base.Owner, "Chat.DoremyMaster".Localize(true), 3f, 0f, 0f, true));
		}
	}
}
