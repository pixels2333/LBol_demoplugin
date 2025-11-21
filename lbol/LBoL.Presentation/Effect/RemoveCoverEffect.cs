using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LBoL.Presentation.Effect
{
	// Token: 0x02000108 RID: 264
	public class RemoveCoverEffect : MonoBehaviour
	{
		// Token: 0x1700026E RID: 622
		// (get) Token: 0x06000E84 RID: 3716 RVA: 0x000452CA File Offset: 0x000434CA
		// (set) Token: 0x06000E85 RID: 3717 RVA: 0x000452D2 File Offset: 0x000434D2
		public GameObject RootObject { get; set; }

		// Token: 0x06000E86 RID: 3718 RVA: 0x000452DB File Offset: 0x000434DB
		private void Awake()
		{
			this.coverOriginRect = this.cover.rectTransform.sizeDelta;
			this.edgeOriginRect = this.edge.rectTransform.sizeDelta;
		}

		// Token: 0x06000E87 RID: 3719 RVA: 0x0004530C File Offset: 0x0004350C
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

		// Token: 0x06000E88 RID: 3720 RVA: 0x00045486 File Offset: 0x00043686
		private void ExileGrowth(float t)
		{
			this._coverMaterial.SetFloat(RemoveCoverEffect.DissolveProgress, 1f - t);
		}

		// Token: 0x06000E89 RID: 3721 RVA: 0x000454A0 File Offset: 0x000436A0
		private void ExileFade(float t)
		{
			this._coverMaterial.SetFloat(RemoveCoverEffect.DissolveEdge, (1f - t) * 0.2f);
			this._coverMaterial.SetFloat(RemoveCoverEffect.DissolveProgress, t);
			this.edge.color = new Color(1f, 1f, 1f, 1f - t);
		}

		// Token: 0x04000ADC RID: 2780
		public Image cover;

		// Token: 0x04000ADD RID: 2781
		public Image edge;

		// Token: 0x04000ADE RID: 2782
		private Material _coverMaterial;

		// Token: 0x04000ADF RID: 2783
		public float growthTime = 0.5f;

		// Token: 0x04000AE0 RID: 2784
		public float fadeTime = 0.5f;

		// Token: 0x04000AE1 RID: 2785
		public float coverOffset = 1.01f;

		// Token: 0x04000AE2 RID: 2786
		public float xRatio;

		// Token: 0x04000AE3 RID: 2787
		public float yRatio;

		// Token: 0x04000AE4 RID: 2788
		public Ease growthEase;

		// Token: 0x04000AE5 RID: 2789
		public Ease fadeEase;

		// Token: 0x04000AE6 RID: 2790
		private Vector2 coverOriginRect;

		// Token: 0x04000AE7 RID: 2791
		private Vector2 edgeOriginRect;

		// Token: 0x04000AE8 RID: 2792
		public ParticleSystem P1;

		// Token: 0x04000AE9 RID: 2793
		public ParticleSystem P2;

		// Token: 0x04000AEA RID: 2794
		public ParticleSystem P3;

		// Token: 0x04000AEB RID: 2795
		public ParticleSystem P4;

		// Token: 0x04000AEC RID: 2796
		private static readonly int DissolveProgress = Shader.PropertyToID("_DissolveProgress");

		// Token: 0x04000AED RID: 2797
		private static readonly int Alpha = Shader.PropertyToID("_Alpha");

		// Token: 0x04000AEE RID: 2798
		private static readonly int DissolveEdge = Shader.PropertyToID("_DissolveEdge");

		// Token: 0x04000AEF RID: 2799
		public readonly List<GameObject> CloseObjects = new List<GameObject>();
	}
}
