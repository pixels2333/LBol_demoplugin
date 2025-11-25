using System;
using System.Linq;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Presentation.UI.Panels;
using TMPro;
using UnityEngine;
namespace LBoL.Presentation.UI.Widgets
{
	public class JadeBoxWidget : MonoBehaviour
	{
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
		[SerializeField]
		private TextMeshProUGUI title;
		[SerializeField]
		private TextMeshProUGUI description;
		[SerializeField]
		private GameObject hint;
	}
}
