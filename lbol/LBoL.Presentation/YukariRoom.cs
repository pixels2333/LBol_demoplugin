using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Serialization;
namespace LBoL.Presentation
{
	public class YukariRoom : MonoBehaviour
	{
		public void Enter()
		{
			this._hasEntered = true;
			int num = CameraController.MainCamera.cullingMask;
			num &= ~LayerMask.GetMask(new string[] { "Background" });
			this._overlayCam = CameraController.SplitSceneCameraByLayer("Yukari Camera", num);
			this._floatingTimer = 0f;
			this._floatingOffsets = new float[this.outdoorBuildings.Length];
			for (int i = 0; i < this.outdoorBuildings.Length; i++)
			{
				this._floatingOffsets[i] = (float)Random.Range(-100, 100);
				this.outdoorBuildings[i].RegisterPosition();
			}
			this.inDoorGroup.DOScale(1f, this.transitionDuration).From(3f, true, false).SetAutoKill(true);
			this.outDoorGroup.DOScale(1f, this.transitionDuration).From(3f, true, false).SetAutoKill(true);
			this.outDoorGroup.DORotate(Vector3.zero, this.transitionDuration, RotateMode.Fast).From(new Vector3(0f, 0f, 45f), true, false).SetAutoKill(true);
			this.outDoorGroup.DOLocalMoveX(this.defaultOutDoorGroupPos.x, this.transitionDuration, false).From(-20f, true, false).SetRelative<TweenerCore<Vector3, Vector3, VectorOptions>>()
				.SetAutoKill(true);
			DOTween.To(() => 0f, delegate(float t)
			{
				this._floatingTransition = t;
			}, 1f, this.transitionDuration).SetAutoKill(true);
			SpriteRenderer[] array = this.inDoorGroup.GetComponentsInChildren<SpriteRenderer>();
			for (int j = 0; j < array.Length; j++)
			{
				array[j].DOColor(Color.white, this.transitionDuration).From(Color.black, true, false);
			}
			array = this.outDoorGroup.GetComponentsInChildren<SpriteRenderer>();
			for (int j = 0; j < array.Length; j++)
			{
				array[j].DOColor(Color.white, this.transitionDuration).From(Color.black, true, false);
			}
			this.hand.DORotate(Vector3.zero, this.transitionDuration, RotateMode.Fast).From(new Vector3(0f, 0f, -65f), true, false);
			this._winkCts = new CancellationTokenSource();
			this.WinkAsync(this.transitionDuration, 0.2f, this._winkCts.Token).Forget();
		}
		private async UniTask WinkAsync(float delay, float duration, CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				for (int i = 0; i < this.eyeList.Count; i++)
				{
					if (i == 0)
					{
						await UniTask.Delay(TimeSpan.FromSeconds((double)delay), false, PlayerLoopTiming.Update, token, false);
					}
					else
					{
						await UniTask.Delay(TimeSpan.FromSeconds((double)(duration / (float)(this.eyeList.Count - 1))), false, PlayerLoopTiming.Update, token, false);
					}
					this.eye.sprite = this.eyeList[i];
				}
				for (int i = this.eyeList.Count - 1; i >= 0; i--)
				{
					await UniTask.Delay(TimeSpan.FromSeconds((double)(duration / (float)(this.eyeList.Count - 1))), false, PlayerLoopTiming.Update, token, false);
					this.eye.sprite = this.eyeList[i];
				}
				delay = (float)Random.Range(4, 7);
			}
		}
		public void Leave()
		{
			this._hasEntered = false;
			this._floatingTransition = 0f;
			CameraController.DestroySceneSubCamera(this._overlayCam);
			this._overlayCam = null;
			if (this._winkCts != null)
			{
				this._winkCts.Cancel();
				this._winkCts = null;
			}
			this.eye.sprite = this.eyeList[0];
		}
		private void Update()
		{
			if (this._hasEntered)
			{
				this._floatingTimer += Time.deltaTime;
				for (int i = 0; i < 5; i++)
				{
					this.outdoorBuildings[i].UpdateTransform(this._floatingTimer + this._floatingOffsets[i], this._floatingTransition);
				}
			}
		}
		private void OnDestroy()
		{
			if (this._winkCts != null)
			{
				this._winkCts.Cancel();
				this._winkCts.Dispose();
			}
		}
		[Header("组件引用")]
		[SerializeField]
		private Transform outDoorGroup;
		[SerializeField]
		private Transform inDoorGroup;
		[SerializeField]
		private Transform hand;
		[SerializeField]
		private SpriteRenderer eye;
		[Header("资源引用")]
		[SerializeField]
		private List<Sprite> eyeList;
		[Header("参数")]
		[SerializeField]
		private Vector3 defaultOutDoorGroupPos = new Vector3(-10.22f, 5.64f, 0f);
		[SerializeField]
		private float transitionDuration = 1f;
		[Header("表现")]
		[FormerlySerializedAs("OutdoorBuildings")]
		public YukariRoom.OutdoorBuilding[] outdoorBuildings;
		private Camera _overlayCam;
		private Coroutine _winkCoroutine;
		private bool _hasEntered;
		private float _floatingTimer;
		private float _floatingTransition;
		private float[] _floatingOffsets;
		private CancellationTokenSource _winkCts;
		[Serializable]
		public class OutdoorBuilding
		{
			public void UpdateTransform(float t, float transition)
			{
				this.transform.rotation = Quaternion.Euler(0f, 0f, this.Sine(t + this.rotationOffset, this.period, this.maxZ) * transition);
				this.transform.position = this.originPos + new Vector3(this.Sine(t, this.period, this.movementRange.x), this.Cosine(t + this.xyOffset, this.period, this.movementRange.y), 0f) * transition;
			}
			private float Sine(float timer, float period, float length)
			{
				return Mathf.Sin(timer % period / period * 2f * 3.1415927f) * length;
			}
			private float Cosine(float timer, float period, float length)
			{
				return Mathf.Cos(timer % period / period * 2f * 3.1415927f) * length;
			}
			public void RegisterPosition()
			{
				this.originPos = this.transform.position;
			}
			public Transform transform;
			public float period;
			public float maxZ;
			public Vector3 movementRange;
			public float rotationOffset;
			public float xyOffset;
			private Vector3 originPos;
		}
	}
}
