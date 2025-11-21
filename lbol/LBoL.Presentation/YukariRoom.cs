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
	// Token: 0x02000013 RID: 19
	public class YukariRoom : MonoBehaviour
	{
		// Token: 0x0600018E RID: 398 RVA: 0x00008018 File Offset: 0x00006218
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

		// Token: 0x0600018F RID: 399 RVA: 0x00008284 File Offset: 0x00006484
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

		// Token: 0x06000190 RID: 400 RVA: 0x000082E0 File Offset: 0x000064E0
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

		// Token: 0x06000191 RID: 401 RVA: 0x00008344 File Offset: 0x00006544
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

		// Token: 0x06000192 RID: 402 RVA: 0x00008399 File Offset: 0x00006599
		private void OnDestroy()
		{
			if (this._winkCts != null)
			{
				this._winkCts.Cancel();
				this._winkCts.Dispose();
			}
		}

		// Token: 0x0400006E RID: 110
		[Header("组件引用")]
		[SerializeField]
		private Transform outDoorGroup;

		// Token: 0x0400006F RID: 111
		[SerializeField]
		private Transform inDoorGroup;

		// Token: 0x04000070 RID: 112
		[SerializeField]
		private Transform hand;

		// Token: 0x04000071 RID: 113
		[SerializeField]
		private SpriteRenderer eye;

		// Token: 0x04000072 RID: 114
		[Header("资源引用")]
		[SerializeField]
		private List<Sprite> eyeList;

		// Token: 0x04000073 RID: 115
		[Header("参数")]
		[SerializeField]
		private Vector3 defaultOutDoorGroupPos = new Vector3(-10.22f, 5.64f, 0f);

		// Token: 0x04000074 RID: 116
		[SerializeField]
		private float transitionDuration = 1f;

		// Token: 0x04000075 RID: 117
		[Header("表现")]
		[FormerlySerializedAs("OutdoorBuildings")]
		public YukariRoom.OutdoorBuilding[] outdoorBuildings;

		// Token: 0x04000076 RID: 118
		private Camera _overlayCam;

		// Token: 0x04000077 RID: 119
		private Coroutine _winkCoroutine;

		// Token: 0x04000078 RID: 120
		private bool _hasEntered;

		// Token: 0x04000079 RID: 121
		private float _floatingTimer;

		// Token: 0x0400007A RID: 122
		private float _floatingTransition;

		// Token: 0x0400007B RID: 123
		private float[] _floatingOffsets;

		// Token: 0x0400007C RID: 124
		private CancellationTokenSource _winkCts;

		// Token: 0x0200016C RID: 364
		[Serializable]
		public class OutdoorBuilding
		{
			// Token: 0x06001162 RID: 4450 RVA: 0x000528E8 File Offset: 0x00050AE8
			public void UpdateTransform(float t, float transition)
			{
				this.transform.rotation = Quaternion.Euler(0f, 0f, this.Sine(t + this.rotationOffset, this.period, this.maxZ) * transition);
				this.transform.position = this.originPos + new Vector3(this.Sine(t, this.period, this.movementRange.x), this.Cosine(t + this.xyOffset, this.period, this.movementRange.y), 0f) * transition;
			}

			// Token: 0x06001163 RID: 4451 RVA: 0x00052988 File Offset: 0x00050B88
			private float Sine(float timer, float period, float length)
			{
				return Mathf.Sin(timer % period / period * 2f * 3.1415927f) * length;
			}

			// Token: 0x06001164 RID: 4452 RVA: 0x000529A2 File Offset: 0x00050BA2
			private float Cosine(float timer, float period, float length)
			{
				return Mathf.Cos(timer % period / period * 2f * 3.1415927f) * length;
			}

			// Token: 0x06001165 RID: 4453 RVA: 0x000529BC File Offset: 0x00050BBC
			public void RegisterPosition()
			{
				this.originPos = this.transform.position;
			}

			// Token: 0x04000D46 RID: 3398
			public Transform transform;

			// Token: 0x04000D47 RID: 3399
			public float period;

			// Token: 0x04000D48 RID: 3400
			public float maxZ;

			// Token: 0x04000D49 RID: 3401
			public Vector3 movementRange;

			// Token: 0x04000D4A RID: 3402
			public float rotationOffset;

			// Token: 0x04000D4B RID: 3403
			public float xyOffset;

			// Token: 0x04000D4C RID: 3404
			private Vector3 originPos;
		}
	}
}
