using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;
using UnityEngine.UI;
namespace LBoL.Presentation.Effect
{
	public class RemoveCoverEffect : MonoBehaviour
	{
		public GameObject RootObject { get; set; }
		private void Awake()
		{
			this.coverOriginRect = this.cover.rectTransform.sizeDelta;
			this.edgeOriginRect = this.edge.rectTransform.sizeDelta;
		}
		public void Remove()
		{
			this.cover.rectTransform.sizeDelta = this.coverOriginRect;
			this.edge.rectTransform.sizeDelta = this.edgeOriginRect;
			this._coverMaterial = Object.Instantiate<Material>(this.cover.material);
			this.cover.material = this._coverMaterial;
			this._coverMaterial.SetFloat(RemoveCoverEffect.DissolveProgress, 1f);
			this.edge.color = new Color(1f, 1f, 1f, 1f);
			this.cover.material.SetFloat(RemoveCoverEffect.Alpha, 1f);
			this.P1.Play();
			this.P2.Play();
			this.P3.Play();
			this.P4.Play();
			DOTween.Sequence("Remove").Append(DOTween.To(new DOSetter<float>(this.ExileGrowth), 0f, 1f, this.growthTime).SetEase(this.growthEase)).AppendCallback(delegate
			{
				foreach (GameObject gameObject in this.CloseObjects)
				{
					gameObject.SetActive(false);
				}
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
			this._coverMaterial.SetFloat(RemoveCoverEffect.DissolveProgress, 1f - t);
		}
		private void ExileFade(float t)
		{
			this._coverMaterial.SetFloat(RemoveCoverEffect.DissolveEdge, (1f - t) * 0.2f);
			this._coverMaterial.SetFloat(RemoveCoverEffect.DissolveProgress, t);
			this.edge.color = new Color(1f, 1f, 1f, 1f - t);
		}
		public Image cover;
		public Image edge;
		private Material _coverMaterial;
		public float growthTime = 0.5f;
		public float fadeTime = 0.5f;
		public float coverOffset = 1.01f;
		public float xRatio;
		public float yRatio;
		public Ease growthEase;
		public Ease fadeEase;
		private Vector2 coverOriginRect;
		private Vector2 edgeOriginRect;
		public ParticleSystem P1;
		public ParticleSystem P2;
		public ParticleSystem P3;
		public ParticleSystem P4;
		private static readonly int DissolveProgress = Shader.PropertyToID("_DissolveProgress");
		private static readonly int Alpha = Shader.PropertyToID("_Alpha");
		private static readonly int DissolveEdge = Shader.PropertyToID("_DissolveEdge");
		public readonly List<GameObject> CloseObjects = new List<GameObject>();
	}
}
