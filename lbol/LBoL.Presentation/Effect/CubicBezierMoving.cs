using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace LBoL.Presentation.Effect
{
	// Token: 0x020000FA RID: 250
	public class CubicBezierMoving : MonoBehaviour
	{
		// Token: 0x06000E10 RID: 3600 RVA: 0x00043468 File Offset: 0x00041668
		private Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
		{
			if (this._needSToP)
			{
				p2 = CameraController.ScenePositionToWorldPositionInUI(p2);
			}
			float num = 1f - t;
			float num2 = t * t;
			return num * num * p0 + 2f * num * t * p1 + num2 * p2;
		}

		// Token: 0x06000E11 RID: 3601 RVA: 0x000434BC File Offset: 0x000416BC
		public void Init(float totalTime, Vector3 cubic, Transform target, bool stp)
		{
			this._totalDuration = totalTime;
			this._startPoint = base.gameObject.transform.position;
			this._targetTransform = target;
			this._cubicPoint = cubic;
			this._init = true;
			this._needSToP = stp;
		}

		// Token: 0x06000E12 RID: 3602 RVA: 0x000434F8 File Offset: 0x000416F8
		private void Update()
		{
			if (!this._init)
			{
				return;
			}
			float num = this._curTime / this._totalDuration;
			Vector3 vector = this.CalculateCubicBezierPoint(num, this._startPoint, this._cubicPoint, this._targetTransform.position);
			base.gameObject.transform.position = vector;
			this._curTime += Time.deltaTime;
			if (this._curTime > this._totalDuration * 0.97f)
			{
				this.DestroySelf();
			}
		}

		// Token: 0x06000E13 RID: 3603 RVA: 0x00043578 File Offset: 0x00041778
		private void DestroySelf()
		{
			base.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).From(1f, true, false)
				.SetUpdate(true);
			Object.Destroy(base.gameObject, 2f);
			this._init = false;
		}

		// Token: 0x04000A85 RID: 2693
		private Vector3 _startPoint;

		// Token: 0x04000A86 RID: 2694
		private Vector3 _cubicPoint;

		// Token: 0x04000A87 RID: 2695
		private Transform _targetTransform;

		// Token: 0x04000A88 RID: 2696
		private bool _init;

		// Token: 0x04000A89 RID: 2697
		private bool _needSToP;

		// Token: 0x04000A8A RID: 2698
		private float _curTime;

		// Token: 0x04000A8B RID: 2699
		private float _totalDuration;
	}
}
