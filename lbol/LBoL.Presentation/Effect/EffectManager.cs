using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using LBoL.ConfigData;
using LBoL.Core;
using UnityEngine;
namespace LBoL.Presentation.Effect
{
	public class EffectManager : Singleton<EffectManager>
	{
		public static bool IsLowPerformance { get; }
		private static EffectManager GetInstance()
		{
			EffectManager instance = Singleton<EffectManager>.Instance;
			if (instance._initialized)
			{
				return instance;
			}
			throw new InvalidOperationException("EffectManager is not initialized, call 'InitializeAsync()' is required");
		}
		public static UniTask InitializeAsync()
		{
			return Singleton<EffectManager>.Instance.InternalInitializeAsync();
		}
		private async UniTask InternalInitializeAsync()
		{
			if (!this._initialized)
			{
				Object @object = await Resources.LoadAsync<EffectBulletView>("Template/EffectBulletTemplate");
				this._effectBulletViewTemplate = (EffectBulletView)@object;
				if (!this._effectBulletViewTemplate)
				{
					throw new NullReferenceException("EffectBullet Template Lost!");
				}
				@object = await Resources.LoadAsync<EffectUIBulletView>("Template/EffectUIBulletTemplate");
				this._effectUIBulletViewTemplate = (EffectUIBulletView)@object;
				if (!this._effectUIBulletViewTemplate)
				{
					throw new NullReferenceException("EffectBullet Template Lost!");
				}
				foreach (EffectConfig effectConfig in EffectConfig.AllConfig())
				{
					string path = effectConfig.Path;
					if (!this._effectDictionary.ContainsKey(path))
					{
						try
						{
							this._effectDictionary.Add(path, ResourcesHelper.LoadEffect(path));
						}
						catch (Exception ex)
						{
							Debug.LogError("Cannot load effect " + path + ": " + ex.Message);
						}
					}
				}
				this._initialized = true;
			}
		}
		public static EffectWidget CreateEffect(string effectName, Transform trans, float delay = 0f, float? life = null, bool local = false, bool startActive = true)
		{
			return EffectManager.GetInstance().InternalCreateEffect(effectName, trans, delay, life, local, startActive);
		}
		private EffectWidget InternalCreateEffect(string effectName, Transform trans, float delay, float? life, bool local, bool startActive)
		{
			if (string.IsNullOrEmpty(effectName) || effectName == "Empty")
			{
				return null;
			}
			EffectConfig effectConfig = EffectConfig.FromName(effectName);
			if (effectConfig == null)
			{
				Debug.LogError("Effect '" + effectName + "' not found");
				return null;
			}
			string path = effectConfig.Path;
			EffectWidget effectWidget;
			if (!this._effectDictionary.TryGetValue(path, ref effectWidget))
			{
				Debug.LogError("Effect at '" + path + "' is not loaded");
				return null;
			}
			EffectWidget effectWidget2 = (local ? Object.Instantiate<EffectWidget>(effectWidget, trans) : Object.Instantiate<EffectWidget>(effectWidget, trans.position, trans.rotation, Singleton<EffectManager>.Instance.transform));
			GameObject gameObject = effectWidget2.gameObject;
			float num = 0f;
			if (startActive)
			{
				if (delay > 0f)
				{
					gameObject.SetActive(false);
					num = delay;
					base.StartCoroutine(EffectManager.ActiveDelay(gameObject, num));
				}
				else
				{
					gameObject.SetActive(true);
				}
			}
			float num2 = life ?? effectConfig.Life;
			if (num2 > 0f)
			{
				num2 += num;
				Object.Destroy(effectWidget2.gameObject, num2);
			}
			return effectWidget2;
		}
		public static EffectWidget CreateEffect(string effectName, Transform trans, bool local)
		{
			return EffectManager.CreateEffect(effectName, trans, 0f, default(float?), local, true);
		}
		private static IEnumerator ActiveDelay(GameObject clone, float delay)
		{
			yield return new WaitForSeconds(delay);
			clone.SetActive(true);
			yield break;
		}
		public static void ModifyEffect(EffectWidget effect, float scale = 1f, int color = 0)
		{
			if (effect == null || !effect)
			{
				return;
			}
			effect.transform.localScale *= scale;
			if (color == 0)
			{
				return;
			}
			bool flag = false;
			float num = 0f;
			switch (color)
			{
			case 1:
				num = 0f;
				break;
			case 2:
				num = 0.083333336f;
				break;
			case 3:
				num = 0.16666667f;
				break;
			case 4:
				num = 0.25f;
				break;
			case 5:
				num = 0.33333334f;
				break;
			case 6:
				num = 0.41666666f;
				break;
			case 7:
				num = 0.5f;
				break;
			case 8:
				num = 0.5833333f;
				break;
			case 9:
				num = 0.6666667f;
				break;
			case 10:
				num = 0.75f;
				break;
			case 11:
				num = 0.8333333f;
				break;
			case 12:
				num = 0.9166667f;
				break;
			case 13:
				flag = true;
				break;
			case 14:
				num = (float)Random.Range(0, 360) / 360f;
				break;
			case 15:
				num = (float)Random.Range(0, 12) / 12f;
				break;
			}
			effect.Modify(num, flag);
		}
		public static void LookForwardWorldPosition(EffectWidget effect, Transform target)
		{
			Transform transform = effect.transform;
			transform.LookAt(target);
			transform.Rotate(0f, 90f, 0f);
		}
		public static EffectBulletView CreateEffectBullet(EffectBullet eb, Vector3 position = default(Vector3), [CanBeNull] Transform parent = null)
		{
			RectTransform rectTransform;
			EffectBulletView effectBulletView;
			if (parent != null && parent.TryGetComponent<RectTransform>(out rectTransform))
			{
				effectBulletView = Object.Instantiate<EffectUIBulletView>(Singleton<EffectManager>.Instance._effectUIBulletViewTemplate, parent);
			}
			else
			{
				effectBulletView = Object.Instantiate<EffectBulletView>(Singleton<EffectManager>.Instance._effectBulletViewTemplate, Singleton<EffectManager>.Instance.transform);
			}
			effectBulletView.SetEffectBullet(eb);
			eb.Position = position;
			if (parent != null)
			{
				effectBulletView.transform.SetParent(parent);
			}
			Singleton<EffectManager>.Instance._activeEffectBullets.Add(eb);
			return effectBulletView;
		}
		public void Tick()
		{
			if (this._dyingEbs.Count > 0)
			{
				foreach (EffectBullet effectBullet in this._dyingEbs)
				{
					this._activeEffectBullets.Remove(effectBullet);
				}
				this._dyingEbs.Clear();
			}
			foreach (EffectBullet effectBullet2 in this._activeEffectBullets)
			{
				effectBullet2.Tick();
			}
		}
		public void Remove(EffectBullet eb)
		{
			this._dyingEbs.Add(eb);
		}
		private bool _initialized;
		private EffectBulletView _effectBulletViewTemplate;
		private EffectUIBulletView _effectUIBulletViewTemplate;
		private readonly List<EffectBullet> _activeEffectBullets = new List<EffectBullet>();
		private readonly List<EffectBullet> _dyingEbs = new List<EffectBullet>();
		private readonly Dictionary<string, EffectWidget> _effectDictionary = new Dictionary<string, EffectWidget>();
	}
}
