using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace LBoL.Presentation.InputSystemExtend
{
	// Token: 0x020000EA RID: 234
	public class GamepadNavigationManager : Singleton<GamepadNavigationManager>
	{
		// Token: 0x06000D99 RID: 3481 RVA: 0x00041983 File Offset: 0x0003FB83
		private void OnEnable()
		{
			Singleton<InputDeviceManager>.Instance.OnInputDeviceChanged.AddListener(new UnityAction<InputDeviceType>(this.OnInputDeviceChanged));
		}

		// Token: 0x06000D9A RID: 3482 RVA: 0x000419A0 File Offset: 0x0003FBA0
		public static void AsnycSelectGameObject(GameObject go)
		{
			Singleton<GamepadNavigationManager>.Instance.StartCoroutine(Singleton<GamepadNavigationManager>.Instance.SelectGameObjectCorutine(go));
		}

		// Token: 0x06000D9B RID: 3483 RVA: 0x000419B8 File Offset: 0x0003FBB8
		private IEnumerator SelectGameObjectCorutine(GameObject go)
		{
			yield return new WaitForEndOfFrame();
			if (go != null)
			{
				EventSystem.current.SetSelectedGameObject(go);
			}
			yield break;
		}

		// Token: 0x06000D9C RID: 3484 RVA: 0x000419C8 File Offset: 0x0003FBC8
		public void SetAvailable(bool value)
		{
			if (value)
			{
				this.panelStack.RemoveAll((string panel) => panel == "INVALID_PANEL");
			}
			else
			{
				this.panelStack.Add("INVALID_PANEL");
			}
			this.OnPanelUpdate.Invoke();
		}

		// Token: 0x06000D9D RID: 3485 RVA: 0x00041A20 File Offset: 0x0003FC20
		public void AddPanel(IInteractablePanel panel)
		{
			this.panelStack.Add(panel.GetPanelName());
			this.OnPanelUpdate.Invoke();
		}

		// Token: 0x06000D9E RID: 3486 RVA: 0x00041A40 File Offset: 0x0003FC40
		public void RemovePanel(IInteractablePanel panel)
		{
			this.panelStack.RemoveAll((string p) => p == panel.GetPanelName());
			this.OnPanelUpdate.Invoke();
		}

		// Token: 0x06000D9F RID: 3487 RVA: 0x00041A80 File Offset: 0x0003FC80
		public string GetTopPanel()
		{
			if (this.panelStack.Count == 0)
			{
				return "INVALID_PANEL";
			}
			if (Enumerable.Count<string>(Enumerable.Where<string>(this.panelStack, (string panel) => panel == "MessageDialog")) > 0)
			{
				return "MessageDialog";
			}
			return this.panelStack[this.panelStack.Count - 1];
		}

		// Token: 0x06000DA0 RID: 3488 RVA: 0x00041AF0 File Offset: 0x0003FCF0
		public static void AddOrigin(GamepadNavigationOrigin origin)
		{
			Singleton<GamepadNavigationManager>.Instance.origins.Add(origin);
		}

		// Token: 0x06000DA1 RID: 3489 RVA: 0x00041B02 File Offset: 0x0003FD02
		public static void RemoveOrigin(GamepadNavigationOrigin origin)
		{
			GamepadNavigationManager instance = Singleton<GamepadNavigationManager>.Instance;
			if (instance == null)
			{
				return;
			}
			instance.origins.Remove(origin);
		}

		// Token: 0x06000DA2 RID: 3490 RVA: 0x00041B1A File Offset: 0x0003FD1A
		public static void SetOverrideOrigin(GameObject go, string panelName)
		{
			if (Singleton<GamepadNavigationManager>.Instance.overrideOrigins.ContainsKey(panelName))
			{
				Singleton<GamepadNavigationManager>.Instance.overrideOrigins[panelName] = go;
				return;
			}
			Singleton<GamepadNavigationManager>.Instance.overrideOrigins.Add(panelName, go);
		}

		// Token: 0x06000DA3 RID: 3491 RVA: 0x00041B54 File Offset: 0x0003FD54
		public static void RefreshSelection()
		{
			if (Singleton<GamepadNavigationManager>.Instance.origins.Count == 0 || Singleton<InputDeviceManager>.Instance.CurrentInputDevice != InputDeviceType.Gamepad)
			{
				return;
			}
			EventSystem.current.SetSelectedGameObject(null, null);
			string topPanel = Singleton<GamepadNavigationManager>.Instance.GetTopPanel();
			if (Singleton<GamepadNavigationManager>.Instance.overrideOrigins.ContainsKey(topPanel))
			{
				GameObject gameObject = Singleton<GamepadNavigationManager>.Instance.overrideOrigins[topPanel];
				Singleton<GamepadNavigationManager>.Instance.overrideOrigins.Remove(topPanel);
				if (gameObject != null)
				{
					GamepadNavigationManager.AsnycSelectGameObject(gameObject);
				}
				return;
			}
			GamepadNavigationManager.AsnycSelectGameObject(Enumerable.LastOrDefault<GamepadNavigationOrigin>(Singleton<GamepadNavigationManager>.Instance.origins).gameObject);
		}

		// Token: 0x06000DA4 RID: 3492 RVA: 0x00041BF4 File Offset: 0x0003FDF4
		private void OnInputDeviceChanged(InputDeviceType inputDevice)
		{
			if (inputDevice == InputDeviceType.Gamepad)
			{
				GamepadNavigationManager.RefreshSelection();
			}
		}

		// Token: 0x04000A40 RID: 2624
		public const string INVALID_PANEL = "INVALID_PANEL";

		// Token: 0x04000A41 RID: 2625
		public UnityEvent OnPanelUpdate = new UnityEvent();

		// Token: 0x04000A42 RID: 2626
		private List<string> panelStack = new List<string>();

		// Token: 0x04000A43 RID: 2627
		private List<GamepadNavigationOrigin> origins = new List<GamepadNavigationOrigin>();

		// Token: 0x04000A44 RID: 2628
		private Dictionary<string, GameObject> overrideOrigins = new Dictionary<string, GameObject>();
	}
}
