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
	public sealed class HighlightManager : MonoBehaviour
	{
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
		private void Update()
		{
			Vector2Int vector2Int = new Vector2Int(Screen.width, Screen.height);
			if (this._currentScreenSize != vector2Int)
			{
				this._currentScreenSize = vector2Int;
				this.ResetRenderTextures();
			}
		}
		public void ResetRenderTextures()
		{
			foreach (HighlightManager.OutlineLayer outlineLayer in this._outlineLayers)
			{
				outlineLayer.ResetRenderTexture();
			}
		}
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
		[SerializeField]
		private Shader outlineShader;
		[SerializeField]
		private HighlightManager.OutlineSet[] outlineSets;
		private static HighlightManager _instance;
		private readonly List<HighlightManager.OutlineLayer> _outlineLayers = new List<HighlightManager.OutlineLayer>();
		private readonly Dictionary<GameObject, LayerMask> _originLayerMaskTable = new Dictionary<GameObject, LayerMask>();
		private Vector2Int _currentScreenSize;
		private static readonly int OutlineParameter = Shader.PropertyToID("_OutlineParameter");
		private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
		private static readonly int MainTex = Shader.PropertyToID("_MainTex");
		private static readonly int MotionValue = Shader.PropertyToID("_Motion");
		[Serializable]
		public class OutlineSet
		{
			public string identifier;
			public string layerName;
			public Vector3 outlineParameter = new Vector3(2f, 2f, 0f);
			[ColorUsage(true, true)]
			public Color color = new Color(1f, 1f, 1f, 1f);
		}
		private sealed class OutlineLayer : IDisposable
		{
			public int LayerIndex { get; }
			private void EnsureNotDisposed()
			{
				if (this._isDisposed)
				{
					throw new ObjectDisposedException(this._name, "OutlineLayer is already disposed");
				}
			}
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
			public void SetColor(Color color)
			{
				this.EnsureNotDisposed();
				this._rawImage.material.SetColor(HighlightManager.OutlineColor, color);
			}
			public void SetOutlineWidth(Vector3 width)
			{
				this.EnsureNotDisposed();
				this._rawImage.material.SetVector(HighlightManager.OutlineParameter, width);
			}
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
			private void PlayFadeInMotion()
			{
				this.EnsureNotDisposed();
				DOTween.Sequence("FadeIn").Append(this._rawImage.material.DOFloat(1f, HighlightManager.MotionValue, 0.66f).From(0f, true, false).SetEase(Ease.OutCubic)).SetUpdate(true)
					.SetAutoKill(true)
					.SetLink(this._rawImage.gameObject);
			}
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
			private readonly string _name;
			private readonly Camera _camera;
			private RenderTexture _renderTexture;
			private readonly RawImage _rawImage;
			private bool _isDisposed;
		}
	}
}
