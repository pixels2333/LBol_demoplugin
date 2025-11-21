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
	// Token: 0x020000A9 RID: 169
	public class ProfilePanel : UiPanel, IInputActionHandler
	{
		// Token: 0x06000950 RID: 2384 RVA: 0x0002FACC File Offset: 0x0002DCCC
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

		// Token: 0x06000951 RID: 2385 RVA: 0x0002FC08 File Offset: 0x0002DE08
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

		// Token: 0x06000952 RID: 2386 RVA: 0x0002FC9D File Offset: 0x0002DE9D
		protected override void OnShowing()
		{
			this.RefreshSaveDatas();
			this._canvasGroup.interactable = true;
			UiManager.PushActionHandler(this);
		}

		// Token: 0x06000953 RID: 2387 RVA: 0x0002FCB8 File Offset: 0x0002DEB8
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

		// Token: 0x06000954 RID: 2388 RVA: 0x0002FD28 File Offset: 0x0002DF28
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

		// Token: 0x06000955 RID: 2389 RVA: 0x0002FE1C File Offset: 0x0002E01C
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

		// Token: 0x06000956 RID: 2390 RVA: 0x0002FF44 File Offset: 0x0002E144
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

		// Token: 0x06000957 RID: 2391 RVA: 0x0002FFCC File Offset: 0x0002E1CC
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

		// Token: 0x06000958 RID: 2392 RVA: 0x00030008 File Offset: 0x0002E208
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

		// Token: 0x06000959 RID: 2393 RVA: 0x000300B0 File Offset: 0x0002E2B0
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

		// Token: 0x0600095A RID: 2394 RVA: 0x000300F4 File Offset: 0x0002E2F4
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

		// Token: 0x0600095B RID: 2395 RVA: 0x0003016C File Offset: 0x0002E36C
		private void OnProfileEditActive(int index, ProfileWidget widget)
		{
			this._clickedIndex = new int?(index);
			this._clickedWidget = widget;
			this.inputField.text = "";
			this.nameInputRoot.gameObject.SetActive(true);
			this.nameInputRoot.GetComponent<CanvasGroup>().interactable = true;
			this.nameInputRoot.GetComponent<CanvasGroup>().DOFade(1f, 0.3f).From(0f, true, false);
		}

		// Token: 0x0600095C RID: 2396 RVA: 0x000301E8 File Offset: 0x0002E3E8
		public Sprite GetHeadSprite(string spriteName)
		{
			Sprite sprite;
			if (!this.headPicList.TryGetValue(spriteName, out sprite))
			{
				return this.headPicList["null"];
			}
			return sprite;
		}

		// Token: 0x0600095D RID: 2397 RVA: 0x00030217 File Offset: 0x0002E417
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

		// Token: 0x0600095E RID: 2398 RVA: 0x00030240 File Offset: 0x0002E440
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

		// Token: 0x040006E0 RID: 1760
		[SerializeField]
		private ProfileWidget widget0;

		// Token: 0x040006E1 RID: 1761
		[SerializeField]
		private ProfileWidget widget1;

		// Token: 0x040006E2 RID: 1762
		[SerializeField]
		private ProfileWidget widget2;

		// Token: 0x040006E3 RID: 1763
		[SerializeField]
		private GameObject deleteConfirmPanel;

		// Token: 0x040006E4 RID: 1764
		[SerializeField]
		private GameObject nameInputRoot;

		// Token: 0x040006E5 RID: 1765
		[SerializeField]
		private TMP_InputField inputField;

		// Token: 0x040006E6 RID: 1766
		[SerializeField]
		private Button confirm;

		// Token: 0x040006E7 RID: 1767
		[SerializeField]
		private Button cancel;

		// Token: 0x040006E8 RID: 1768
		[SerializeField]
		private Button deleteConfirm;

		// Token: 0x040006E9 RID: 1769
		[SerializeField]
		private Button deleteCancel;

		// Token: 0x040006EA RID: 1770
		[SerializeField]
		private AssociationList<string, Sprite> headPicList;

		// Token: 0x040006EB RID: 1771
		private int? _clickedIndex;

		// Token: 0x040006EC RID: 1772
		private ProfileWidget _clickedWidget;

		// Token: 0x040006ED RID: 1773
		private bool _isCreate;

		// Token: 0x040006EE RID: 1774
		private CanvasGroup _canvasGroup;
	}
}
