using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class DropdownWidget : CommonButtonWidget
	{
		private bool IsExpanded
		{
			get
			{
				return this._isExpanded;
			}
			set
			{
				this._isExpanded = value;
				this.OnOpenOrClose(value);
			}
		}
		private void OnOpenOrClose(bool isOpen)
		{
			this.cursor.rectTransform.localRotation = Quaternion.Euler(0f, 0f, (float)(this.IsExpanded ? 0 : 180));
			CommonButtonWidget.ButtonWeight buttonWeight = this.buttonWeight;
			if (buttonWeight == CommonButtonWidget.ButtonWeight.Normal)
			{
				AudioManager.Button(isOpen ? 0 : 1);
				return;
			}
			if (buttonWeight != CommonButtonWidget.ButtonWeight.Light)
			{
				throw new ArgumentOutOfRangeException();
			}
			AudioManager.Button(isOpen ? 3 : 4);
		}
		public void Update()
		{
			if (this.IsExpanded == this.dropdown.IsExpanded)
			{
				return;
			}
			this.IsExpanded = this.dropdown.IsExpanded;
		}
		[SerializeField]
		private Image cursor;
		public TMP_Dropdown dropdown;
		private bool _isExpanded;
	}
}
