using System;
using UnityEngine;
namespace LBoL.Presentation.Effect
{
	public class EffectUIBulletView : EffectBulletView
	{
		protected override void Refresh()
		{
			RectTransform component = base.GetComponent<RectTransform>();
			component.position = base.EffectBullet.Position;
			component.localRotation = base.EffectBullet.Rotation;
		}
	}
}
