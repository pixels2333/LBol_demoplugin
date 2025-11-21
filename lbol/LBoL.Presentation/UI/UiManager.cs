using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Cysharp.Threading.Tasks;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Presentation.Bullet;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

namespace LBoL.Presentation.UI
{
	// Token: 0x02000033 RID: 51
	public sealed class UiManager : MonoBehaviour
	{
		// Token: 0x1700008C RID: 140
		// (get) Token: 0x06000363 RID: 867 RVA: 0x0000E938 File Offset: 0x0000CB38
		public static UiManager Instance
		{
			get
			{
				UiManager instance = UiManager._instance;
				if (instance == null || !instance)
				{
					throw new InvalidOperationException("UiManager is not instantiated.");
				}
				return instance;
			}
		}

		// Token: 0x1700008D RID: 141
		// (get) Token: 0x06000364 RID: 868 RVA: 0x0000E962 File Offset: 0x0000CB62
		public static bool IsInitialized
		{
			get
			{
				return UiManager._instance;
			}
		}

		// Token: 0x1700008E RID: 142
		// (get) Token: 0x06000365 RID: 869 RVA: 0x0000E96E File Offset: 0x0000CB6E
		// (set) Token: 0x06000366 RID: 870 RVA: 0x0000E976 File Offset: 0x0000CB76
		public InputAction DebugMenuAction { get; private set; }

		// Token: 0x1700008F RID: 143
		// (get) Token: 0x06000367 RID: 871 RVA: 0x0000E97F File Offset: 0x0000CB7F
		// (set) Token: 0x06000368 RID: 872 RVA: 0x0000E987 File Offset: 0x0000CB87
		public InputAction DebugConsoleAction { get; private set; }

		// Token: 0x17000090 RID: 144
		// (get) Token: 0x06000369 RID: 873 RVA: 0x0000E990 File Offset: 0x0000CB90
		// (set) Token: 0x0600036A RID: 874 RVA: 0x0000E998 File Offset: 0x0000CB98
		public InputAction BattleLog { get; private set; }

