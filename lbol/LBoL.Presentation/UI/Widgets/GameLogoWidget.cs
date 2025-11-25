using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
namespace LBoL.Presentation.UI.Widgets
{
	public class GameLogoWidget : MonoBehaviour
	{
		private bool Updating { get; set; }
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
		private void OnEnable()
		{
			this.Updating = true;
			foreach (Transform transform in this.borderTransList)
			{
				transform.DOLocalRotate(new Vector3(0f, 0f, 360f), 2f, RotateMode.FastBeyond360).SetLoops(-1).SetEase(Ease.Linear)
					.SetUpdate(true);
			}
		}
		private void OnDisable()
		{
			this.Updating = false;
			foreach (Transform transform in this.borderTransList)
			{
				transform.DOKill(false);
			}
		}
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
		private Vector2 GetPosition(float phase)
		{
			float num = phase * 0.017453292f;
			Vector2 vector = new Vector2(Mathf.Cos(num) * this.size.x, Mathf.Sin(num) * this.size.y);
			vector = Quaternion.Euler(0f, 0f, this.tilt) * vector;
			vector += this.center;
			return vector;
		}
		private static bool IsFront(float phase)
		{
			return phase - (float)(Mathf.FloorToInt(phase / 360f) * 360) > 180f;
		}
		[SerializeField]
		private Transform effectBack;
		[SerializeField]
		private Transform effectFront;
		[SerializeField]
		private Vector2 center;
		[SerializeField]
		private Vector2 size;
		[SerializeField]
		private float tilt;
		[SerializeField]
		private float speed;
		[SerializeField]
		private LogoOrbWidget orbTemplate;
		[SerializeField]
		private List<Sprite> orbSprites;
		[SerializeField]
		private List<Sprite> orbBorderSprites;
		[SerializeField]
		private List<GameObject> orbParticles;
		private float _phase;
		private readonly bool[] _front = new bool[5];
		private readonly List<Transform> orbImageTransList = new List<Transform>();
		private readonly List<Transform> borderTransList = new List<Transform>();
	}
}
