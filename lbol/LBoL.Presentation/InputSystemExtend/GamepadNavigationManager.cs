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
	public class GamepadNavigationManager : Singleton<GamepadNavigationManager>
	{
		private void OnEnable()
		{
			Singleton<InputDeviceManager>.Instance.OnInputDeviceChanged.AddListener(new UnityAction<InputDeviceType>(this.OnInputDeviceChanged));
		}
		public static void AsnycSelectGameObject(GameObject go)
		{
			Singleton<GamepadNavigationManager>.Instance.StartCoroutine(Singleton<GamepadNavigationManager>.Instance.SelectGameObjectCorutine(go));
		}
		private IEnumerator SelectGameObjectCorutine(GameObject go)
		{
			yield return new WaitForEndOfFrame();
			if (go != null)
			{
				EventSystem.current.SetSelectedGameObject(go);
			}
			yield break;
		}
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
		public void AddPanel(IInteractablePanel panel)
		{
			this.panelStack.Add(panel.GetPanelName());
			this.OnPanelUpdate.Invoke();
		}
		public void RemovePanel(IInteractablePanel panel)
		{
			this.panelStack.RemoveAll((string p) => p == panel.GetPanelName());
			this.OnPanelUpdate.Invoke();
		}
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
		public static void AddOrigin(GamepadNavigationOrigin origin)
		{
			Singleton<GamepadNavigationManager>.Instance.origins.Add(origin);
		}
		public static void RemoveOrigin(GamepadNavigationOrigin origin)
		{
			GamepadNavigationManager instance = Singleton<GamepadNavigationManager>.Instance;
			if (instance == null)
			{
				return;
			}
			instance.origins.Remove(origin);
		}
		public static void SetOverrideOrigin(GameObject go, string panelName)
		{
			if (Singleton<GamepadNavigationManager>.Instance.overrideOrigins.ContainsKey(panelName))
			{
				Singleton<GamepadNavigationManager>.Instance.overrideOrigins[panelName] = go;
				return;
			}
			Singleton<GamepadNavigationManager>.Instance.overrideOrigins.Add(panelName, go);
		}
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
		private void OnInputDeviceChanged(InputDeviceType inputDevice)
		{
			if (inputDevice == InputDeviceType.Gamepad)
			{
				GamepadNavigationManager.RefreshSelection();
			}
		}
		public const string INVALID_PANEL = "INVALID_PANEL";
		public UnityEvent OnPanelUpdate = new UnityEvent();
		private List<string> panelStack = new List<string>();
		private List<GamepadNavigationOrigin> origins = new List<GamepadNavigationOrigin>();
		private Dictionary<string, GameObject> overrideOrigins = new Dictionary<string, GameObject>();
	}
}
