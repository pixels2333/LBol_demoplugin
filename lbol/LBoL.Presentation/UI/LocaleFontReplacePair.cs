using System;
using LBoL.Core;
using TMPro;
using UnityEngine;
namespace LBoL.Presentation.UI
{
	[Serializable]
	public class LocaleFontReplacePair
	{
		public Locale locale;
		public float resize;
		public TMP_FontAsset font;
		public Material material;
	}
}
