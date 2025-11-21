using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Presentation.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace LBoL.Presentation
{
	// Token: 0x0200000B RID: 11
	public sealed class HighlightManager : MonoBehaviour
	{
		// Token: 0x06000150 RID: 336 RVA: 0x0000720F File Offset: 0x0000540F
		public void Awake()
		{
			if (HighlightManager._instance != null)
			{
				Debug.LogError("Multiple HighlightManager exists");
				Object.Destroy(base.gameObject);
				return;
			}
			HighlightManager._instance = this;
		}

		// Token: 0x06000151 RID: 337 RVA: 0x0000723C File Offset: 0x0000543C
		private void OnValidate()
		{
			foreach (HighlightManager.OutlineSet outlineSet in this.outlineSets)
			{
				if (LayerMask.NameToLayer(outlineSet.layerName) < 0)
				{
					Debug.LogError("[HighlightManager] Invalid layer name " + outlineSet.layerName);
				}
			}
		}

		// Token: 0x06000152 RID: 338 RVA: 0x00007288 File Offset: 0x00005488
		private void Start()
		{
			foreach (HighlightManager.OutlineLayer outlineLayer in this._outlineLayers)
			{
				outlineLayer.Dispose();
			}
			this._outlineLayers.Clear();
			this._currentScreenSize = new Vector2Int(Screen.width, Screen.height);
			foreach (HighlightManager.OutlineSet outlineSet in this.outlineSets)
			{
				int num = LayerMask.NameToLayer(outlineSet.layerName);
				if (num < 0)
				{
					Debug.LogError("[HighlightManager] Invalid layer name " + outlineSet.layerName);
				}
				else
				{
					HighlightManager.OutlineLayer outlineLayer2 = new HighlightManager.OutlineLayer(outlineSet.identifier, num, this.outlineShader);
					outlineLayer2.SetColor(outlineSet.color);
					outlineLayer2.SetOutlineWidth(outlineSet.outlineParameter);
					outlineLayer2.SetActive(false);
					this._outlineLayers.Add(outlineLayer2);
				}
			}
		}

		// Token: 0x06000153 RID: 339 RVA: 0x00007380 File Offset: 0x00005580
		private void Update()
		{
			Vector2Int vector2Int = new Vector2Int(Screen.width, Screen.height);
			if (this._currentScreenSize != vector2Int)
			{
				this._currentScreenSize = vector2Int;
				this.ResetRenderTextures();
			}
		}

		// Token: 0x06000154 RID: 340 RVA: 0x000073BC File Offset: 0x000055BC
		public void ResetRenderTextures()
		{
			foreach (HighlightManager.OutlineLayer outlineLayer in this._outlineLayers)
			{
				outlineLayer.ResetRenderTexture();
			}
		}

		// Token: 0x06000155 RID: 341 RVA: 0x0000740C File Offset: 0x0000560C
		public static void SetOutlineEnabled(GameObject go, int outlineType, bool enabled)
		{
			HighlightManager.OutlineLayer outlineLayer = HighlightManager._instance._outlineLayers[outlineType];
			outlineLayer.SetActive(enabled);
			LayerMask layerMask;
			if (enabled)
			{
				if (HighlightManager._instance._originLayerMaskTable.TryAdd(go, go.layer))
				{
					Transform[] array = go.transform.GetComponentsInChildren<Transform>(true);
					for (int i = 0; i < array.Length; i++)
					{
						array[i].gameObject.layer = outlineLayer.LayerIndex;
					}
					return;
				}
			}
			else if (HighlightManager._instance._originLayerMaskTable.Remove(go, ref layerMask))
			{
				Transform[] array = go.transform.GetComponentsInChildren<Transform>(true);
				for (int i = 0; i < array.Length; i++)
				{
					array[i].gameObject.layer = layerMask;
				}
			}
		}

		// Token: 0x04000051 RID: 81
		[SerializeField]
		private Shader outlineShader;

		// Token: 0x04000052 RID: 82
		[SerializeField]
		private HighlightManager.OutlineSet[] outlineSets;

		// Token: 0x04000053 RID: 83
		private static HighlightManager _instance;

		// Token: 0x04000054 RID: 84
		private readonly List<HighlightManager.OutlineLayer> _outlineLayers = new List<HighlightManager.OutlineLayer>();

		// Token: 0x04000055 RID: 85
		private readonly Dictionary<GameObject, LayerMask> _originLayerMaskTable = new Dictionary<GameObject, LayerMask>();

		// Token: 0x04000056 RID: 86
		private Vector2Int _currentScreenSize;

		// Token: 0x04000057 RID: 87
		private static readonly int OutlineParameter = Shader.PropertyToID("_OutlineParameter");

		// Token: 0x04000058 RID: 88
		private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");

		// Token: 0x04000059 RID: 89
		private static readonly int MainTex = Shader.PropertyToID("_MainTex");

		// Token: 0x0400005A RID: 90
		private static readonly int MotionValue = Shader.PropertyToID("_Motion");

		// Token: 0x0200015B RID: 347
		[Serializable]
		public class OutlineSet
		{
			// Token: 0x04000CFA RID: 3322
			public string identifier;

			// Token: 0x04000CFB RID: 3323
			public string layerName;

			// Token: 0x04000CFC RID: 3324
			public Vector3 outlineParameter = new Vector3(2f, 2f, 0f);

			// Token: 0x04000CFD RID: 3325
			[ColorUsage(true, true)]
			public Color color = new Color(1f, 1f, 1f, 1f);
		}

		// Token: 0x0200015C RID: 348
		private sealed class OutlineLayer : IDisposable
		{
			// Token: 0x17000368 RID: 872
			// (get) Token: 0x0600113A RID: 4410 RVA: 0x0005151C File Offset: 0x0004F71C
			public int LayerIndex { get; }

			// Token: 0x0600113B RID: 4411 RVA: 0x00051524 File Offset: 0x0004F724
			private void EnsureNotDisposed()
			{
				if (this._isDisposed)
				{
					throw new ObjectDisposedException(this._name, "OutlineLayer is already disposed");
				}
			}

			// Token: 0x0600113C RID: 4412 RVA: 0x00051540 File Offset: 0x0004F740
			public OutlineLayer(string id, int layerIndex, Shader shader)
			{
				this._name = "OutlineLayer " + id;
				this.LayerIndex = layerIndex;
				this._renderTexture = new RenderTexture(new RenderTextureDescriptor(Screen.width / 2, Screen.height / 2, RenderTextureFormat.ARGB32, 0))
				{
					filterMode = FilterMode.Bilinear
				};
				this._camera = CameraController.DuplicateSceneCamera("Highlight Camera: " + this._name);
				this._camera.cullingMask = 1 << layerIndex;
				this._camera.targetTexture = this._renderTexture;
				this._rawImage = UiManager.CreateHighlightRawImage("Highlight RawImage: " + this._name);
				Material material = CoreUtils.CreateEngineMaterial(shader);
				material.SetTexture(HighlightManager.MainTex, this._renderTexture);
				this._rawImage.material = material;
			}

			// Token: 0x0600113D RID: 4413 RVA: 0x00051610 File Offset: 0x0004F810
			public void ResetRenderTexture()
			{
				this.EnsureNotDisposed();
				if (this._renderTexture)
				{
					Object.Destroy(this._renderTexture);
				}
				this._renderTexture = new RenderTexture(new RenderTextureDescriptor(Screen.width / 2, Screen.height / 2, RenderTextureFormat.ARGB32, 0))
				{
					filterMode = FilterMode.Bilinear
				};
				this._camera.targetTexture = this._renderTexture;
				this._rawImage.material.SetTexture(HighlightManager.MainTex, this._renderTexture);
			}

			// Token: 0x0600113E RID: 4414 RVA: 0x0005168E File Offset: 0x0004F88E
			public void SetColor(Color color)
			{
				this.EnsureNotDisposed();
				this._rawImage.material.SetColor(HighlightManager.OutlineColor, color);
			}

			// Token: 0x0600113F RID: 4415 RVA: 0x000516AC File Offset: 0x0004F8AC
			public void SetOutlineWidth(Vector3 width)
			{
				this.EnsureNotDisposed();
				this._rawImage.material.SetVector(HighlightManager.OutlineParameter, width);
			}

			// Token: 0x06001140 RID: 4416 RVA: 0x000516CF File Offset: 0x0004F8CF
			public void SetActive(bool active)
			{
				this.EnsureNotDisposed();
				this._camera.gameObject.SetActive(active);
				this._rawImage.gameObject.SetActive(active);
				if (active)
				{
					this.PlayFadeInMotion();
				}
			}

			// Token: 0x06001141 RID: 4417 RVA: 0x00051704 File Offset: 0x0004F904
			private void PlayFadeInMotion()
			{
				this.EnsureNotDisposed();
				DOTween.Sequence("FadeIn").Append(this._rawImage.material.DOFloat(1f, HighlightManager.MotionValue, 0.66f).From(0f, true, false).SetEase(Ease.OutCubic)).SetUpdate(true)
					.SetAutoKill(true)
					.SetLink(this._rawImage.gameObject);
			}

			// Token: 0x06001142 RID: 4418 RVA: 0x00051778 File Offset: 0x0004F978
			public void Dispose()
			{
				if (this._isDisposed)
				{
					return;
				}
				this._isDisposed = true;
				if (this._camera)
				{
					Object.Destroy(this._camera.gameObject);
				}
				if (this._renderTexture)
				{
					Object.Destroy(this._renderTexture);
				}
				if (this._rawImage)
				{
					Object.Destroy(this._rawImage.gameObject);
				}
			}

			// Token: 0x04000CFE RID: 3326
			private readonly string _name;

			// Token: 0x04000CFF RID: 3327
			private readonly Camera _camera;

			// Token: 0x04000D00 RID: 3328
			private RenderTexture _renderTexture;

			// Token: 0x04000D01 RID: 3329
			private readonly RawImage _rawImage;

			// Token: 0x04000D02 RID: 3330
			private bool _isDisposed;
		}
	}
}
