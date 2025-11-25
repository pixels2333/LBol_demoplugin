using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;
using UnityEngine.UI;
namespace LBoL.Presentation.Effect
{
	public class ExileCoverEffect : MonoBehaviour
	{
		public GameObject RootObject { get; set; }
		private void Awake()
		{
			this.coverOriginRect = this.cover.rectTransform.sizeDelta;
			this.edgeOriginRect = this.edge.rectTransform.sizeDelta;
			this._coverMaterial = new Material(this.cover.material);
			this.cover.material = this._coverMaterial;
		}
		public void Exile()
		{
			this.cover.rectTransform.sizeDelta = this.coverOriginRect;
			this.edge.rectTransform.sizeDelta = this.edgeOriginRect;
			this._coverMaterial.SetFloat(ExileCoverEffect.DissolveProgress, 1f);
			this.edge.color = new Color(1f, 1f, 1f, 0f);
			this._coverMaterial.SetFloat(ExileCoverEffect.Alpha, 1f);
			DOTween.Sequence("Exile").Append(DOTween.To(new DOSetter<float>(this.ExileGrowth), 0f, 1f, this.growthTime).SetEase(this.growthEase)).AppendCallback(delegate
			{
				foreach (GameObject gameObject in this.CloseObjects)
				{
					gameObject.SetActive(false);
				}
				this.flare.Play();
				this.particles.Play();
			})
				.Append(DOTween.To(new DOSetter<float>(this.ExileFade), 0f, 1f, this.fadeTime).SetEase(this.fadeEase))
				.OnComplete(delegate
				{
					Object.Destroy(this.RootObject, 2f);
				})
				.SetUpdate(true)
				.SetAutoKill(true);
		}
		private void ExileGrowth(float t)
		{
			this._coverMaterial.SetFloat(ExileCoverEffect.DissolveProgress, 1f - t);
			this.edge.color = new Color(1f, 1f, 1f, t);
		}
		private void ExileFade(float t)
		{
			this.cover.rectTransform.sizeDelta = Vector2.Lerp(this.coverOriginRect, new Vector2(this.coverOriginRect.x * this.xRatio, this.coverOriginRect.y * this.yRatio), t * this.coverOffset);
			this.edge.rectTransform.sizeDelta = Vector2.Lerp(this.edgeOriginRect, new Vector2(this.edgeOriginRect.x * this.xRatio, this.edgeOriginRect.y * this.yRatio), t);
		}
		public Image cover;
		public Image edge;
		public ParticleSystem flare;
		public ParticleSystem particles;
		public float growthTime = 0.5f;
		public float fadeTime = 0.5f;
		public float coverOffset = 1.01f;
		public float xRatio;
		public float yRatio;
		public Ease growthEase;
		public Ease fadeEase;
		private Vector2 coverOriginRect;
		private Vector2 edgeOriginRect;
		private static readonly int DissolveProgress = Shader.PropertyToID("_DissolveProgress");
		private static readonly int Alpha = Shader.PropertyToID("_Alpha");
		public readonly List<GameObject> CloseObjects = new List<GameObject>();
		private Material _coverMaterial;
	}
}