		// Token: 0x0600036B RID: 875 RVA: 0x0000E9A4 File Offset: 0x0000CBA4
		private void Awake()
		{
			UiManager._instance = this;
			this._canvas = base.GetComponent<Canvas>();
			this.inputBlocker.SetActive(false);
			InputActionMap inputActionMap = this.inputActions.FindActionMap("Debug", false);
			this.DebugMenuAction = inputActionMap["DebugMenu"];
			this.DebugConsoleAction = inputActionMap["DebugConsole"];
			InputAction inputAction = inputActionMap["DebugSwitchHideUI"];
			InputActionMap inputActionMap2 = this.inputActions.FindActionMap("UI", false);
			inputActionMap2.Enable();
			InputAction inputAction2 = inputActionMap2["Confirm"];
			InputAction inputAction3 = inputActionMap2["Cancel"];
			InputAction inputAction4 = inputActionMap2["Navigate"];
			InputAction inputAction5 = inputActionMap2["RightClick"];
			inputAction2.performed += delegate(InputAction.CallbackContext ctx)
			{
				IInputActionHandler inputActionHandler = this.TryGetInputHandler(ctx, false);
				if (inputActionHandler == null)
				{
					return;
				}
				inputActionHandler.OnConfirm();
			};
			inputAction3.performed += delegate(InputAction.CallbackContext ctx)
			{
				IInputActionHandler inputActionHandler2 = this.TryGetInputHandler(ctx, false);
				if (inputActionHandler2 == null)
				{
					return;
				}
				inputActionHandler2.OnCancel();
			};
			inputAction5.canceled += delegate(InputAction.CallbackContext ctx)
			{
				this.RightClicked();
			};
			inputAction4.performed += delegate(InputAction.CallbackContext ctx)
			{
				IInputActionHandler inputActionHandler3 = this.TryGetInputHandler(ctx, false);
				if (inputActionHandler3 == null)
				{
					return;
				}
				float num;
				float num2;
				ctx.ReadValue<Vector2>().Deconstruct(out num, out num2);
				float num3 = num;
				float num4 = num2;
				NavigateDirection navigateDirection = ((Math.Abs(num3) > Math.Abs(num4)) ? ((num3 < 0f) ? NavigateDirection.Left : NavigateDirection.Right) : ((num4 > 0f) ? NavigateDirection.Up : NavigateDirection.Down));
				inputActionHandler3.OnNavigate(navigateDirection);
			};
			this._shortcutsMap = this.inputActions.FindActionMap("Shortcuts", false);
			this._shortcutsMap.Enable();
			this._selectNs = Enumerable.ToArray<InputAction>(Enumerable.Select<int, InputAction>(Enumerable.Range(1, 12), (int i) => this._shortcutsMap["Select" + i.ToString()]));
			this._poolColor = new SortedDictionary<ManaColor, InputAction>();
			foreach (ManaColor manaColor in ManaColors.WUBRGCP)
			{
				this._poolColor.Add(manaColor, this._shortcutsMap["Pool" + manaColor.ToShortName().ToString()]);
			}
			this._useUs = this._shortcutsMap["UseUs"];
			this._endTurn = this._shortcutsMap["EndTurn"];
			this._toggleBaseDeck = this._shortcutsMap["ToggleBaseDeck"];
			this._toggleDrawZone = this._shortcutsMap["ToggleDrawZone"];
			this._toggleDiscardZone = this._shortcutsMap["ToggleDiscardZone"];
			this._toggleExileZone = this._shortcutsMap["ToggleExileZone"];
			this._toggleMap = this._shortcutsMap["ToggleMap"];
			this._toogleMinimize = this._shortcutsMap["ToggleMinimize"];
			this._skipDialogAction = this._shortcutsMap["SkipDialog"];
			this._showEnemyMoveOrder = this._shortcutsMap["ShowEnemyMoveOrder"];
			this.BattleLog = this._shortcutsMap["BattleLog"];
			this._skipDialogAction.performed += delegate(InputAction.CallbackContext ctx)
			{
				IInputActionHandler inputActionHandler4 = this.TryGetInputHandler(ctx, true);
				if (inputActionHandler4 == null)
				{
					return;
				}
				inputActionHandler4.BeginSkipDialog();
			};
			this._skipDialogAction.canceled += delegate(InputAction.CallbackContext ctx)
			{
				IInputActionHandler inputActionHandler5 = this.TryGetInputHandler(ctx, true);
				if (inputActionHandler5 == null)
				{
					return;
				}
				inputActionHandler5.EndSkipDialog();
			};
			this._showEnemyMoveOrder.performed += delegate(InputAction.CallbackContext ctx)
			{
				IInputActionHandler inputActionHandler6 = this.TryGetInputHandler(ctx, true);
				if (inputActionHandler6 == null)
				{
					return;
				}
				inputActionHandler6.BeginShowEnemyMoveOrder();
			};
			this._showEnemyMoveOrder.canceled += delegate(InputAction.CallbackContext ctx)
			{
				IInputActionHandler inputActionHandler7 = this.TryGetInputHandler(ctx, true);
				if (inputActionHandler7 == null)
				{
					return;
				}
				inputActionHandler7.EndShowEnemyMoveOrder();
			};
			foreach (ValueTuple<int, InputAction> valueTuple in this._selectNs.WithIndices<InputAction>())
			{
				int i = valueTuple.Item1;
				valueTuple.Item2.performed += delegate(InputAction.CallbackContext ctx)
				{
					IInputActionHandler inputActionHandler8 = this.TryGetInputHandler(ctx, true);
					if (inputActionHandler8 == null)
					{
						return;
					}
					inputActionHandler8.OnSelect(i);
				};
			}
			foreach (KeyValuePair<ManaColor, InputAction> keyValuePair in this._poolColor)
			{
				ManaColor manaColor2;
				InputAction inputAction6;
				keyValuePair.Deconstruct(ref manaColor2, ref inputAction6);
				ManaColor c = manaColor2;
				inputAction6.performed += delegate(InputAction.CallbackContext ctx)
				{
					IInputActionHandler inputActionHandler9 = this.TryGetInputHandler(ctx, true);
					if (inputActionHandler9 == null)
					{
						return;
					}
					inputActionHandler9.OnPoolMana(c);
				};
			}
			this._useUs.performed += delegate(InputAction.CallbackContext ctx)
			{
				IInputActionHandler inputActionHandler10 = this.TryGetInputHandler(ctx, true);
				if (inputActionHandler10 == null)
				{
					return;
				}
				inputActionHandler10.OnUseUs();
			};
			this._endTurn.performed += delegate(InputAction.CallbackContext ctx)
			{
				IInputActionHandler inputActionHandler11 = this.TryGetInputHandler(ctx, true);
				if (inputActionHandler11 == null)
				{
					return;
				}
				inputActionHandler11.OnEndTurn();
			};
			this._toggleBaseDeck.performed += delegate(InputAction.CallbackContext ctx)
			{
				IInputActionHandler inputActionHandler12 = this.TryGetInputHandler(ctx, true);
				if (inputActionHandler12 == null)
				{
					return;
				}
				inputActionHandler12.OnToggleBaseDeck();
			};
			this._toggleDrawZone.performed += delegate(InputAction.CallbackContext ctx)
			{
				IInputActionHandler inputActionHandler13 = this.TryGetInputHandler(ctx, true);
				if (inputActionHandler13 == null)
				{
					return;
				}
				inputActionHandler13.OnToggleDrawZone();
			};
			this._toggleDiscardZone.performed += delegate(InputAction.CallbackContext ctx)
			{
				IInputActionHandler inputActionHandler14 = this.TryGetInputHandler(ctx, true);
				if (inputActionHandler14 == null)
				{
					return;
				}
				inputActionHandler14.OnToggleDiscardZone();
			};
			this._toggleExileZone.performed += delegate(InputAction.CallbackContext ctx)
			{
				IInputActionHandler inputActionHandler15 = this.TryGetInputHandler(ctx, true);
				if (inputActionHandler15 == null)
				{
					return;
				}
				inputActionHandler15.OnToggleExileZone();
			};
			this._toggleMap.performed += delegate(InputAction.CallbackContext ctx)
			{
				IInputActionHandler inputActionHandler16 = this.TryGetInputHandler(ctx, true);
				if (inputActionHandler16 == null)
				{
					return;
				}
				inputActionHandler16.OnToggleMap();
			};
			this._toogleMinimize.performed += delegate(InputAction.CallbackContext ctx)
			{
				IInputActionHandler inputActionHandler17 = this.TryGetInputHandler(ctx, true);
				if (inputActionHandler17 == null)
				{
					return;
				}
				inputActionHandler17.OnToggleMinimize();
			};
		}

