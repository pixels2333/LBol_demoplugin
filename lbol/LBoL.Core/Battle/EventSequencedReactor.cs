using System;
using System.Collections.Generic;

namespace LBoL.Core.Battle
{
	// Token: 0x0200014A RID: 330
	// (Invoke) Token: 0x06000D3B RID: 3387
	public delegate IEnumerable<BattleAction> EventSequencedReactor<in TEventArgs>(TEventArgs args) where TEventArgs : GameEventArgs;
}
