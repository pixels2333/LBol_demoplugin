using System;
using LBoL.Base;
using UnityEngine;

namespace LBoL.Presentation.Effect
{
	// Token: 0x02000104 RID: 260
	public class ManaFlyEffect : MonoBehaviour
	{
		// Token: 0x06000E69 RID: 3689 RVA: 0x00044E0C File Offset: 0x0004300C
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

		// Token: 0x06000E6A RID: 3690 RVA: 0x00044EE8 File Offset: 0x000430E8
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

		// Token: 0x04000ACC RID: 2764
		[SerializeField]
		private ParticleSystem[] particleSystems;

		// Token: 0x04000ACD RID: 2765
		[SerializeField]
		private AssociationList<ManaColor, Color> colorTable;

		// Token: 0x04000ACE RID: 2766
		private ParticleSystem.MinMaxGradient _pColor;
	}
}