		// Token: 0x0600036C RID: 876 RVA: 0x0000EE4C File Offset: 0x0000D04C
		public static string GetHandShortcutDisplayString(int index)
		{
			InputAction inputAction = UiManager.Instance._selectNs.TryGetValue(index);
			if (inputAction == null)
			{
				return "Null";
			}
			return inputAction.GetBindingDisplayString((InputBinding.DisplayStringOptions)0, null);
		}

		// Token: 0x0600036D RID: 877 RVA: 0x0000EE7B File Offset: 0x0000D07B
		public static string GetSkipString()
		{
			return UiManager.Instance._skipDialogAction.GetBindingDisplayString((InputBinding.DisplayStringOptions)0, null);
		}

		// Token: 0x0600036E RID: 878 RVA: 0x0000EE8E File Offset: 0x0000D08E
		public static IEnumerable<ActionMapping> EnumerateRebindableActions()
		{
			UiManager instance = UiManager.Instance;
			foreach (ValueTuple<int, InputAction> valueTuple in instance._selectNs.WithIndices<InputAction>())
			{
				int item = valueTuple.Item1;
				InputAction item2 = valueTuple.Item2;
				yield return new ActionMapping
				{
					Id = "Select" + (item + 1).ToString(),
					InputAction = item2
				};
			}
			IEnumerator<ValueTuple<int, InputAction>> enumerator = null;
			foreach (KeyValuePair<ManaColor, InputAction> keyValuePair in instance._poolColor)
			{
				ManaColor manaColor;
				InputAction inputAction;
				keyValuePair.Deconstruct(ref manaColor, ref inputAction);
				ManaColor manaColor2 = manaColor;
				InputAction inputAction2 = inputAction;
				yield return new ActionMapping
				{
					Id = "Pool" + manaColor2.ToShortName().ToString(),
					InputAction = inputAction2
				};
			}
			SortedDictionary<ManaColor, InputAction>.Enumerator enumerator2 = default(SortedDictionary<ManaColor, InputAction>.Enumerator);
			yield return new ActionMapping
			{
				Id = "UseUs",
				InputAction = instance._useUs
			};
			yield return new ActionMapping
			{
				Id = "EndTurn",
				InputAction = instance._endTurn
			};
			yield return new ActionMapping
			{
				Id = "ToggleBaseDeck",
				InputAction = instance._toggleBaseDeck
			};
			yield return new ActionMapping
			{
				Id = "ToggleDrawZone",
				InputAction = instance._toggleDrawZone
			};
			yield return new ActionMapping
			{
				Id = "ToggleDiscardZone",
				InputAction = instance._toggleDiscardZone
			};
			yield return new ActionMapping
			{
				Id = "ToggleExileZone",
				InputAction = instance._toggleExileZone
			};
			yield return new ActionMapping
			{
				Id = "ToggleMap",
				InputAction = instance._toggleMap
			};
			yield return new ActionMapping
			{
				Id = "SkipDialog",
				InputAction = instance._skipDialogAction
			};
			yield return new ActionMapping
			{
				Id = "ShowEnemyMoveOrder",
				InputAction = instance._showEnemyMoveOrder
			};
			yield break;
			yield break;
		}

