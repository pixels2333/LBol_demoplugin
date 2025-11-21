using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000059 RID: 89
	public class GameLogoWidget : MonoBehaviour
	{
		// Token: 0x170000D9 RID: 217
		// (get) Token: 0x06000506 RID: 1286 RVA: 0x00015731 File Offset: 0x00013931
		// (set) Token: 0x06000507 RID: 1287 RVA: 0x00015739 File Offset: 0x00013939
		private bool Updating { get; set; }

		// Token: 0x06000508 RID: 1288 RVA: 0x00015744 File Offset: 0x00013944
		private void Start()
		{
			for (int i = 0; i < 5; i++)
			{
				this._front[i] = GameLogoWidget.IsFront((float)(i * 72));
				LogoOrbWidget logoOrbWidget = Object.Instantiate<LogoOrbWidget>(this.orbTemplate, this._front[i] ? this.effectFront : this.effectBack);
				logoOrbWidget.name = "Orb" + i.ToString();
				logoOrbWidget.gameObject.SetActive(true);
				Transform transform = logoOrbWidget.transform;
				transform.localPosition = this.GetPosition((float)(i * 72));
				this.orbImageTransList.Add(transform);
				logoOrbWidget.orbImage.sprite = this.orbSprites[4 - i];
				logoOrbWidget.borderImage.sprite = this.orbBorderSprites[4 - i];
				Transform transform2 = logoOrbWidget.borderImage.transform;
				this.borderTransList.Add(transform2);
				Object.Instantiate<GameObject>(this.orbParticles[4 - i], logoOrbWidget.trailParent);
			}
		}

		// Token: 0x06000509 RID: 1289 RVA: 0x0001584C File Offset: 0x00013A4C
		private void OnEnable()
		{
			this.Updating = true;
			foreach (Transform transform in this.borderTransList)
			{
				transform.DOLocalRotate(new Vector3(0f, 0f, 360f), 2f, RotateMode.FastBeyond360).SetLoops(-1).SetEase(Ease.Linear)
					.SetUpdate(true);
			}
		}

		// Token: 0x0600050A RID: 1290 RVA: 0x000158D0 File Offset: 0x00013AD0
		private void OnDisable()
		{
			this.Updating = false;
			foreach (Transform transform in this.borderTransList)
			{
				transform.DOKill(false);
			}
		}

		// Token: 0x0600050B RID: 1291 RVA: 0x0001592C File Offset: 0x00013B2C
		private void Update()
		{
			if (this.Updating)
			{
				this._phase += this.speed * Time.unscaledDeltaTime;
				for (int i = 0; i < 5; i++)
				{
					this.orbImageTransList[i].localPosition = this.GetPosition(this._phase + (float)(i * 72));
					bool flag = GameLogoWidget.IsFront(this._phase + (float)(i * 72));
					if (flag != this._front[i])
					{
						this._front[i] = flag;
						this.orbImageTransList[i].SetParent(flag ? this.effectFront : this.effectBack);
					}
				}
			}
		}

		// Token: 0x0600050C RID: 1292 RVA: 0x000159DC File Offset: 0x00013BDC
		private Vector2 GetPosition(float phase)
		{
			float num = phase * 0.017453292f;
			Vector2 vector = new Vector2(Mathf.Cos(num) * this.size.x, Mathf.Sin(num) * this.size.y);
			vector = Quaternion.Euler(0f, 0f, this.tilt) * vector;
			vector += this.center;
			return vector;
		}

		// Token: 0x0600050D RID: 1293 RVA: 0x00015A50 File Offset: 0x00013C50
		private static bool IsFront(float phase)
		{
			return phase - (float)(Mathf.FloorToInt(phase / 360f) * 360) > 180f;
		}

		// Token: 0x040002C3 RID: 707
		[SerializeField]
		private Transform effectBack;

		// Token: 0x040002C4 RID: 708
		[SerializeField]
		private Transform effectFront;

		// Token: 0x040002C5 RID: 709
		[SerializeField]
		private Vector2 center;

		// Token: 0x040002C6 RID: 710
		[SerializeField]
		private Vector2 size;

		// Token: 0x040002C7 RID: 711
		[SerializeField]
		private float tilt;

		// Token: 0x040002C8 RID: 712
		[SerializeField]
		private float speed;

		// Token: 0x040002C9 RID: 713
		[SerializeField]
		private LogoOrbWidget orbTemplate;

		// Token: 0x040002CA RID: 714
		[SerializeField]
		private List<Sprite> orbSprites;

		// Token: 0x040002CB RID: 715
		[SerializeField]
		private List<Sprite> orbBorderSprites;

		// Token: 0x040002CC RID: 716
		[SerializeField]
		private List<GameObject> orbParticles;

		// Token: 0x040002CE RID: 718
		private float _phase;

		// Token: 0x040002CF RID: 719
		private readonly bool[] _front = new bool[5];

		// Token: 0x040002D0 RID: 720
		private readonly List<Transform> orbImageTransList = new List<Transform>();

		// Token: 0x040002D1 RID: 721
		private readonly List<Transform> borderTransList = new List<Transform>();
	}
}
