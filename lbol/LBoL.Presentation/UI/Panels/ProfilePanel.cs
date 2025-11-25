using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.SaveData;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Panels
{
	public class ProfilePanel : UiPanel, IInputActionHandler
	{
		public void Awake()
		{
			this.widget0.GetComponent<Button>().onClick.AddListener(delegate
			{
				this.OnProfileClicked(0, this.widget0);
			});
			this.widget1.GetComponent<Button>().onClick.AddListener(delegate
			{
				this.OnProfileClicked(1, this.widget1);
			});
			this.widget2.GetComponent<Button>().onClick.AddListener(delegate
			{
				this.OnProfileClicked(2, this.widget2);
			});
			this.widget0.Init(delegate
			{
				this.OnProfileDeleteActive(0, this.widget0);
			}, delegate
			{
				this.OnProfileEditActive(0, this.widget0);
			});
			this.widget1.Init(delegate
			{
				this.OnProfileDeleteActive(1, this.widget1);
			}, delegate
			{
				this.OnProfileEditActive(1, this.widget1);
			});
			this.widget2.Init(delegate
			{
				this.OnProfileDeleteActive(2, this.widget2);
			}, delegate
			{
				this.OnProfileEditActive(2, this.widget2);
			});
			this.confirm.onClick.AddListener(new UnityAction(this.ConfirmInput));
			this.cancel.onClick.AddListener(new UnityAction(this.CancelInput));
			this.deleteCancel.onClick.AddListener(new UnityAction(this.OnProfileDeleteCancel));
			this._canvasGroup = base.GetComponent<CanvasGroup>();
		}
		private void RefreshSaveDatas()
		{
			this._clickedIndex = default(int?);
			this._clickedWidget = null;
			this.nameInputRoot.SetActive(false);
			this.SetSaveData(this.widget0, 0);
			this.SetSaveData(this.widget1, 1);
			this.SetSaveData(this.widget2, 2);
			this.widget0.gameObject.GetComponent<Button>().interactable = true;
			this.widget1.gameObject.GetComponent<Button>().interactable = true;
			this.widget2.gameObject.GetComponent<Button>().interactable = true;
		}
		protected override void OnShowing()
		{
			this.RefreshSaveDatas();
			this._canvasGroup.interactable = true;
			UiManager.PushActionHandler(this);
		}
		protected override void OnHiding()
		{
			EntryPanel panel = UiManager.GetPanel<EntryPanel>();
			MainMenuPanel panel2 = UiManager.GetPanel<MainMenuPanel>();
			if (Singleton<GameMaster>.Instance.CurrentSaveIndex != null)
			{
				panel2.ChangeToMain(1f);
				if (panel.IsVisible)
				{
					panel.Hide();
				}
			}
			else
			{
				panel2.ChangeToLogo();
				if (!panel.IsVisible)
				{
					panel.Show();
				}
			}
			this._canvasGroup.interactable = false;
			UiManager.PopActionHandler(this);
		}
		public void OnProfileClicked(int index, ProfileWidget widget)
		{
			if (this.nameInputRoot.activeSelf)
			{
				return;
			}
			if (widget.Profile == null)
			{
				this._clickedIndex = new int?(index);
				this._clickedWidget = widget;
				this.inputField.text = "";
				this.nameInputRoot.gameObject.SetActive(true);
				this._isCreate = true;
				this.nameInputRoot.GetComponent<CanvasGroup>().interactable = true;
				this.nameInputRoot.GetComponent<CanvasGroup>().DOFade(1f, 0.3f).From(0f, true, false);
				return;
			}
			this.widget0.gameObject.GetComponent<Button>().interactable = false;
			this.widget1.gameObject.GetComponent<Button>().interactable = false;
			this.widget2.gameObject.GetComponent<Button>().interactable = false;
			GameMaster.SelectProfile(new int?(index));
			UiManager.GetPanel<MainMenuPanel>().RefreshProfile();
			base.Hide();
		}
		private void ConfirmInput()
		{
			if (this._clickedIndex == null)
			{
				throw new InvalidOperationException("Confirm input while selected index is null");
			}
			if (string.IsNullOrWhiteSpace(this.inputField.text))
			{
				UiManager.GetDialog<MessageDialog>().Show(new MessageContent
				{
					TextKey = "GamepadUnfinishedText",
					Buttons = DialogButtons.Confirm
				});
				return;
			}
			if (this._isCreate)
			{
				GameMaster.CreateAndSelectProfile(this._clickedIndex.Value, this.inputField.text);
			}
			else
			{
				GameMaster.SetProfileName(this._clickedIndex.Value, this.inputField.text);
			}
			this.SetSaveData(this._clickedWidget, this._clickedIndex.Value);
			UiManager.GetPanel<MainMenuPanel>().RefreshProfile();
			this._clickedIndex = default(int?);
			this._clickedWidget = null;
			this.nameInputRoot.GetComponent<CanvasGroup>().interactable = false;
			TweenerCore<float, float, FloatOptions> tweenerCore = this.nameInputRoot.GetComponent<CanvasGroup>().DOFade(0f, 0.3f).From(1f, true, false);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, delegate
			{
				this.nameInputRoot.gameObject.SetActive(false);
			});
		}
		private void CancelInput()
		{
			this._clickedIndex = default(int?);
			this._clickedWidget = null;
			this.inputField.text = "";
			this.nameInputRoot.GetComponent<CanvasGroup>().interactable = false;
			TweenerCore<float, float, FloatOptions> tweenerCore = this.nameInputRoot.GetComponent<CanvasGroup>().DOFade(0f, 0.3f).From(1f, true, false);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, delegate
			{
				this.nameInputRoot.gameObject.SetActive(false);
			});
		}
		private void SetSaveData(ProfileWidget widget, int index)
		{
			ProfileSaveData profileSaveData;
			if (!GameMaster.TryLoadProfileSaveData(index, out profileSaveData))
			{
				widget.SetSaveData(null, null);
				return;
			}
			GameRunSaveData gameRunSaveData;
			if (GameMaster.TryLoadGameRunSaveData(index, out gameRunSaveData) == GameMaster.GameRunSaveDataLoadResult.Success)
			{
				widget.SetSaveData(profileSaveData, gameRunSaveData);
				return;
			}
			widget.SetSaveData(profileSaveData, null);
		}
		private void OnProfileDeleteActive(int index, ProfileWidget widget)
		{
			this._clickedIndex = new int?(index);
			this._clickedWidget = widget;
			this.deleteConfirmPanel.GetComponent<CanvasGroup>().alpha = 0f;
			this.deleteConfirmPanel.SetActive(true);
			this.deleteConfirmPanel.GetComponent<CanvasGroup>().interactable = true;
			this.deleteConfirmPanel.GetComponent<CanvasGroup>().DOFade(1f, 0.5f).From(0f, true, false);
			this.deleteConfirm.onClick.RemoveAllListeners();
			this.deleteConfirm.onClick.AddListener(new UnityAction(this.OnProfileDeleteConfirm));
		}
		private void OnProfileDeleteConfirm()
		{
			ProfileSaveData profile = this._clickedWidget.Profile;
			int? clickedIndex = this._clickedIndex;
			if (clickedIndex != null)
			{
				int valueOrDefault = clickedIndex.GetValueOrDefault();
				GameMaster.DeleteProfile(valueOrDefault);
			}
			this.OnProfileDeleteCancel();
			this.RefreshSaveDatas();
		}
		private void OnProfileDeleteCancel()
		{
			this._clickedIndex = default(int?);
			this._clickedWidget = null;
			this.deleteConfirmPanel.GetComponent<CanvasGroup>().interactable = false;
			TweenerCore<float, float, FloatOptions> tweenerCore = this.deleteConfirmPanel.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).From(1f, true, false);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, delegate
			{
				this.deleteConfirmPanel.SetActive(false);
			});
		}
		private void OnProfileEditActive(int index, ProfileWidget widget)
		{
			this._clickedIndex = new int?(index);
			this._clickedWidget = widget;
			this.inputField.text = "";
			this.nameInputRoot.gameObject.SetActive(true);
			this.nameInputRoot.GetComponent<CanvasGroup>().interactable = true;
			this.nameInputRoot.GetComponent<CanvasGroup>().DOFade(1f, 0.3f).From(0f, true, false);
		}
		public Sprite GetHeadSprite(string spriteName)
		{
			Sprite sprite;
			if (!this.headPicList.TryGetValue(spriteName, out sprite))
			{
				return this.headPicList["null"];
			}
			return sprite;
		}
		void IInputActionHandler.OnConfirm()
		{
			if (this.nameInputRoot.activeSelf)
			{
				this.ConfirmInput();
				return;
			}
			if (this.deleteConfirmPanel.activeSelf)
			{
				this.OnProfileDeleteConfirm();
			}
		}
		void IInputActionHandler.OnCancel()
		{
			if (this.nameInputRoot.activeSelf)
			{
				this.CancelInput();
				return;
			}
			if (this.deleteConfirmPanel.activeSelf)
			{
				this.OnProfileDeleteCancel();
				return;
			}
			base.Hide();
		}
		[SerializeField]
		private ProfileWidget widget0;
		[SerializeField]
		private ProfileWidget widget1;
		[SerializeField]
		private ProfileWidget widget2;
		[SerializeField]
		private GameObject deleteConfirmPanel;
		[SerializeField]
		private GameObject nameInputRoot;
		[SerializeField]
		private TMP_InputField inputField;
		[SerializeField]
		private Button confirm;
		[SerializeField]
		private Button cancel;
		[SerializeField]
		private Button deleteConfirm;
		[SerializeField]
		private Button deleteCancel;
		[SerializeField]
		private AssociationList<string, Sprite> headPicList;
		private int? _clickedIndex;
		private ProfileWidget _clickedWidget;
		private bool _isCreate;
		private CanvasGroup _canvasGroup;
	}
}