		// Token: 0x0600036F RID: 879 RVA: 0x0000EE98 File Offset: 0x0000D098
		public static void LoadKeyboardBindings(IReadOnlyDictionary<string, string> keybindings)
		{
			foreach (ActionMapping actionMapping in UiManager.EnumerateRebindableActions())
			{
				InputAction inputAction = actionMapping.InputAction;
				int bindingIndex = inputAction.GetBindingIndex("Keyboard&Mouse", null);
				string text;
				if (bindingIndex < 0)
				{
					Debug.LogError("Cannot get keyboard binding for " + inputAction.name);
				}
				else if (keybindings.TryGetValue(actionMapping.Id, ref text) && text != null)
				{
					inputAction.ApplyBindingOverride(bindingIndex, text);
				}
				else
				{
					inputAction.RemoveBindingOverride(bindingIndex);
				}
			}
		}

		// Token: 0x14000003 RID: 3
		// (add) Token: 0x06000370 RID: 880 RVA: 0x0000EF34 File Offset: 0x0000D134
		// (remove) Token: 0x06000371 RID: 881 RVA: 0x0000EF6C File Offset: 0x0000D16C
		public event Action OnActionHandlerChanged;

		// Token: 0x06000372 RID: 882 RVA: 0x0000EFA4 File Offset: 0x0000D1A4
		private IInputActionHandler TryGetInputHandler(InputAction.CallbackContext context, bool disableIfKeyboardDisabled = false)
		{
			if (disableIfKeyboardDisabled && context.control is KeyControl && !GameMaster.EnableKeyboard)
			{
				return null;
			}
			if (this.inputBlocker.gameObject.activeSelf)
			{
				return null;
			}
			IInputActionHandler inputActionHandler;
			if (!this._actionHandlerStack.TryPeek(ref inputActionHandler))
			{
				return null;
			}
			return inputActionHandler;
		}

		// Token: 0x06000373 RID: 883 RVA: 0x0000EFF4 File Offset: 0x0000D1F4
		public IInputActionHandler TryGetInputHandler()
		{
			if (this.inputBlocker.gameObject.activeSelf)
			{
				return null;
			}
			IInputActionHandler inputActionHandler;
			if (!this._actionHandlerStack.TryPeek(ref inputActionHandler))
			{
				return null;
			}
			return inputActionHandler;
		}

		// Token: 0x06000374 RID: 884 RVA: 0x0000F028 File Offset: 0x0000D228
		public static void PushActionHandler(IInputActionHandler handler)
		{
			IInputActionHandler inputActionHandler;
			if (UiManager.Instance._actionHandlerStack.TryPeek(ref inputActionHandler))
			{
				inputActionHandler.Reset();
			}
			UiManager.Instance._actionHandlerStack.Push(handler);
			Action onActionHandlerChanged = UiManager.Instance.OnActionHandlerChanged;
			if (onActionHandlerChanged == null)
			{
				return;
			}
			onActionHandlerChanged.Invoke();
		}

		// Token: 0x06000375 RID: 885 RVA: 0x0000F074 File Offset: 0x0000D274
		public static void PopActionHandler(IInputActionHandler handler)
		{
			IInputActionHandler inputActionHandler = UiManager.Instance._actionHandlerStack.Pop();
			if (inputActionHandler != handler)
			{
				Debug.LogError("[UiManager] ActionHandler top " + inputActionHandler.GetType().Name + " is not " + handler.GetType().Name);
			}
			Action onActionHandlerChanged = UiManager.Instance.OnActionHandlerChanged;
			if (onActionHandlerChanged == null)
			{
				return;
			}
			onActionHandlerChanged.Invoke();
		}

		// Token: 0x06000376 RID: 886 RVA: 0x0000F0D3 File Offset: 0x0000D2D3
		public static void ClearActionHandler()
		{
			UiManager.Instance._actionHandlerStack.Clear();
			Action onActionHandlerChanged = UiManager.Instance.OnActionHandlerChanged;
			if (onActionHandlerChanged == null)
			{
				return;
			}
			onActionHandlerChanged.Invoke();
		}

		// Token: 0x06000377 RID: 887 RVA: 0x0000F0F8 File Offset: 0x0000D2F8
		public static void EnterGameRun(GameRunController gameRun)
		{
			foreach (UiPanelBase uiPanelBase in UiManager.Instance._panelTable.Values)
			{
				uiPanelBase.EnterGameRun(gameRun);
			}
		}

