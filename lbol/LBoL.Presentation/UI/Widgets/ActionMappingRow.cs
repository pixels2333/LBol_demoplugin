using System;
using LBoL.Core;
using LBoL.Presentation.I10N;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x0200003B RID: 59
	[DisallowMultipleComponent]
	public sealed class ActionMappingRow : MonoBehaviour
	{
		// Token: 0x1700009E RID: 158
		// (get) Token: 0x060003D9 RID: 985 RVA: 0x0000FE39 File Offset: 0x0000E039
		public InputAction InputAction
		{
			get
			{
				return this._inputAction;
			}
		}

		// Token: 0x060003DA RID: 986 RVA: 0x0000FE41 File Offset: 0x0000E041
		private void Awake()
		{
			this.keyboardButton.onClick.AddListener(delegate
			{
				Action buttonClicked = this.ButtonClicked;
				if (buttonClicked == null)
				{
					return;
				}
				buttonClicked.Invoke();
			});
		}

		// Token: 0x060003DB RID: 987 RVA: 0x0000FE5F File Offset: 0x0000E05F
		private void OnEnable()
		{
			L10nManager.LocaleChanged += new Action(this.OnLocaleChanged);
		}

		// Token: 0x060003DC RID: 988 RVA: 0x0000FE72 File Offset: 0x0000E072
		private void OnDisable()
		{
			L10nManager.LocaleChanged -= new Action(this.OnLocaleChanged);
		}

		// Token: 0x060003DD RID: 989 RVA: 0x0000FE85 File Offset: 0x0000E085
		private void OnLocaleChanged()
		{
			if (this._actionId != null)
			{
				this.actionNameText.text = ("Setting.Actions." + this._actionId).Localize(true);
			}
		}

		// Token: 0x060003DE RID: 990 RVA: 0x0000FEB0 File Offset: 0x0000E0B0
		public void SetMapping(ActionMapping mapping)
		{
			this._actionId = mapping.Id;
			this._inputAction = mapping.InputAction;
			this.actionNameText.text = ("Setting.Actions." + this._actionId).Localize(true);
			this.Refresh();
		}

		// Token: 0x060003DF RID: 991 RVA: 0x0000FEFC File Offset: 0x0000E0FC
		public void Refresh()
		{
			int bindingIndex = this._inputAction.GetBindingIndex(InputBinding.MaskByGroup("Keyboard&Mouse"));
			if (bindingIndex >= 0)
			{
				InputBinding inputBinding = this._inputAction.bindings[bindingIndex];
				this.keyboardButtonText.text = inputBinding.ToDisplayString((InputBinding.DisplayStringOptions)0, null);
				return;
			}
			this.keyboardButtonText.text = "<no-bindings>";
		}

		// Token: 0x14000004 RID: 4
		// (add) Token: 0x060003E0 RID: 992 RVA: 0x0000FF60 File Offset: 0x0000E160
		// (remove) Token: 0x060003E1 RID: 993 RVA: 0x0000FF98 File Offset: 0x0000E198
		public event Action ButtonClicked;

		// Token: 0x040001C3 RID: 451
		[SerializeField]
		private TextMeshProUGUI actionNameText;

		// Token: 0x040001C4 RID: 452
		[SerializeField]
		private Button keyboardButton;

		// Token: 0x040001C5 RID: 453
		[SerializeField]
		private TextMeshProUGUI keyboardButtonText;

		// Token: 0x040001C6 RID: 454
		private string _actionId;

		// Token: 0x040001C7 RID: 455
		private InputAction _inputAction;
	}
}
