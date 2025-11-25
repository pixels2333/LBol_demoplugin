using System;
using LBoL.Core;
using LBoL.Presentation.I10N;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	[DisallowMultipleComponent]
	public sealed class ActionMappingRow : MonoBehaviour
	{
		public InputAction InputAction
		{
			get
			{
				return this._inputAction;
			}
		}
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
		private void OnEnable()
		{
			L10nManager.LocaleChanged += new Action(this.OnLocaleChanged);
		}
		private void OnDisable()
		{
			L10nManager.LocaleChanged -= new Action(this.OnLocaleChanged);
		}
		private void OnLocaleChanged()
		{
			if (this._actionId != null)
			{
				this.actionNameText.text = ("Setting.Actions." + this._actionId).Localize(true);
			}
		}
		public void SetMapping(ActionMapping mapping)
		{
			this._actionId = mapping.Id;
			this._inputAction = mapping.InputAction;
			this.actionNameText.text = ("Setting.Actions." + this._actionId).Localize(true);
			this.Refresh();
		}
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
		public event Action ButtonClicked;
		[SerializeField]
		private TextMeshProUGUI actionNameText;
		[SerializeField]
		private Button keyboardButton;
		[SerializeField]
		private TextMeshProUGUI keyboardButtonText;
		private string _actionId;
		private InputAction _inputAction;
	}
}