		// Token: 0x06000378 RID: 888 RVA: 0x0000F154 File Offset: 0x0000D354
		public static void LeaveGameRun()
		{
			foreach (UiPanelBase uiPanelBase in UiManager.Instance._panelTable.Values)
			{
				uiPanelBase.LeaveGameRun();
			}
		}

		// Token: 0x06000379 RID: 889 RVA: 0x0000F1B0 File Offset: 0x0000D3B0
		public static void EnterBattle(BattleController battle)
		{
			foreach (UiPanelBase uiPanelBase in UiManager.Instance._panelTable.Values)
			{
				uiPanelBase.EnterBattle(battle);
			}
		}

		// Token: 0x0600037A RID: 890 RVA: 0x0000F20C File Offset: 0x0000D40C
		public static void LeaveBattle()
		{
			GunManager.ClearAll();
			foreach (UiPanelBase uiPanelBase in UiManager.Instance._panelTable.Values)
			{
				uiPanelBase.LeaveBattle();
			}
		}

		// Token: 0x0600037B RID: 891 RVA: 0x0000F26C File Offset: 0x0000D46C
		private async UniTask<UiBase> InternalLoadPanelAsync(Type panelType, string parentFolder, bool show)
		{
			if (this._panelTable.ContainsKey(panelType))
			{
				throw new InvalidOperationException("Already loaded panel '" + panelType.Name + "'");
			}
			UiPanelBase component = (await ResourcesHelper.LoadUiPanelAsync(parentFolder, panelType.Name)).GetComponent<UiPanelBase>();
			RectTransform rectTransform;
			switch (component.Layer)
			{
			case PanelLayer.Normal:
				rectTransform = this.normalLayer;
				break;
			case PanelLayer.Base:
				rectTransform = this.baseLayer;
				break;
			case PanelLayer.Bottom:
				rectTransform = this.bottomLayer;
				break;
			case PanelLayer.VisualNovel:
				rectTransform = this.visualNovelLayer;
				break;
			case PanelLayer.Top:
				rectTransform = this.topLayer;
				break;
			case PanelLayer.Topmost:
				rectTransform = this.topmostLayer;
				break;
			case PanelLayer.Tooltip:
				rectTransform = this.tooltipLayer;
				break;
			default:
				throw new ArgumentOutOfRangeException(string.Format("Invalid panel layer {0}", component.Layer));
			}
			RectTransform rectTransform2 = rectTransform;
			UiPanelBase panel = Object.Instantiate<UiPanelBase>(component, rectTransform2, false);
			panel.name = panelType.Name;
			await panel.InitializeAsync();
			await panel.CustomLocalizationAsync();
			if (!show)
			{
				panel.gameObject.SetActive(false);
			}
			this._panelTable.Add(panelType, panel);
			Type[] array;
			if (panelType.TryGetGenericBaseTypeArguments(typeof(UiAdventurePanel<>), out array))
			{
				GameMaster.RegisterExtraAdventureHandlers(array[0], (IAdventureHandler)panel);
			}
			return panel;
		}

		// Token: 0x0600037C RID: 892 RVA: 0x0000F2C8 File Offset: 0x0000D4C8
		public static async UniTask<TPanel> LoadPanelAsync<TPanel>([MaybeNull] string parentFolder, bool show = false) where TPanel : UiPanelBase
		{
			UniTask<UiBase>.Awaiter awaiter = UiManager.Instance.InternalLoadPanelAsync(typeof(TPanel), parentFolder, show).GetAwaiter();
			if (!awaiter.IsCompleted)
			{
				await awaiter;
				UniTask<UiBase>.Awaiter awaiter2;
				awaiter = awaiter2;
				awaiter2 = default(UniTask<UiBase>.Awaiter);
			}
			return (TPanel)((object)awaiter.GetResult());
		}

		// Token: 0x0600037D RID: 893 RVA: 0x0000F314 File Offset: 0x0000D514
		public static async UniTask<TPanel> LoadPanelAsync<TPanel>(bool show = false) where TPanel : UiPanelBase
		{
			UniTask<UiBase>.Awaiter awaiter = UiManager.Instance.InternalLoadPanelAsync(typeof(TPanel), null, show).GetAwaiter();
			if (!awaiter.IsCompleted)
			{
				await awaiter;
				UniTask<UiBase>.Awaiter awaiter2;
				awaiter = awaiter2;
				awaiter2 = default(UniTask<UiBase>.Awaiter);
			}
			return (TPanel)((object)awaiter.GetResult());
		}

