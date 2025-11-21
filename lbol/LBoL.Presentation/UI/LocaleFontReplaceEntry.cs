using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LBoL.Presentation.UI
{
	// Token: 0x02000023 RID: 35
	[Serializable]
	public class LocaleFontReplaceEntry
	{
		// Token: 0x04000156 RID: 342
		public string name;

		// Token: 0x04000157 RID: 343
		public TMP_FontAsset font;

		// Token: 0x04000158 RID: 344
		public Material material;

		// Token: 0x04000159 RID: 345
		public List<LocaleFontReplacePair> pairs;
	}
}
