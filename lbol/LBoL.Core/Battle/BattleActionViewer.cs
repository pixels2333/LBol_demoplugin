using System;
using System.Collections;

namespace LBoL.Core.Battle
{
	// Token: 0x0200013D RID: 317
	// (Invoke) Token: 0x06000C09 RID: 3081
	public delegate IEnumerator BattleActionViewer<in TAction>(TAction action) where TAction : BattleAction;
}
