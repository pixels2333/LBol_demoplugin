using System;
using LBoL.Base.Extensions;
using LBoL.Core;
using UnityEngine;
namespace LBoL.Presentation.Effect
{
	public abstract class EffectBullet
	{
		public EffectBulletView EffectBulletView { get; set; }
		public Vector3 Position { get; set; }
		public Quaternion Rotation { get; set; }
		public bool Active { get; set; }
		public float Time { get; set; }
		public bool FirstTickFlag { get; set; } = true;
		public string EffectName { get; set; }
		public void Tick()
		{
			if (this.FirstTickFlag)
			{
				this.FirstTick();
			}
			this.Time += 0.016666668f;
			this.Calculation();
		}
		public virtual void FirstTick()
		{
			if (!this.EffectName.IsNullOrEmpty())
			{
				EffectManager.CreateEffect(this.EffectName, this.EffectBulletView.widgetRoot, 0f, new float?(0f), true, true);
			}
			this.FirstTickFlag = false;
		}
		public virtual void Calculation()
		{
		}
		protected virtual void Die()
		{
			this.Active = false;
			Singleton<EffectManager>.Instance.Remove(this);
			this.EffectBulletView.Die();
		}
	}
}
