using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Core;
using UnityEngine;
namespace LBoL.Presentation.I10N
{
	public class LocalizedGameObject : MonoBehaviour
	{
		private void OnEnable()
		{
			this.OnLocaleChanged();
			L10nManager.LocaleChanged += new Action(this.OnLocaleChanged);
		}
		private void OnDisable()
		{
			L10nManager.LocaleChanged -= new Action(this.OnLocaleChanged);
		}
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
		public GameObject GetCurrentGameObject()
		{
			GameObject gameObject;
			if (!this.gameObjectTable.TryGetValue(Localization.CurrentLocale, out gameObject))
			{
				return null;
			}
			return gameObject;
		}
		public TComponent GetCurrentComponent<TComponent>() where TComponent : Component
		{
			GameObject gameObject;
			if (!this.gameObjectTable.TryGetValue(Localization.CurrentLocale, out gameObject))
			{
				return default(TComponent);
			}
			return gameObject.GetComponent<TComponent>();
		}
		[SerializeField]
		private AssociationList<Locale, GameObject> gameObjectTable;
	}
}
