using System;
using System.Linq;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Presentation.UI.Panels;
using TMPro;
using UnityEngine;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x0200005E RID: 94
	public class JadeBoxWidget : MonoBehaviour
	{
		// Token: 0x0600053C RID: 1340 RVA: 0x000165F0 File Offset: 0x000147F0
		public void SetJadeBox(JadeBox jadeBox)
		{
			if (jadeBox == null)
			{
				this.title.text = "System.NoJadeBox.Title".Localize(true);
				this.description.text = "System.NoJadeBox.Description".Localize(true);
				this.hint.gameObject.SetActive(false);
				return;
			}
			this.title.text = jadeBox.Name;
			this.description.text = jadeBox.Description;
			if (Enumerable.Any<Card>(jadeBox.EnumerateRelativeCards()))
			{
				this.hint.AddComponent<MultipleCardTooltip>().Cards = jadeBox.EnumerateRelativeCards();
				this.hint.SetActive(true);
				return;
			}
			this.hint.SetActive(false);
		}

		// Token: 0x040002FD RID: 765
		[SerializeField]
		private TextMeshProUGUI title;

		// Token: 0x040002FE RID: 766
		[SerializeField]
		private TextMeshProUGUI description;

		// Token: 0x040002FF RID: 767
		[SerializeField]
		private GameObject hint;
	}
}