		// Token: 0x0600037E RID: 894 RVA: 0x0000F358 File Offset: 0x0000D558
		private void InternalUnloadPanel(Type panelType)
		{
			UiPanelBase uiPanelBase;
			if (!this._panelTable.TryGetValue(panelType, ref uiPanelBase))
			{
				throw new InvalidOperationException("Cannot unload panel '" + panelType.Name + "': no such panel found");
			}
			this._panelTable.Remove(panelType);
			Type[] array;
			if (panelType.TryGetGenericBaseTypeArguments(typeof(UiAdventurePanel<>), out array))
			{
				GameMaster.UnregisterExtraAdventureHandlers(array[0], (IAdventureHandler)uiPanelBase);
			}
			Object.Destroy(uiPanelBase.gameObject);
			Resources.UnloadUnusedAssets();
		}

		// Token: 0x0600037F RID: 895 RVA: 0x0000F3D0 File Offset: 0x0000D5D0
		public static void UnloadPanel(Type type)
		{
			UiManager.Instance.InternalUnloadPanel(type);
		}

		// Token: 0x06000380 RID: 896 RVA: 0x0000F3DD File Offset: 0x0000D5DD
		public static void UnloadPanel<TPanel>() where TPanel : UiPanelBase
		{
			UiManager.Instance.InternalUnloadPanel(typeof(TPanel));
		}

		// Token: 0x06000381 RID: 897 RVA: 0x0000F3F4 File Offset: 0x0000D5F4
		private async UniTask<UiDialogBase> InternalLoadDialogAsync(Type dialogType, [MaybeNull] string parentFolder)
		{
			if (this._dialogTable.ContainsKey(dialogType))
			{
				throw new InvalidOperationException("Already loaded dialog '" + dialogType.Name + "'");
			}
			UiDialogBase component = (await ResourcesHelper.LoadUiDialogAsync(parentFolder, dialogType.Name)).GetComponent<UiDialogBase>();
			UiDialogBase dialog = Object.Instantiate<UiDialogBase>(component, this.dialogLayer, false);
			dialog.name = dialogType.Name;
			await dialog.InitializeAsync();
			await dialog.CustomLocalizationAsync();
			dialog.gameObject.SetActive(false);
			this._dialogTable.Add(dialogType, dialog);
			return dialog;
		}

		// Token: 0x06000382 RID: 898 RVA: 0x0000F448 File Offset: 0x0000D648
		public static async UniTask<TDialog> LoadDialogAsync<TDialog>(string parentFolder = null) where TDialog : UiDialogBase
		{
			UniTask<UiDialogBase>.Awaiter awaiter = UiManager.Instance.InternalLoadDialogAsync(typeof(TDialog), parentFolder).GetAwaiter();
			if (!awaiter.IsCompleted)
			{
				await awaiter;
				UniTask<UiDialogBase>.Awaiter awaiter2;
				awaiter = awaiter2;
				awaiter2 = default(UniTask<UiDialogBase>.Awaiter);
			}
			return (TDialog)((object)awaiter.GetResult());
		}

		// Token: 0x06000383 RID: 899 RVA: 0x0000F48C File Offset: 0x0000D68C
		private UiPanelBase InternalGetPanel(Type panelType)
		{
			UiPanelBase uiPanelBase;
			if (!this._panelTable.TryGetValue(panelType, ref uiPanelBase))
			{
				throw new InvalidOperationException("Cannot get panel '" + panelType.Name + "', not loaded yet.");
			}
			return uiPanelBase;
		}

		// Token: 0x06000384 RID: 900 RVA: 0x0000F4C5 File Offset: 0x0000D6C5
		public static TPanel GetPanel<TPanel>() where TPanel : UiPanelBase
		{
			return (TPanel)((object)UiManager.Instance.InternalGetPanel(typeof(TPanel)));
		}

		// Token: 0x06000385 RID: 901 RVA: 0x0000F4E0 File Offset: 0x0000D6E0
		public static void Show<TPanel>() where TPanel : UiPanel
		{
			UiManager.GetPanel<TPanel>().Show();
		}

		// Token: 0x06000386 RID: 902 RVA: 0x0000F4F1 File Offset: 0x0000D6F1
		public static void Hide<TPanel>(bool transition = true) where TPanel : UiPanelBase
		{
			UiManager.GetPanel<TPanel>().Hide(transition);
		}

		// Token: 0x06000387 RID: 903 RVA: 0x0000F504 File Offset: 0x0000D704
		private UiDialogBase InternalGetDialog(Type dialogType)
		{
			UiDialogBase uiDialogBase;
			if (!this._dialogTable.TryGetValue(dialogType, ref uiDialogBase))
			{
				throw new InvalidOperationException("Cannot get dialog '" + dialogType.Name + "', not loaded yet.");
			}
			return uiDialogBase;
		}

