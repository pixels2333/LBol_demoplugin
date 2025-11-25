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
	public class LoveGirlEffectManager : MonoBehaviour
	{
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
		private void Update()
		{
			this.time += Time.unscaledDeltaTime;
			foreach (LoveGirlEffectView loveGirlEffectView in this.list)
			{
				loveGirlEffectView.Calculation(this.time, Time.unscaledDeltaTime);
			}
		}
		[SerializeField]
		private LoveGirlEffectView template;
		private readonly List<LoveGirlEffectView> list = new List<LoveGirlEffectView>();
		private float time;
	}
}
