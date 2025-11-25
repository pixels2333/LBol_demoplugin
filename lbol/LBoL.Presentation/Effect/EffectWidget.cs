using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace LBoL.Presentation.Effect
{
	public class EffectWidget : MonoBehaviour
	{
		private void Awake()
		{
			this._particleSystemRawStartColors = new ParticleSystem.MinMaxGradient[this.particleSystemElements.Length];
			this._particleSystemRawLifetimeColors = new ParticleSystem.MinMaxGradient[this.particleSystemElements.Length];
			for (int i = 0; i < this.particleSystemElements.Length; i++)
			{
				if (this.particleSystemElements[i].lowPerformance && EffectManager.IsLowPerformance)
				{
					this.particleSystemElements[i].particleSystem.gameObject.SetActive(false);
				}
				else if (this.particleSystemElements[i].changeColor)
				{
					this._particleSystemRawStartColors[i] = this.particleSystemElements[i].particleSystem.main.startColor;
					this._particleSystemRawLifetimeColors[i] = this.particleSystemElements[i].particleSystem.colorOverLifetime.color;
				}
			}
			this._trailRenderRawColors = new Gradient[this.trailRendererElements.Length];
			for (int j = 0; j < this.trailRendererElements.Length; j++)
			{
				if (this.trailRendererElements[j].lowPerformance && EffectManager.IsLowPerformance)
				{
					this.trailRendererElements[j].trailRenderer.gameObject.SetActive(false);
				}
				else if (this.trailRendererElements[j].changeColor)
				{
					this._trailRenderRawColors[j] = this.trailRendererElements[j].trailRenderer.colorGradient;
				}
			}
		}
		public void RefindElements()
		{
			List<EffectWidget.ParticleSystemElement> list = new List<EffectWidget.ParticleSystemElement>();
			ParticleSystem[] componentsInChildren = base.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				ParticleSystem ps = componentsInChildren[i];
				EffectWidget.ParticleSystemElement particleSystemElement = Enumerable.FirstOrDefault<EffectWidget.ParticleSystemElement>(this.particleSystemElements, (EffectWidget.ParticleSystemElement e) => e.particleSystem == ps);
				List<EffectWidget.ParticleSystemElement> list2 = list;
				EffectWidget.ParticleSystemElement particleSystemElement2;
				if ((particleSystemElement2 = particleSystemElement) == null)
				{
					EffectWidget.ParticleSystemElement particleSystemElement3 = new EffectWidget.ParticleSystemElement();
					particleSystemElement3.particleSystem = ps;
					particleSystemElement3.changeColor = false;
					particleSystemElement2 = particleSystemElement3;
					particleSystemElement3.dieType = EffectWidget.DieType.DoNothing;
				}
				list2.Add(particleSystemElement2);
			}
			this.particleSystemElements = list.ToArray();
			List<EffectWidget.TrailRendererElement> list3 = new List<EffectWidget.TrailRendererElement>();
			TrailRenderer[] componentsInChildren2 = base.GetComponentsInChildren<TrailRenderer>();
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				TrailRenderer trail = componentsInChildren2[i];
				EffectWidget.TrailRendererElement trailRendererElement = Enumerable.FirstOrDefault<EffectWidget.TrailRendererElement>(this.trailRendererElements, (EffectWidget.TrailRendererElement e) => e.trailRenderer == trail);
				List<EffectWidget.TrailRendererElement> list4 = list3;
				EffectWidget.TrailRendererElement trailRendererElement2;
				if ((trailRendererElement2 = trailRendererElement) == null)
				{
					EffectWidget.TrailRendererElement trailRendererElement3 = new EffectWidget.TrailRendererElement();
					trailRendererElement3.trailRenderer = trail;
					trailRendererElement3.changeColor = false;
					trailRendererElement2 = trailRendererElement3;
					trailRendererElement3.dieType = EffectWidget.DieType.DoNothing;
				}
				list4.Add(trailRendererElement2);
			}
			this.trailRendererElements = list3.ToArray();
		}
		public void CheckColor(string path)
		{
			foreach (EffectWidget.ParticleSystemElement particleSystemElement in this.particleSystemElements)
			{
				if (particleSystemElement.changeColor)
				{
					ParticleSystem particleSystem = particleSystemElement.particleSystem;
					ParticleSystem.MainModule main = particleSystem.main;
					switch (main.startColor.mode)
					{
					case ParticleSystemGradientMode.Color:
					case ParticleSystemGradientMode.Gradient:
					case ParticleSystemGradientMode.TwoColors:
					case ParticleSystemGradientMode.RandomColor:
						break;
					default:
						Debug.LogWarning(path + " particleSystemElements" + string.Format(" Changing color of type {0} in MainColor not supported", main.startColor.mode));
						break;
					}
					if (particleSystem.colorOverLifetime.enabled)
					{
						ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystem.colorOverLifetime;
						switch (colorOverLifetime.color.mode)
						{
						case ParticleSystemGradientMode.Color:
						case ParticleSystemGradientMode.Gradient:
						case ParticleSystemGradientMode.TwoColors:
						case ParticleSystemGradientMode.RandomColor:
							break;
						default:
							Debug.LogWarning(path + " particleSystemElements" + string.Format("Changing color of type {0} not in ColorOverLifeTime supported", colorOverLifetime.color.mode));
							break;
						}
					}
				}
			}
		}
		public void ResetColors()
		{
			for (int i = 0; i < this.particleSystemElements.Length; i++)
			{
				if (this.particleSystemElements[i].changeColor)
				{
					this.particleSystemElements[i].particleSystem.main.startColor = this._particleSystemRawStartColors[i];
					this.particleSystemElements[i].particleSystem.colorOverLifetime.color = this._particleSystemRawLifetimeColors[i];
				}
			}
			for (int j = 0; j < this.trailRendererElements.Length; j++)
			{
				if (this.trailRendererElements[j].changeColor)
				{
					this.trailRendererElements[j].trailRenderer.colorGradient = this._trailRenderRawColors[j];
				}
			}
		}
		public void Modify(float hue, bool isDecolor)
		{
			foreach (EffectWidget.ParticleSystemElement particleSystemElement in this.particleSystemElements)
			{
				if (particleSystemElement.changeColor)
				{
					ParticleSystem particleSystem = particleSystemElement.particleSystem;
					ParticleSystem.MainModule main = particleSystem.main;
					switch (main.startColor.mode)
					{
					case ParticleSystemGradientMode.Color:
						main.startColor = EffectWidget.ReplaceHue(main.startColor.color, hue, isDecolor);
						break;
					case ParticleSystemGradientMode.Gradient:
						main.startColor = EffectWidget.ReplaceHue(main.startColor.gradient, hue, isDecolor);
						break;
					case ParticleSystemGradientMode.TwoColors:
					{
						ParticleSystem.MinMaxGradient startColor = main.startColor;
						startColor.colorMin = EffectWidget.ReplaceHue(startColor.colorMin, hue, isDecolor);
						startColor.colorMax = EffectWidget.ReplaceHue(startColor.colorMax, hue, isDecolor);
						main.startColor = startColor;
						break;
					}
					case ParticleSystemGradientMode.TwoGradients:
						goto IL_0111;
					case ParticleSystemGradientMode.RandomColor:
						main.startColor = EffectWidget.ReplaceHue(main.startColor.gradient, hue, isDecolor);
						break;
					default:
						goto IL_0111;
					}
					IL_0135:
					if (particleSystem.colorOverLifetime.enabled)
					{
						ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystem.colorOverLifetime;
						switch (colorOverLifetime.color.mode)
						{
						case ParticleSystemGradientMode.Color:
							colorOverLifetime.color = EffectWidget.ReplaceHue(colorOverLifetime.color.color, hue, isDecolor);
							goto IL_0264;
						case ParticleSystemGradientMode.Gradient:
							colorOverLifetime.color = EffectWidget.ReplaceHue(colorOverLifetime.color.gradient, hue, isDecolor);
							goto IL_0264;
						case ParticleSystemGradientMode.TwoColors:
						{
							ParticleSystem.MinMaxGradient startColor2 = main.startColor;
							startColor2.colorMin = EffectWidget.ReplaceHue(startColor2.colorMin, hue, isDecolor);
							startColor2.colorMax = EffectWidget.ReplaceHue(startColor2.colorMax, hue, isDecolor);
							main.startColor = startColor2;
							goto IL_0264;
						}
						case ParticleSystemGradientMode.RandomColor:
							colorOverLifetime.color = EffectWidget.ReplaceHue(colorOverLifetime.color.gradient, hue, isDecolor);
							goto IL_0264;
						}
						Debug.LogWarning("particleSystemElements" + string.Format("Changing color of type {0} not in ColorOverLifeTime supported", colorOverLifetime.color.mode));
						goto IL_0264;
					}
					goto IL_0264;
					IL_0111:
					Debug.LogWarning(string.Format("Changing color of type {0} in MainColor not supported", main.startColor.mode));
					goto IL_0135;
				}
				IL_0264:;
			}
			foreach (EffectWidget.TrailRendererElement trailRendererElement in this.trailRendererElements)
			{
				if (trailRendererElement.changeColor)
				{
					TrailRenderer trailRenderer = trailRendererElement.trailRenderer;
					trailRenderer.colorGradient = EffectWidget.ReplaceHue(trailRenderer.colorGradient, hue, isDecolor);
				}
			}
		}
		private static Color ReplaceHue(Color color, float hue, bool decolor)
		{
			float a = color.a;
			float num;
			float num2;
			float num3;
			Color.RGBToHSV(color, out num, out num2, out num3);
			color = Color.HSVToRGB(hue, decolor ? 0f : num2, decolor ? (0.8f * num3) : num3);
			color.a = a;
			return color;
		}
		private static Gradient ReplaceHue(Gradient g, float hue, bool decolor)
		{
			GradientColorKey[] colorKeys = g.colorKeys;
			for (int i = 0; i < g.colorKeys.Length; i++)
			{
				colorKeys[i].color = EffectWidget.ReplaceHue(g.colorKeys[i].color, hue, decolor);
			}
			g.colorKeys = colorKeys;
			return g;
		}
		public void DieOut()
		{
			foreach (EffectWidget.ParticleSystemElement particleSystemElement in this.particleSystemElements)
			{
				if (particleSystemElement.dieType == EffectWidget.DieType.Inactivate)
				{
					particleSystemElement.particleSystem.gameObject.SetActive(false);
				}
				else if (particleSystemElement.dieType == EffectWidget.DieType.StopEmission)
				{
					particleSystemElement.particleSystem.emission.enabled = false;
				}
			}
			foreach (EffectWidget.TrailRendererElement trailRendererElement in this.trailRendererElements)
			{
				if (trailRendererElement.dieType == EffectWidget.DieType.Inactivate)
				{
					trailRendererElement.trailRenderer.gameObject.SetActive(false);
				}
				else if (trailRendererElement.dieType == EffectWidget.DieType.StopEmission)
				{
					trailRendererElement.trailRenderer.emitting = false;
				}
			}
			Object.Destroy(base.gameObject, 3f);
		}
		public void SetSortingOrder(int order)
		{
			EffectWidget.ParticleSystemElement[] array = this.particleSystemElements;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].particleSystem.GetComponent<Renderer>().sortingOrder = order;
			}
		}
		[SerializeField]
		private EffectWidget.ParticleSystemElement[] particleSystemElements;
		[SerializeField]
		private EffectWidget.TrailRendererElement[] trailRendererElements;
		private ParticleSystem.MinMaxGradient[] _particleSystemRawStartColors;
		private ParticleSystem.MinMaxGradient[] _particleSystemRawLifetimeColors;
		private Gradient[] _trailRenderRawColors;
		public enum DieType
		{
			Inactivate,
			StopEmission,
			DoNothing
		}
		[Serializable]
		public class ParticleSystemElement
		{
			public ParticleSystem particleSystem;
			public bool changeColor;
			public EffectWidget.DieType dieType;
			public bool lowPerformance;
		}
		[Serializable]
		public class TrailRendererElement
		{
			public TrailRenderer trailRenderer;
			public bool changeColor;
			public EffectWidget.DieType dieType;
			public bool lowPerformance;
		}
	}
}
