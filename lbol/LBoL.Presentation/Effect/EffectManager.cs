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
	// Token: 0x020000FD RID: 253
	public class EffectManager : Singleton<EffectManager>
	{
		// Token: 0x1700025F RID: 607
		// (get) Token: 0x06000E2F RID: 3631 RVA: 0x00043762 File Offset: 0x00041962
		public static bool IsLowPerformance { get; }

		// Token: 0x06000E30 RID: 3632 RVA: 0x0004376C File Offset: 0x0004196C
		private static EffectManager GetInstance()
		{
			EffectManager instance = Singleton<EffectManager>.Instance;
			if (instance._initialized)
			{
				return instance;
			}
			throw new InvalidOperationException("EffectManager is not initialized, call 'InitializeAsync()' is required");
		}

		// Token: 0x06000E31 RID: 3633 RVA: 0x00043793 File Offset: 0x00041993
		public static UniTask InitializeAsync()
		{
			return Singleton<EffectManager>.Instance.InternalInitializeAsync();
		}

		// Token: 0x06000E32 RID: 3634 RVA: 0x000437A0 File Offset: 0x000419A0
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

		// Token: 0x06000E33 RID: 3635 RVA: 0x000437E3 File Offset: 0x000419E3
		public static EffectWidget CreateEffect(string effectName, Transform trans, float delay = 0f, float? life = null, bool local = false, bool startActive = true)
		{
			return EffectManager.GetInstance().InternalCreateEffect(effectName, trans, delay, life, local, startActive);
		}

		// Token: 0x06000E34 RID: 3636 RVA: 0x000437F8 File Offset: 0x000419F8
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

		// Token: 0x06000E35 RID: 3637 RVA: 0x00043914 File Offset: 0x00041B14
		public static EffectWidget CreateEffect(string effectName, Transform trans, bool local)
		{
			return EffectManager.CreateEffect(effectName, trans, 0f, default(float?), local, true);
		}

		// Token: 0x06000E36 RID: 3638 RVA: 0x00043938 File Offset: 0x00041B38
		private static IEnumerator ActiveDelay(GameObject clone, float delay)
		{
			yield return new WaitForSeconds(delay);
			clone.SetActive(true);
			yield break;
		}

		// Token: 0x06000E37 RID: 3639 RVA: 0x00043950 File Offset: 0x00041B50
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

		// Token: 0x06000E38 RID: 3640 RVA: 0x00043A69 File Offset: 0x00041C69
		public static void LookForwardWorldPosition(EffectWidget effect, Transform target)
		{
			Transform transform = effect.transform;
			transform.LookAt(target);
			transform.Rotate(0f, 90f, 0f);
		}

		// Token: 0x06000E39 RID: 3641 RVA: 0x00043A8C File Offset: 0x00041C8C
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

		// Token: 0x06000E3A RID: 3642 RVA: 0x00043B10 File Offset: 0x00041D10
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

		// Token: 0x06000E3B RID: 3643 RVA: 0x00043BC4 File Offset: 0x00041DC4
		public void Remove(EffectBullet eb)
		{
			this._dyingEbs.Add(eb);
		}

		// Token: 0x04000A95 RID: 2709
		private bool _initialized;

		// Token: 0x04000A96 RID: 2710
		private EffectBulletView _effectBulletViewTemplate;

		// Token: 0x04000A97 RID: 2711
		private EffectUIBulletView _effectUIBulletViewTemplate;

		// Token: 0x04000A98 RID: 2712
		private readonly List<EffectBullet> _activeEffectBullets = new List<EffectBullet>();

		// Token: 0x04000A99 RID: 2713
		private readonly List<EffectBullet> _dyingEbs = new List<EffectBullet>();

		// Token: 0x04000A9A RID: 2714
		private readonly Dictionary<string, EffectWidget> _effectDictionary = new Dictionary<string, EffectWidget>();
	}
}
