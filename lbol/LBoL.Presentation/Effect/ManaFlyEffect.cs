using System;
using LBoL.Base;
using UnityEngine;
namespace LBoL.Presentation.Effect
{
	public class ManaFlyEffect : MonoBehaviour
	{
		private void Awake()
		{
			Gradient gradient = new Gradient
			{
				colorKeys = new GradientColorKey[]
				{
					new GradientColorKey(this.colorTable[ManaColor.Red], 0f),
					new GradientColorKey(this.colorTable[ManaColor.Black], 0.2f),
					new GradientColorKey(this.colorTable[ManaColor.Blue], 0.4f),
					new GradientColorKey(this.colorTable[ManaColor.Green], 0.6f),
					new GradientColorKey(this.colorTable[ManaColor.White], 0.8f),
					new GradientColorKey(this.colorTable[ManaColor.Red], 1f)
				}
			};
			this._pColor = new ParticleSystem.MinMaxGradient(gradient);
		}
		public void SetColor(ManaColor manaColor)
		{
			ParticleSystem[] array = this.particleSystems;
			for (int i = 0; i < array.Length; i++)
			{
				ParticleSystem.MainModule main = array[i].main;
				if (manaColor == ManaColor.Philosophy)
				{
					main.startColor = this._pColor;
				}
				else
				{
					Color color2;
					Color color = (this.colorTable.TryGetValue(manaColor, out color2) ? color2 : Color.white);
					main.startColor = new ParticleSystem.MinMaxGradient(color);
				}
			}
		}
		[SerializeField]
		private ParticleSystem[] particleSystems;
		[SerializeField]
		private AssociationList<ManaColor, Color> colorTable;
		private ParticleSystem.MinMaxGradient _pColor;
	}
}
