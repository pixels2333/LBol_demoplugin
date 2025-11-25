using System;
using UnityEngine;
using UnityEngine.UI;
namespace LBoL.Presentation.UI
{
	[RequireComponent(typeof(RawImage))]
	public sealed class RawImageUvTweener : MonoBehaviour
	{
		private void Awake()
		{
			this._rawImage = base.GetComponent<RawImage>();
		}
		private void Update()
		{
			Rect uvRect = this._rawImage.uvRect;
			uvRect.position += (this.timeScaleIndependent ? Time.unscaledDeltaTime : Time.deltaTime) * this.speed;
			this._rawImage.uvRect = uvRect;
		}
		[SerializeField]
		private Vector2 speed;
		[SerializeField]
		private bool timeScaleIndependent;
		private RawImage _rawImage;
	}
}
