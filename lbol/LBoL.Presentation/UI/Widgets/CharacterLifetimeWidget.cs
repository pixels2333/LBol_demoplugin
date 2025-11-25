using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;
using TMPro;
using UnityEngine;
namespace LBoL.Presentation.UI.Widgets
{
	public class CharacterLifetimeWidget : MonoBehaviour
	{
		public int Order { get; set; } = -99;
		public void SetValue(string valueText)
		{
			this.valueTmp.text = valueText;
		}
		public void SetTitle([CanBeNull] PlayerUnit chara, bool isOld)
		{
			this.Chara = chara;
			this.isOld = isOld;
			this.Refresh();
		}
		public void Refresh()
		{
			if (this.isOld)
			{
				this.nameTmp.text = "Lifetime.OldData".Localize(true);
				return;
			}
			if (this.Chara != null)
			{
				this.nameTmp.text = this.Chara.Name;
			}
		}
		[SerializeField]
		private TextMeshProUGUI nameTmp;
		[SerializeField]
		private TextMeshProUGUI valueTmp;
		public PlayerUnit Chara;
		public bool isOld;
	}
}
