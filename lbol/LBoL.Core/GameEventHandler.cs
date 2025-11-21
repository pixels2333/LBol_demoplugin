using System;

namespace LBoL.Core
{
	// Token: 0x02000016 RID: 22
	// (Invoke) Token: 0x060000CB RID: 203
	public delegate void GameEventHandler<in T>(T args) where T : GameEventArgs;
}
