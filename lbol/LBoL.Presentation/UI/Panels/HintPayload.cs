using System;
using UnityEngine;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x0200009B RID: 155
	public sealed class HintPayload
	{
		// Token: 0x17000157 RID: 343
		// (get) Token: 0x06000816 RID: 2070 RVA: 0x000263C4 File Offset: 0x000245C4
		// (set) Token: 0x06000817 RID: 2071 RVA: 0x000263CC File Offset: 0x000245CC
		public string HintKey { get; set; }

		// Token: 0x17000158 RID: 344
		// (get) Token: 0x06000818 RID: 2072 RVA: 0x000263D5 File Offset: 0x000245D5
		// (set) Token: 0x06000819 RID: 2073 RVA: 0x000263DD File Offset: 0x000245DD
		public RectTransform Target { get; set; }

		// Token: 0x17000159 RID: 345
		// (get) Token: 0x0600081A RID: 2074 RVA: 0x000263E6 File Offset: 0x000245E6
		// (set) Token: 0x0600081B RID: 2075 RVA: 0x000263EE File Offset: 0x000245EE
		public RectTransform CopyedGameObject { get; set; }

		// Token: 0x1700015A RID: 346
		// (get) Token: 0x0600081C RID: 2076 RVA: 0x000263F7 File Offset: 0x000245F7
		// (set) Token: 0x0600081D RID: 2077 RVA: 0x000263FF File Offset: 0x000245FF
		public float Delay { get; set; }
	}
}
