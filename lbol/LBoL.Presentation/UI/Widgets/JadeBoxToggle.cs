using System;
using LBoL.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class JadeBoxToggle : MonoBehaviour
	{
		public bool IsOn
		{
			get
			{
				return this.toggle.isOn;
			}
		}
		public Toggle Toggle
		{
			get
			{
				return this.toggle;
			}
		}
		public JadeBox JadeBox
		{
			get
			{
				return this._jadeBox;
			}
			set
			{
				if (this._jadeBox == value)
				{
					return;
				}
				this._jadeBox = value;
				this.Refresh();
			}
		}
		public void Refresh()
		{
			if (this._jadeBox == null)
			{
				base.gameObject.SetActive(false);
				return;
			}
			this.title.text = this._jadeBox.Name;
			this.description.text = this._jadeBox.Description;
		}
		[SerializeField]
		private TextMeshProUGUI title;
		[SerializeField]
		private TextMeshProUGUI description;
		[SerializeField]
		private Toggle toggle;
		[SerializeField]
		public Image icon;
		[SerializeField]
		public Image bg;
		private JadeBox _jadeBox;
	}
}
