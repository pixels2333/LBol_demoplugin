using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace LBoL.Presentation.UI
{
	[Serializable]
	public class LocaleFontReplaceEntry
	{
		public string name;
		public TMP_FontAsset font;
		public Material material;
		public List<LocaleFontReplacePair> pairs;
	}
}
