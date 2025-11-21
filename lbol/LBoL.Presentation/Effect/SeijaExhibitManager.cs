using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core.StatusEffects;
using UnityEngine;

namespace LBoL.Presentation.Effect
{
	// Token: 0x0200010B RID: 267
	public class SeijaExhibitManager : MonoBehaviour
	{
		// Token: 0x06000EA6 RID: 3750 RVA: 0x00045B34 File Offset: 0x00043D34
		[UsedImplicitly]
		public void AddExhibit(StatusEffect effect)
		{
			foreach (SeijaExhibitView seijaExhibitView in this.templates)
			{
				if (seijaExhibitView.name == effect.Id)
				{
					SeijaExhibitView seijaExhibitView2 = Object.Instantiate<SeijaExhibitView>(seijaExhibitView, base.transform);
					this.dictionary.Add(effect.Id, seijaExhibitView2);
					if (this.dictionary.Count <= 0)
					{
						break;
					}
					using (IEnumerator<ValueTuple<int, KeyValuePair<string, SeijaExhibitView>>> enumerator2 = this.dictionary.WithIndices<KeyValuePair<string, SeijaExhibitView>>().GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							ValueTuple<int, KeyValuePair<string, SeijaExhibitView>> valueTuple = enumerator2.Current;
							KeyValuePair<string, SeijaExhibitView> item = valueTuple.Item2;
							string text;
							SeijaExhibitView seijaExhibitView3;
							item.Deconstruct(ref text, ref seijaExhibitView3);
							int item2 = valueTuple.Item1;
							SeijaExhibitView seijaExhibitView4 = seijaExhibitView3;
							seijaExhibitView4.orbitAngleOffset = (float)(item2 * 360 / this.dictionary.Count);
							seijaExhibitView4.phase = (float)(item2 * 30);
							seijaExhibitView4.InTargetPosition = false;
							seijaExhibitView4.EnterTargetSpeed = 0.5f;
						}
						break;
					}
				}
			}
		}

		// Token: 0x06000EA7 RID: 3751 RVA: 0x00045C5C File Offset: 0x00043E5C
		private void Update()
		{
			this.time += Time.unscaledDeltaTime;
			foreach (KeyValuePair<string, SeijaExhibitView> keyValuePair in this.dictionary)
			{
				string text;
				SeijaExhibitView seijaExhibitView;
				keyValuePair.Deconstruct(ref text, ref seijaExhibitView);
				seijaExhibitView.Calculation(this.time, Time.unscaledDeltaTime);
			}
		}

		// Token: 0x06000EA8 RID: 3752 RVA: 0x00045CD8 File Offset: 0x00043ED8
		public Transform GetExhibitTransform(string id)
		{
			foreach (KeyValuePair<string, SeijaExhibitView> keyValuePair in this.dictionary)
			{
				string text;
				SeijaExhibitView seijaExhibitView;
				keyValuePair.Deconstruct(ref text, ref seijaExhibitView);
				string text2 = text;
				SeijaExhibitView seijaExhibitView2 = seijaExhibitView;
				if (text2 == id)
				{
					return seijaExhibitView2.transform;
				}
			}
			return null;
		}

		// Token: 0x04000B04 RID: 2820
		[SerializeField]
		private List<SeijaExhibitView> templates;

		// Token: 0x04000B05 RID: 2821
		private readonly Dictionary<string, SeijaExhibitView> dictionary = new Dictionary<string, SeijaExhibitView>();

		// Token: 0x04000B06 RID: 2822
		private float time;
	}
}
