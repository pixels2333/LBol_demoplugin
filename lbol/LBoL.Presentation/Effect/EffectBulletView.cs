using System;
using UnityEngine;
namespace LBoL.Presentation.Effect
{
	public class EffectBulletView : MonoBehaviour
	{
		public EffectBullet EffectBullet { get; set; }
		public void SetEffectBullet(EffectBullet eb)
		{
			this.EffectBullet = eb;
			eb.EffectBulletView = this;
			eb.Calculation();
			this.Refresh();
			base.name = eb.EffectName + "(EffectBullet)";
		}
		private void LateUpdate()
		{
			this.Refresh();
		}
		protected virtual void Refresh()
		{
			Transform transform = base.transform;
			transform.localPosition = this.EffectBullet.Position;
			transform.localRotation = this.EffectBullet.Rotation;
		}
		public void Die()
		{
			Object.Destroy(base.gameObject);
		}
		public Transform widgetRoot;
	}
}
