using System;
using UnityEngine;
namespace LBoL.Presentation.UI.Panels
{
	public sealed class HintPayload
	{
		public string HintKey { get; set; }
		public RectTransform Target { get; set; }
		public RectTransform CopyedGameObject { get; set; }
		public float Delay { get; set; }
	}
}
