using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LBoL.Presentation.Effect
{
	// Token: 0x02000101 RID: 257
	public class ExileCoverEffect : MonoBehaviour
	{
		// Token: 0x17000260 RID: 608
		// (get) Token: 0x06000E4C RID: 3660 RVA: 0x00044531 File Offset: 0x00042731
		// (set) Token: 0x06000E4D RID: 3661 RVA: 0x00044539 File Offset: 0x00042739
		public GameObject RootObject { get; set; }

		// Token: 0x06000E4E RID: 3662 RVA: 0x00044544 File Offset: 0x00042744
		private void Awake()
		{
			this.coverOriginRect = this.cover.rectTransform.sizeDelta;
			this.edgeOriginRect = this.edge.rectTransform.sizeDelta;
			this._coverMaterial = new Material(this.cover.material);
			this.cover.material = this._coverMaterial;
		}

		// Token: 0x06000E4F RID: 3663 RVA: 0x000445A4 File Offset: 0x000427A4
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

		// Token: 0x06000E50 RID: 3664 RVA: 0x000446C6 File Offset: 0x000428C6
		private void ExileGrowth(float t)
		{
			this._coverMaterial.SetFloat(ExileCoverEffect.DissolveProgress, 1f - t);
			this.edge.color = new Color(1f, 1f, 1f, t);
		}

		// Token: 0x06000E51 RID: 3665 RVA: 0x00044700 File Offset: 0x00042900
		private void ExileFade(float t)
		{
			this.cover.rectTransform.sizeDelta = Vector2.Lerp(this.coverOriginRect, new Vector2(this.coverOriginRect.x * this.xRatio, this.coverOriginRect.y * this.yRatio), t * this.coverOffset);
			this.edge.rectTransform.sizeDelta = Vector2.Lerp(this.edgeOriginRect, new Vector2(this.edgeOriginRect.x * this.xRatio, this.edgeOriginRect.y * this.yRatio), t);
		}

		// Token: 0x04000AA4 RID: 2724
		public Image cover;

		// Token: 0x04000AA5 RID: 2725
		public Image edge;

		// Token: 0x04000AA6 RID: 2726
		public ParticleSystem flare;

		// Token: 0x04000AA7 RID: 2727
		public ParticleSystem particles;

		// Token: 0x04000AA8 RID: 2728
		public float growthTime = 0.5f;

		// Token: 0x04000AA9 RID: 2729
		public float fadeTime = 0.5f;

		// Token: 0x04000AAA RID: 2730
		public float coverOffset = 1.01f;

		// Token: 0x04000AAB RID: 2731
		public float xRatio;

		// Token: 0x04000AAC RID: 2732
		public float yRatio;

		// Token: 0x04000AAD RID: 2733
		public Ease growthEase;

		// Token: 0x04000AAE RID: 2734
		public Ease fadeEase;

		// Token: 0x04000AAF RID: 2735
		private Vector2 coverOriginRect;

		// Token: 0x04000AB0 RID: 2736
		private Vector2 edgeOriginRect;

		// Token: 0x04000AB1 RID: 2737
		private static readonly int DissolveProgress = Shader.PropertyToID("_DissolveProgress");

		// Token: 0x04000AB2 RID: 2738
		private static readonly int Alpha = Shader.PropertyToID("_Alpha");

		// Token: 0x04000AB3 RID: 2739
		public readonly List<GameObject> CloseObjects = new List<GameObject>();

		// Token: 0x04000AB5 RID: 2741
		private Material _coverMaterial;
	}
}
