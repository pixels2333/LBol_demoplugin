using System;
using JetBrains.Annotations;
using LBoL.Core;
using UnityEngine;

namespace LBoL.Presentation.Effect
{
	// Token: 0x02000100 RID: 256
	public class ExhibitActivating : MonoBehaviour
	{
		// Token: 0x06000E49 RID: 3657 RVA: 0x000444BD File Offset: 0x000426BD
		private void Start()
		{
			this.particleTemplate.SetActive(false);
		}

		// Token: 0x06000E4A RID: 3658 RVA: 0x000444CC File Offset: 0x000426CC
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

		// Token: 0x04000AA1 RID: 2721
		[SerializeField]
		private GameObject particleTemplate;

		// Token: 0x04000AA2 RID: 2722
		[SerializeField]
		private Transform root;

		// Token: 0x04000AA3 RID: 2723
		[SerializeField]
		private Material particleMaterial;
	}
}
