using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.StatusEffects;
using UnityEngine;
namespace LBoL.Presentation.Effect
{
	public class ReimiChain : MonoBehaviour
	{
		[UsedImplicitly]
		public void OnPropertyChanged(StatusEffect effect)
		{
			for (int i = 0; i < this.particleList.Count; i++)
			{
				int limit = effect.Limit;
				this.particleList[i].SetActive(i < 7 - effect.Count);
			}
		}
		[SerializeField]
		private List<GameObject> particleList;
	}
}
