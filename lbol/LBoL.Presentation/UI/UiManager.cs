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
	public sealed class UiManager : MonoBehaviour
	{
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
		public static bool IsInitialized
		{
			get
			{
				return UiManager._instance;
			}
		}
		public InputAction DebugMenuAction { get; private set; }
		public InputAction DebugConsoleAction { get; private set; }
		public InputAction BattleLog { get; private set; }
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
		public static string GetHandShortcutDisplayString(int index)
		{
			InputAction inputAction = UiManager.Instance._selectNs.TryGetValue(index);
			if (inputAction == null)
			{
				return "Null";
			}
			return inputAction.GetBindingDisplayString((InputBinding.DisplayStringOptions)0, null);
		}
		public static string GetSkipString()
		{
			return UiManager.Instance._skipDialogAction.GetBindingDisplayString((InputBinding.DisplayStringOptions)0, null);
		}
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
		public event Action OnActionHandlerChanged;
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
		public static void EnterGameRun(GameRunController gameRun)
		{
			foreach (UiPanelBase uiPanelBase in UiManager.Instance._panelTable.Values)
			{
				uiPanelBase.EnterGameRun(gameRun);
			}
		}
		public static void LeaveGameRun()
		{
			foreach (UiPanelBase uiPanelBase in UiManager.Instance._panelTable.Values)
			{
				uiPanelBase.LeaveGameRun();
			}
		}
		public static void EnterBattle(BattleController battle)
		{
			foreach (UiPanelBase uiPanelBase in UiManager.Instance._panelTable.Values)
			{
				uiPanelBase.EnterBattle(battle);
			}
		}
		public static void LeaveBattle()
		{
			GunManager.ClearAll();
			foreach (UiPanelBase uiPanelBase in UiManager.Instance._panelTable.Values)
			{
				uiPanelBase.LeaveBattle();
			}
		}
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
		public static void UnloadPanel(Type type)
		{
			UiManager.Instance.InternalUnloadPanel(type);
		}
		public static void UnloadPanel<TPanel>() where TPanel : UiPanelBase
		{
			UiManager.Instance.InternalUnloadPanel(typeof(TPanel));
		}
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
		private UiPanelBase InternalGetPanel(Type panelType)
		{
			UiPanelBase uiPanelBase;
			if (!this._panelTable.TryGetValue(panelType, ref uiPanelBase))
			{
				throw new InvalidOperationException("Cannot get panel '" + panelType.Name + "', not loaded yet.");
			}
			return uiPanelBase;
		}
		public static TPanel GetPanel<TPanel>() where TPanel : UiPanelBase
		{
			return (TPanel)((object)UiManager.Instance.InternalGetPanel(typeof(TPanel)));
		}
		public static void Show<TPanel>() where TPanel : UiPanel
		{
			UiManager.GetPanel<TPanel>().Show();
		}
		public static void Hide<TPanel>(bool transition = true) where TPanel : UiPanelBase
		{
			UiManager.GetPanel<TPanel>().Hide(transition);
		}
		private UiDialogBase InternalGetDialog(Type dialogType)
		{
			UiDialogBase uiDialogBase;
			if (!this._dialogTable.TryGetValue(dialogType, ref uiDialogBase))
			{
				throw new InvalidOperationException("Cannot get dialog '" + dialogType.Name + "', not loaded yet.");
			}
			return uiDialogBase;
		}
		public static TDialog GetDialog<TDialog>() where TDialog : UiDialogBase
		{
			return (TDialog)((object)UiManager.Instance.InternalGetDialog(typeof(TDialog)));
		}
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
		internal static void HideDialogCheck(UiDialogBase dialog)
		{
			UiManager instance = UiManager.Instance;
			if (instance._currentDialog != dialog)
			{
				Debug.LogWarning(string.Format("Hiding dialog '{0}' while current dialog == '{1}'", dialog.GetType().Name, instance._currentDialog.GetType()));
			}
			instance._currentDialog = null;
		}
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
		public static bool IsShowingLoading { get; private set; }
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
		public static bool HoveringRightClickInteractionElements { get; set; }
		private void RightClicked()
		{
			if (!UiManager.HoveringRightClickInteractionElements)
			{
				this.TryGetInputHandler().OnRightClickCancel();
			}
		}
		private static UiManager _instance;
		[SerializeField]
		private RectTransform bottomLayer;
		[SerializeField]
		private RectTransform baseLayer;
		[SerializeField]
		private RectTransform normalLayer;
		[SerializeField]
		private RectTransform visualNovelLayer;
		[SerializeField]
		private RectTransform topLayer;
		[SerializeField]
		private RectTransform topmostLayer;
		[SerializeField]
		private RectTransform dialogLayer;
		[SerializeField]
		private RectTransform tooltipLayer;
		[SerializeField]
		private RectTransform highlightLayer;
		[SerializeField]
		private LoadingScreen loadingScreen;
		[SerializeField]
		private GameObject inputBlocker;
		[SerializeField]
		private InputActionAsset inputActions;
		private Canvas _canvas;
		private readonly Dictionary<Type, UiPanelBase> _panelTable = new Dictionary<Type, UiPanelBase>();
		private readonly Dictionary<Type, UiDialogBase> _dialogTable = new Dictionary<Type, UiDialogBase>();
		private UiDialogBase _currentDialog;
		private InputActionMap _shortcutsMap;
		private InputAction[] _selectNs;
		private SortedDictionary<ManaColor, InputAction> _poolColor;
		private InputAction _useUs;
		private InputAction _endTurn;
		private InputAction _toggleBaseDeck;
		private InputAction _toggleDrawZone;
		private InputAction _toggleDiscardZone;
		private InputAction _toggleExileZone;
		private InputAction _toggleMap;
		private InputAction _toogleMinimize;
		private InputAction _skipDialogAction;
		private InputAction _showEnemyMoveOrder;
		private readonly Stack<IInputActionHandler> _actionHandlerStack = new Stack<IInputActionHandler>();
		private bool _isHidingUI;
	}
}
