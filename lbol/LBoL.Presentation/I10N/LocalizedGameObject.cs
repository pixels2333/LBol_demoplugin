using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Core;
using UnityEngine;

namespace LBoL.Presentation.I10N
{
	// Token: 0x020000F4 RID: 244
	public class LocalizedGameObject : MonoBehaviour
	{
		// Token: 0x06000DE2 RID: 3554 RVA: 0x00042B1F File Offset: 0x00040D1F
		private void OnEnable()
		{
			this.OnLocaleChanged();
			L10nManager.LocaleChanged += new Action(this.OnLocaleChanged);
		}

		// Token: 0x06000DE3 RID: 3555 RVA: 0x00042B38 File Offset: 0x00040D38
		private void OnDisable()
		{
			L10nManager.LocaleChanged -= new Action(this.OnLocaleChanged);
		}

		// Token: 0x06000DE4 RID: 3556 RVA: 0x00042B4C File Offset: 0x00040D4C
		private void OnLocaleChanged()
		{
			foreach (KeyValuePair<Locale, GameObject> keyValuePair in this.gameObjectTable)
			{
				Locale locale;
				GameObject gameObject;
				keyValuePair.Deconstruct(ref locale, ref gameObject);
				gameObject.SetActive(false);
			}
			GameObject gameObject2;
			if (this.gameObjectTable.TryGetValue(Localization.CurrentLocale, out gameObject2) && gameObject2)
			{
				gameObject2.SetActive(true);
			}
		}

		// Token: 0x06000DE5 RID: 3557 RVA: 0x00042BC8 File Offset: 0x00040DC8
		public GameObject GetCurrentGameObject()
		{
			GameObject gameObject;
			if (!this.gameObjectTable.TryGetValue(Localization.CurrentLocale, out gameObject))
			{
				return null;
			}
			return gameObject;
		}

		// Token: 0x06000DE6 RID: 3558 RVA: 0x00042BEC File Offset: 0x00040DEC
		public TComponent GetCurrentComponent<TComponent>() where TComponent : Component
		{
			GameObject gameObject;
			if (!this.gameObjectTable.TryGetValue(Localization.CurrentLocale, out gameObject))
			{
				return default(TComponent);
			}
			return gameObject.GetComponent<TComponent>();
		}

		// Token: 0x04000A68 RID: 2664
		[SerializeField]
		private AssociationList<Locale, GameObject> gameObjectTable;
	}
}
