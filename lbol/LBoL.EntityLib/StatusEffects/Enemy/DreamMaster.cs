using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x02000097 RID: 151
	[UsedImplicitly]
	public sealed class DreamMaster : StatusEffect
	{
		// Token: 0x0600021F RID: 543 RVA: 0x00006694 File Offset: 0x00004894
		protected override void OnAdded(Unit unit)
		{
			this.React(PerformAction.Chat(base.Owner, "Chat.DoremyMaster".Localize(true), 3f, 0f, 0f, true));
		}
	}
}
