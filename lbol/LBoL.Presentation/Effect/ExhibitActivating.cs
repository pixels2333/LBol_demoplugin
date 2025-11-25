using System;
using JetBrains.Annotations;
using LBoL.Core;
using UnityEngine;
namespace LBoL.Presentation.Effect
{
	public class ExhibitActivating : MonoBehaviour
	{
		private void Start()
		{
			this.particleTemplate.SetActive(false);
		}
		[UsedImplicitly]
		public void OnPropertyChanged(Exhibit exhibit)
		{
			Sprite sprite = ResourcesHelper.TryGetSprite<Exhibit>(exhibit.Id);
			if (sprite != null)
			{
				GameObject gameObject = Object.Instantiate<GameObject>(this.particleTemplate, this.root);
				Renderer componentInChildren = gameObject.GetComponentInChildren<ParticleSystemRenderer>();
				Material material = new Material(this.particleMaterial)
				{
					mainTexture = sprite.texture
				};
				componentInChildren.material = material;
				gameObject.SetActive(true);
			}
		}
		[SerializeField]
		private GameObject particleTemplate;
		[SerializeField]
		private Transform root;
		[SerializeField]
		private Material particleMaterial;
	}
}
