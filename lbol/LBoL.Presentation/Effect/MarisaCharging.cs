using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.StatusEffects;
using UnityEngine;
namespace LBoL.Presentation.Effect
{
	public class MarisaCharging : MonoBehaviour
	{
		[UsedImplicitly]
		public void OnPropertyChanged(StatusEffect effect)
		{
			this.StartType(effect.Level);
		}
		private void StartType(int type)
		{
			for (int i = 0; i < this.particleList.Count; i++)
			{
				this.particleList[i].SetActive(i < type);
			}
		}
		[SerializeField]
		private List<GameObject> particleList;
	}
}