		// Token: 0x06000388 RID: 904 RVA: 0x0000F53D File Offset: 0x0000D73D
		public static TDialog GetDialog<TDialog>() where TDialog : UiDialogBase
		{
			return (TDialog)((object)UiManager.Instance.InternalGetDialog(typeof(TDialog)));
		}

		// Token: 0x06000389 RID: 905 RVA: 0x0000F558 File Offset: 0x0000D758
		internal static void ShowDialogCheck(UiDialogBase dialog)
		{
			UiManager instance = UiManager.Instance;
			if (instance._currentDialog != null)
			{
				throw new InvalidOperationException(string.Concat(new string[]
				{
					"Cannot show '",
					dialog.GetType().Name,
					"' while showing '",
					instance._currentDialog.GetType().Name,
					"'"
				}));
			}
			instance._currentDialog = dialog;
		}

		// Token: 0x0600038A RID: 906 RVA: 0x0000F5C4 File Offset: 0x0000D7C4
		internal static void HideDialogCheck(UiDialogBase dialog)
		{
			UiManager instance = UiManager.Instance;
			if (instance._currentDialog != dialog)
			{
				Debug.LogWarning(string.Format("Hiding dialog '{0}' while current dialog == '{1}'", dialog.GetType().Name, instance._currentDialog.GetType()));
			}
			instance._currentDialog = null;
		}

		// Token: 0x0600038B RID: 907 RVA: 0x0000F60C File Offset: 0x0000D80C
		internal static IEnumerable<UiBase> EnumerateAll()
		{
			if (UiManager._instance == null)
			{
				yield break;
			}
			foreach (KeyValuePair<Type, UiPanelBase> keyValuePair in UiManager._instance._panelTable)
			{
				yield return keyValuePair.Value;
			}
			Dictionary<Type, UiPanelBase>.Enumerator enumerator = default(Dictionary<Type, UiPanelBase>.Enumerator);
			foreach (KeyValuePair<Type, UiDialogBase> keyValuePair2 in UiManager._instance._dialogTable)
			{
				yield return keyValuePair2.Value;
			}
			Dictionary<Type, UiDialogBase>.Enumerator enumerator2 = default(Dictionary<Type, UiDialogBase>.Enumerator);
			yield break;
			yield break;
		}

		// Token: 0x17000091 RID: 145
		// (get) Token: 0x0600038C RID: 908 RVA: 0x0000F615 File Offset: 0x0000D815
		// (set) Token: 0x0600038D RID: 909 RVA: 0x0000F61C File Offset: 0x0000D81C
		public static bool IsShowingLoading { get; private set; }

		// Token: 0x0600038E RID: 910 RVA: 0x0000F624 File Offset: 0x0000D824
		public static async UniTask ShowLoading(float duration = 0.5f)
		{
			if (!UiManager.IsShowingLoading)
			{
				UiManager.IsShowingLoading = true;
				UiManager instance = UiManager.Instance;
				instance.inputBlocker.SetActive(true);
				await instance.loadingScreen.Show(duration);
			}
		}

		// Token: 0x0600038F RID: 911 RVA: 0x0000F668 File Offset: 0x0000D868
		public static async UniTask HideLoading(float duration = 0.5f)
		{
			if (UiManager.IsShowingLoading)
			{
				UiManager.IsShowingLoading = false;
				UiManager instance = UiManager.Instance;
				await UiManager.Instance.loadingScreen.Hide(duration);
				instance.inputBlocker.SetActive(false);
				Action onActionHandlerChanged = UiManager.Instance.OnActionHandlerChanged;
				if (onActionHandlerChanged != null)
				{
					onActionHandlerChanged.Invoke();
				}
				UiManager.IsShowingLoading = false;
			}
		}

		// Token: 0x06000390 RID: 912 RVA: 0x0000F6AC File Offset: 0x0000D8AC
		public static RawImage CreateHighlightRawImage(string name)
		{
			GameObject gameObject = Utils.CreateGameObject(UiManager.Instance.highlightLayer, name);
			RectTransform rectTransform = (RectTransform)gameObject.transform;
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
			RawImage rawImage = gameObject.AddComponent<RawImage>();
			rawImage.raycastTarget = false;
			return rawImage;
		}

		// Token: 0x17000092 RID: 146
		// (get) Token: 0x06000391 RID: 913 RVA: 0x0000F70B File Offset: 0x0000D90B
		// (set) Token: 0x06000392 RID: 914 RVA: 0x0000F713 File Offset: 0x0000D913
		private bool IsHidingUI
		{
			get
			{
				return this._isHidingUI;
			}
			set
			{
				this._isHidingUI = value;
				this._canvas.enabled = !value;
			}
		}

