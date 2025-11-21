using System;
using System.Collections;
using LBoL.Core.Battle;

namespace LBoL.Core
{
	// Token: 0x02000052 RID: 82
	// (Invoke) Token: 0x06000372 RID: 882
	public delegate IEnumerator InteractionViewer<in TInteraction>(TInteraction interaction) where TInteraction : Interaction;
}
