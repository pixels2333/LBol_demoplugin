using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using UnityEngine;

namespace LBoL.Presentation.Effect
{
	// Token: 0x02000102 RID: 258
	public class LoveGirlEffectManager : MonoBehaviour
	{
		// Token: 0x06000E56 RID: 3670 RVA: 0x00044870 File Offset: 0x00042A70
		[UsedImplicitly]
		public void Add(int amount)
		{
			for (int i = 0; i < amount; i++)
			{
				LoveGirlEffectView loveGirlEffectView = Object.Instantiate<LoveGirlEffectView>(this.template, base.transform);
				this.list.Add(loveGirlEffectView);
			}
			this.Refresh();
		}

		// Token: 0x06000E57 RID: 3671 RVA: 0x000448B0 File Offset: 0x00042AB0
		[UsedImplicitly]
		public void Remove(int amount)
		{
			if (this.list.Count == 0)
			{
				return;
			}
			if (amount >= this.list.Count)
			{
				using (List<LoveGirlEffectView>.Enumerator enumerator = this.list.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						LoveGirlEffectView v2 = enumerator.Current;
						v2.transform.DOScale(0f, 0.2f).OnComplete(delegate
						{
							Object.Destroy(v2.gameObject);
						});
					}
				}
				this.list.Clear();
			}
			else
			{
				List<LoveGirlEffectView> list = Enumerable.ToList<LoveGirlEffectView>(Enumerable.Take<LoveGirlEffectView>(this.list, amount));
				using (List<LoveGirlEffectView>.Enumerator enumerator = list.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						LoveGirlEffectView v = enumerator.Current;
						v.transform.DOScale(0f, 0.2f).OnComplete(delegate
						{
							Object.Destroy(v.gameObject);
						});
					}
				}
				foreach (LoveGirlEffectView loveGirlEffectView in list)
				{
					this.list.Remove(loveGirlEffectView);
				}
			}
			this.Refresh();
		}

		// Token: 0x06000E58 RID: 3672 RVA: 0x00044A2C File Offset: 0x00042C2C
		private void Refresh()
		{
			if (this.list.Count > 0)
			{
				foreach (ValueTuple<int, LoveGirlEffectView> valueTuple in this.list.WithIndices<LoveGirlEffectView>())
				{
					int item = valueTuple.Item1;
					LoveGirlEffectView item2 = valueTuple.Item2;
					item2.orbitAngleOffset = (float)(item * 360 / this.list.Count);
					item2.phase = (float)(item * 30);
					item2.InTargetPosition = false;
					item2.EnterTargetSpeed = 0.5f;
				}
			}
		}

		// Token: 0x06000E59 RID: 3673 RVA: 0x00044AC8 File Offset: 0x00042CC8
		private void Update()
		{
			this.time += Time.unscaledDeltaTime;
			foreach (LoveGirlEffectView loveGirlEffectView in this.list)
			{
				loveGirlEffectView.Calculation(this.time, Time.unscaledDeltaTime);
			}
		}

		// Token: 0x04000AB6 RID: 2742
		[SerializeField]
		private LoveGirlEffectView template;

		// Token: 0x04000AB7 RID: 2743
		private readonly List<LoveGirlEffectView> list = new List<LoveGirlEffectView>();

		// Token: 0x04000AB8 RID: 2744
		private float time;
	}
}