		// Token: 0x17000093 RID: 147
		// (get) Token: 0x06000393 RID: 915 RVA: 0x0000F72B File Offset: 0x0000D92B
		// (set) Token: 0x06000394 RID: 916 RVA: 0x0000F73C File Offset: 0x0000D93C
		public static bool IsBlockingInput
		{
			get
			{
				return UiManager._instance.inputBlocker.activeSelf;
			}
			set
			{
				UiManager._instance.inputBlocker.SetActive(value);
			}
		}

		// Token: 0x17000094 RID: 148
		// (get) Token: 0x06000395 RID: 917 RVA: 0x0000F74E File Offset: 0x0000D94E
		// (set) Token: 0x06000396 RID: 918 RVA: 0x0000F755 File Offset: 0x0000D955
		public static bool HoveringRightClickInteractionElements { get; set; }

		// Token: 0x06000397 RID: 919 RVA: 0x0000F75D File Offset: 0x0000D95D
		private void RightClicked()
		{
			if (!UiManager.HoveringRightClickInteractionElements)
			{
				this.TryGetInputHandler().OnRightClickCancel();
			}
		}

		// Token: 0x0400018C RID: 396
		private static UiManager _instance;

		// Token: 0x0400018D RID: 397
		[SerializeField]
		private RectTransform bottomLayer;

		// Token: 0x0400018E RID: 398
		[SerializeField]
		private RectTransform baseLayer;

		// Token: 0x0400018F RID: 399
		[SerializeField]
		private RectTransform normalLayer;

		// Token: 0x04000190 RID: 400
		[SerializeField]
		private RectTransform visualNovelLayer;

		// Token: 0x04000191 RID: 401
		[SerializeField]
		private RectTransform topLayer;

		// Token: 0x04000192 RID: 402
		[SerializeField]
		private RectTransform topmostLayer;

		// Token: 0x04000193 RID: 403
		[SerializeField]
		private RectTransform dialogLayer;

		// Token: 0x04000194 RID: 404
		[SerializeField]
		private RectTransform tooltipLayer;

		// Token: 0x04000195 RID: 405
		[SerializeField]
		private RectTransform highlightLayer;

		// Token: 0x04000196 RID: 406
		[SerializeField]
		private LoadingScreen loadingScreen;

		// Token: 0x04000197 RID: 407
		[SerializeField]
		private GameObject inputBlocker;

		// Token: 0x04000198 RID: 408
		[SerializeField]
		private InputActionAsset inputActions;

		// Token: 0x04000199 RID: 409
		private Canvas _canvas;

		// Token: 0x0400019A RID: 410
		private readonly Dictionary<Type, UiPanelBase> _panelTable = new Dictionary<Type, UiPanelBase>();

		// Token: 0x0400019B RID: 411
		private readonly Dictionary<Type, UiDialogBase> _dialogTable = new Dictionary<Type, UiDialogBase>();

		// Token: 0x0400019C RID: 412
		private UiDialogBase _currentDialog;

		// Token: 0x0400019F RID: 415
		private InputActionMap _shortcutsMap;

		// Token: 0x040001A0 RID: 416
		private InputAction[] _selectNs;

		// Token: 0x040001A1 RID: 417
		private SortedDictionary<ManaColor, InputAction> _poolColor;

		// Token: 0x040001A2 RID: 418
		private InputAction _useUs;

		// Token: 0x040001A3 RID: 419
		private InputAction _endTurn;

		// Token: 0x040001A4 RID: 420
		private InputAction _toggleBaseDeck;

		// Token: 0x040001A5 RID: 421
		private InputAction _toggleDrawZone;

		// Token: 0x040001A6 RID: 422
		private InputAction _toggleDiscardZone;

		// Token: 0x040001A7 RID: 423
		private InputAction _toggleExileZone;

		// Token: 0x040001A8 RID: 424
		private InputAction _toggleMap;

		// Token: 0x040001A9 RID: 425
		private InputAction _toogleMinimize;

		// Token: 0x040001AA RID: 426
		private InputAction _skipDialogAction;

		// Token: 0x040001AB RID: 427
		private InputAction _showEnemyMoveOrder;

		// Token: 0x040001AD RID: 429
		private readonly Stack<IInputActionHandler> _actionHandlerStack = new Stack<IInputActionHandler>();

		// Token: 0x040001B0 RID: 432
		private bool _isHidingUI;
	}
}
