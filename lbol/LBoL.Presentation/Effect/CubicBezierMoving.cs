using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
namespace LBoL.Presentation.Effect
{
	public class CubicBezierMoving : MonoBehaviour
	{
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
		public void Init(float totalTime, Vector3 cubic, Transform target, bool stp)
		{
			this._totalDuration = totalTime;
			this._startPoint = base.gameObject.transform.position;
			this._targetTransform = target;
			this._cubicPoint = cubic;
			this._init = true;
			this._needSToP = stp;
		}
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
		private void DestroySelf()
		{
			base.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).From(1f, true, false)
				.SetUpdate(true);
			Object.Destroy(base.gameObject, 2f);
			this._init = false;
		}
		private Vector3 _startPoint;
		private Vector3 _cubicPoint;
		private Transform _targetTransform;
		private bool _init;
		private bool _needSToP;
		private float _curTime;
		private float _totalDuration;
	}
}
