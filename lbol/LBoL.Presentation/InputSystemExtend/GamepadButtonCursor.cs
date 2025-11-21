using System;
using LBoL.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LBoL.Presentation.InputSystemExtend
{
	// Token: 0x020000E4 RID: 228
	[RequireComponent(typeof(Selectable))]
	public class GamepadButtonCursor : GamepadBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler, ISubmitHandler
	{
		// Token: 0x17000244 RID: 580
		// (get) Token: 0x06000D7D RID: 3453 RVA: 0x00041544 File Offset: 0x0003F744
		// (set) Token: 0x06000D7E RID: 3454 RVA: 0x0004154C File Offset: 0x0003F74C
		public bool IsSelected { get; private set; }

		// Token: 0x17000245 RID: 581
		// (get) Token: 0x06000D7F RID: 3455 RVA: 0x00041555 File Offset: 0x0003F755
		public UnityEvent<bool> OnSelectChanged
		{
			get
			{
				return this.onSelectChanged;
			}
		}

		// Token: 0x17000246 RID: 582
		// (get) Token: 0x06000D80 RID: 3456 RVA: 0x0004155D File Offset: 0x0003F75D
		public UnityEvent OnClick
		{
			get
			{
				return this.onClick;
			}
		}

		// Token: 0x06000D81 RID: 3457 RVA: 0x00041565 File Offset: 0x0003F765
		protected void Awake()
		{
			this.OnDeselect(null);
		}

		// Token: 0x06000D82 RID: 3458 RVA: 0x0004156E File Offset: 0x0003F76E
		protected override void OnDisable()
		{
			base.OnDisable();
			this.OnDeselect(null);
		}

		// Token: 0x06000D83 RID: 3459 RVA: 0x0004157D File Offset: 0x0003F77D
		public virtual void OnDeselect(BaseEventData eventData)
		{
			this.IsSelected = false;
			if (this.cursor != null)
			{
				this.cursor.SetActive(false);
			}
			UnityEvent<bool> unityEvent = this.onSelectChanged;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke(false);
		}

		// Token: 0x06000D84 RID: 3460 RVA: 0x000415B4 File Offset: 0x0003F7B4
		public virtual void OnSelect(BaseEventData eventData)
		{
			if (Singleton<InputDeviceManager>.Instance.CurrentInputDevice == InputDeviceType.Gamepad)
			{
				this.IsSelected = true;
				if (this.cursor != null)
				{
					this.cursor.SetActive(true);
				}
				UnityEvent<bool> unityEvent = this.onSelectChanged;
				if (unityEvent == null)
				{
					return;
				}
				unityEvent.Invoke(true);
			}
		}

		// Token: 0x06000D85 RID: 3461 RVA: 0x00041600 File Offset: 0x0003F800
		public virtual void OnSubmit(BaseEventData eventData)
		{
			if (Singleton<InputDeviceManager>.Instance.CurrentInputDevice == InputDeviceType.Gamepad)
			{
				UnityEvent unityEvent = this.onClick;
				if (unityEvent == null)
				{
					return;
				}
				unityEvent.Invoke();
			}
		}

		// Token: 0x06000D86 RID: 3462 RVA: 0x0004161F File Offset: 0x0003F81F
		protected override void OnInputDeviceChanged(InputDeviceType inputDevice)
		{
			if (inputDevice != InputDeviceType.Gamepad)
			{
				this.OnDeselect(null);
			}
		}

		// Token: 0x04000A2B RID: 2603
		[SerializeField]
		private GameObject cursor;

		// Token: 0x04000A2C RID: 2604
		[SerializeField]
		private UnityEvent<bool> onSelectChanged;

		// Token: 0x04000A2D RID: 2605
		[SerializeField]
		protected UnityEvent onClick;
	}
}
