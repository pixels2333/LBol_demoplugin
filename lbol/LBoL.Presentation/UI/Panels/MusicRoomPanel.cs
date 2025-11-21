using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x020000A5 RID: 165
	public class MusicRoomPanel : UiPanel, IInputActionHandler
	{
		// Token: 0x1700016D RID: 365
		// (get) Token: 0x060008D8 RID: 2264 RVA: 0x0002C7AD File Offset: 0x0002A9AD
		private int CurrentWidgetIndex
		{
			get
			{
				return this._musicWidgets.FindIndex((MusicWidget widget) => widget.IsSelect);
			}
		}

		// Token: 0x060008D9 RID: 2265 RVA: 0x0002C7DC File Offset: 0x0002A9DC
		private void Awake()
		{
			this.musicRoot.DestroyChildren();
			foreach (ValueTuple<int, BgmConfig> valueTuple in BgmConfig.AllConfig().WithIndices<BgmConfig>())
			{
				BgmConfig item = valueTuple.Item2;
				MusicWidget musicWidget = Object.Instantiate<MusicWidget>(this.musicTemplate, this.musicRoot);
				musicWidget.SetMusic(item, item.No);
				this._musicWidgets.Add(musicWidget);
			}
			this.returnButton.onClick.AddListener(new UnityAction(base.Hide));
			this.playToggle.onValueChanged.AddListener(delegate(bool isOn)
			{
				if (!isOn)
				{
					this.audioSourceList[this._currentAudioIndex].Play();
					this.armParent.DOKill(false);
					this.armParent.DOLocalRotate(new Vector3(0f, 0f, -25f), 0.4f, RotateMode.Fast).SetEase(Ease.Linear).SetLink(base.gameObject)
						.SetUpdate(true);
					this._tempPauseEndTime = AudioSettings.dspTime;
					this._currentLoop += (float)(this._tempPauseEndTime - this._tempPauseStartTime);
					this._nextLoop += (float)(this._tempPauseEndTime - this._tempPauseStartTime);
					if (this.loopToggle.isOn && this._loopAudioIndex != this._currentAudioIndex)
					{
						this.audioSourceList[1 - this._currentAudioIndex].time = this._loopStart;
						this.audioSourceList[1 - this._currentAudioIndex].PlayScheduled((double)this._nextLoop);
						this.audioSourceList[1 - this._currentAudioIndex].SetScheduledEndTime((double)(this._nextLoop + (this._loopEnd - this._loopStart)));
						this._loopAudioIndex = 1 - this._currentAudioIndex;
						return;
					}
				}
				else
				{
					this.audioSourceList[0].Pause();
					this.audioSourceList[1].Pause();
					this.armParent.DOKill(false);
					this.armParent.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.4f, RotateMode.Fast).SetEase(Ease.Linear).SetLink(base.gameObject)
						.SetUpdate(true);
					this._tempPauseStartTime = AudioSettings.dspTime;
				}
			});
			this.loopToggle.onValueChanged.AddListener(delegate(bool isOn)
			{
				if (isOn)
				{
					this.loopImage.transform.DOLocalRotate(new Vector3(0f, 0f, -360f), 5f, RotateMode.FastBeyond360).SetEase(Ease.Linear).From(Vector3.zero, true, false)
						.SetLoops(-1)
						.SetUpdate(true)
						.SetLink(base.gameObject);
					return;
				}
				Transform transform = this.loopImage.transform;
				transform.DOKill(false);
				transform.localEulerAngles = Vector3.zero;
				if (this._loopAudioIndex != this._currentAudioIndex)
				{
					this.audioSourceList[this._loopAudioIndex].Stop();
					this.audioSourceList[this._loopAudioIndex].time = 0f;
					this._loopAudioIndex = this._currentAudioIndex;
				}
				float num = this.audioSourceList[this._currentAudioIndex].clip.length - this.audioSourceList[this._currentAudioIndex].time;
				this.audioSourceList[this._currentAudioIndex].SetScheduledEndTime(AudioSettings.dspTime + (double)num);
			});
			this.InitSpectrumArea();
			this.StopMusic();
			this._canvasGroup = base.GetComponent<CanvasGroup>();
		}

		// Token: 0x060008DA RID: 2266 RVA: 0x0002C8D0 File Offset: 0x0002AAD0
		protected override void OnShowing()
		{
			this.audioSourceList[0].outputAudioMixerGroup = AudioManager.BgmGroup;
			this.audioSourceList[1].outputAudioMixerGroup = AudioManager.BgmGroup;
			this._currentAudioIndex = 0;
			this.dialogGroup.alpha = 0f;
			this._tempPauseStartTime = 0.0;
			this._tempPauseEndTime = 0.0;
			this.loopToggle.SetIsOnWithoutNotify(false);
			this.playToggle.SetIsOnWithoutNotify(true);
			this.playToggle.interactable = false;
			this.loopToggle.interactable = false;
			AudioManager.EnterMusicRoomFadeOutBgm();
			GameMaster.PlatformHandler.SetMainMenuInfo(MainMenuStatus.Music);
			this._canvasGroup.interactable = true;
			UiManager.PushActionHandler(this);
		}

		// Token: 0x060008DB RID: 2267 RVA: 0x0002C990 File Offset: 0x0002AB90
		protected override void OnHiding()
		{
			AudioManager.LeaveMusicRoomFadeInBgm();
			this.audioSourceList[0].Stop();
			this.audioSourceList[1].Stop();
			foreach (MusicWidget musicWidget in this._musicWidgets)
			{
				musicWidget.IsSelect = false;
			}
			this.StopMusic();
			GameMaster.PlatformHandler.SetMainMenuInfo(MainMenuStatus.Idle);
			this._canvasGroup.interactable = false;
			UiManager.PopActionHandler(this);
		}

		// Token: 0x060008DC RID: 2268 RVA: 0x0002CA2C File Offset: 0x0002AC2C
		void IInputActionHandler.OnCancel()
		{
			this.returnButton.onClick.Invoke();
		}

		// Token: 0x060008DD RID: 2269 RVA: 0x0002CA40 File Offset: 0x0002AC40
		public async UniTask PlayMusic(BgmConfig config)
		{
			foreach (MusicWidget musicWidget in this._musicWidgets)
			{
				musicWidget.Interactable = false;
				musicWidget.IsSelect = false;
			}
			this.armParent.DOKill(false);
			this.armParent.DOLocalRotate(new Vector3(0f, 0f, -25f), 0.4f, RotateMode.Fast).SetEase(Ease.Linear).SetLink(base.gameObject)
				.SetUpdate(true);
			string path = config.Folder + "/" + config.Path;
			try
			{
				this._isLoading = true;
				this.discImage.localRotation = Quaternion.identity;
				AudioClip audioClip = await ResourcesHelper.LoadBgmAsync(path);
				this.audioSourceList[0].time = 0f;
				this.audioSourceList[0].clip = audioClip;
				this.audioSourceList[0].Play();
				this.audioSourceList[1].clip = audioClip;
				this.audioSourceList[0].volume = config.Volume;
				this.audioSourceList[1].volume = config.Volume;
				this.playToggle.isOn = false;
				this.playToggle.interactable = true;
				this.loopToggle.interactable = true;
				this._currentAudioIndex = 0;
				this._loopAudioIndex = 0;
				this.dialogGroup.DOKill(false);
				TweenerCore<float, float, FloatOptions> tweenerCore = this.dialogGroup.DOFade(0f, 0.4f);
				tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, delegate
				{
					this.commentText.text = config.Comment;
					this.artistText.text = config.Artist;
					this.originText.text = "TopInfo.Original".Localize(true) + config.Original;
					this.usesText.text = config.Name;
				});
				this.dialogGroup.DOFade(1f, 0.4f).SetDelay(0.4f);
				float? num = config.LoopStart;
				if (num != null)
				{
					float s = num.GetValueOrDefault();
					this._loopStart = s;
				}
				else
				{
					this._loopStart = 0f;
				}
				num = config.LoopEnd;
				if (num != null)
				{
					float e = num.GetValueOrDefault();
					this._loopEnd = e;
				}
				else
				{
					this._loopEnd = audioClip.length;
				}
				this._nextLoop = (float)AudioSettings.dspTime + this._loopEnd;
				this._currentLoop = this._nextLoop;
				this._isLoading = false;
			}
			catch (Exception)
			{
				Debug.LogError(string.Concat(new string[] { "Cannot play bgm ", config.ID, " at '", path, "'" }));
			}
			foreach (MusicWidget musicWidget2 in this._musicWidgets)
			{
				musicWidget2.Interactable = true;
			}
		}

		// Token: 0x060008DE RID: 2270 RVA: 0x0002CA8C File Offset: 0x0002AC8C
		private void PrepareLoop()
		{
			float time = this.audioSourceList[this._loopAudioIndex].time;
			this.audioSourceList[this._loopAudioIndex].SetScheduledEndTime(AudioSettings.dspTime + (double)this._loopEnd - (double)time);
			int num = 1 - this._loopAudioIndex;
			this.audioSourceList[num].time = this._loopStart;
			this.audioSourceList[num].PlayScheduled((double)this._nextLoop);
			this.audioSourceList[num].SetScheduledEndTime((double)(this._nextLoop + (this._loopEnd - this._loopStart)));
			this._nextLoop += this._loopEnd - this._loopStart;
			this._loopAudioIndex = num;
		}

		// Token: 0x060008DF RID: 2271 RVA: 0x0002CB58 File Offset: 0x0002AD58
		public void StopMusic()
		{
			this.dialogGroup.DOFade(0f, 0.4f);
			this.audioSourceList[0].Stop();
			this.audioSourceList[0].clip = null;
			this.audioSourceList[1].Stop();
			this.audioSourceList[1].clip = null;
			this.playToggle.SetIsOnWithoutNotify(true);
			this.playToggle.interactable = false;
			Transform transform = this.loopImage.transform;
			transform.DOKill(false);
			transform.localEulerAngles = Vector3.zero;
			this.loopToggle.SetIsOnWithoutNotify(false);
			this.armParent.DOKill(false);
			this.armParent.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.4f, RotateMode.Fast).SetEase(Ease.Linear).SetLink(base.gameObject)
				.SetUpdate(true);
		}

		// Token: 0x060008E0 RID: 2272 RVA: 0x0002CC4C File Offset: 0x0002AE4C
		private void InitSpectrumArea()
		{
			Vector2 sizeDelta = this.spectrumArea.sizeDelta;
			float x = sizeDelta.x;
			float y = sizeDelta.y;
			float num = y * 0.01f;
			float num2 = x / 8f;
			float num3 = num2 * 0.8f;
			float num4 = (num2 - num3) / 2f;
			for (int i = 0; i < 8; i++)
			{
				GameObject gameObject = new GameObject(string.Format("Bar {0:00}", i), new Type[] { typeof(RectTransform) });
				RectTransform rectTransform = (RectTransform)gameObject.transform;
				rectTransform.SetParent(this.spectrumArea, false);
				rectTransform.pivot = Vector2.zero;
				rectTransform.anchorMin = Vector2.zero;
				rectTransform.anchorMax = Vector2.zero;
				rectTransform.anchoredPosition = new Vector2(num4, 0f);
				rectTransform.sizeDelta = new Vector2(num3, y);
				gameObject.AddComponent<RawImage>().color = this.barColor;
				this._bars[i] = rectTransform;
				GameObject gameObject2 = new GameObject(string.Format("Cap {0:00}", i), new Type[] { typeof(RectTransform) });
				RectTransform rectTransform2 = (RectTransform)gameObject2.transform;
				rectTransform2.SetParent(this.spectrumArea, false);
				rectTransform2.pivot = Vector2.zero;
				rectTransform2.anchorMin = Vector2.zero;
				rectTransform2.anchorMax = Vector2.zero;
				rectTransform2.anchoredPosition = new Vector2(num4, 0f);
				rectTransform2.sizeDelta = new Vector2(num3, num);
				gameObject2.AddComponent<RawImage>().color = this.capColor;
				this._caps[i] = rectTransform2;
				num4 += num2;
			}
		}

		// Token: 0x060008E1 RID: 2273 RVA: 0x0002CDFC File Offset: 0x0002AFFC
		private void UpdateSpectrumArea()
		{
			this._barUpdateTimeout -= Time.unscaledDeltaTime;
			if (this._barUpdateTimeout < 0f)
			{
				this._barUpdateTimeout = this.barUpdateInterval;
				int num = 1;
				for (int i = 0; i < 8; i++)
				{
					int num2 = num / 2;
					this._barValues[i] = 0f;
					for (int j = num2; j < num; j++)
					{
						this._barValues[i] += this._spectrumData[j];
					}
					this._barValues[i] = Math.Min(this._barValues[i], 1f);
					num *= 2;
				}
			}
			for (int k = 0; k < 8; k++)
			{
				if (this._bufferedBarValues[k] < this._barValues[k])
				{
					this._bufferedBarValues[k] = this._barValues[k];
				}
				else
				{
					this._bufferedBarValues[k] = Mathf.MoveTowards(this._bufferedBarValues[k], this._barValues[k], this.barDropSpeed * Time.deltaTime);
				}
			}
			float y = this.spectrumArea.sizeDelta.y;
			for (int l = 0; l < 8; l++)
			{
				RectTransform rectTransform = this._bars[l];
				Vector2 sizeDelta = rectTransform.sizeDelta;
				sizeDelta.y = this._barValues[l] * y;
				rectTransform.sizeDelta = sizeDelta;
				RectTransform rectTransform2 = this._caps[l];
				Vector2 anchoredPosition = rectTransform2.anchoredPosition;
				anchoredPosition.y = this._bufferedBarValues[l] * y;
				rectTransform2.anchoredPosition = anchoredPosition;
			}
		}

		// Token: 0x060008E2 RID: 2274 RVA: 0x0002CF6C File Offset: 0x0002B16C
		private void Update()
		{
			if (AudioSettings.dspTime + (double)((this._loopEnd - this._loopStart) / 2f) > (double)this._nextLoop && this.loopToggle.isOn && !this.playToggle.isOn)
			{
				this.PrepareLoop();
			}
			if (AudioSettings.dspTime > (double)this._currentLoop && this.loopToggle.isOn && !this.playToggle.isOn)
			{
				this._currentAudioIndex = 1 - this._currentAudioIndex;
				this._currentLoop += this._loopEnd - this._loopStart;
			}
			if (this.audioSourceList[this._currentAudioIndex].isPlaying)
			{
				float time = this.audioSourceList[this._currentAudioIndex].time;
				float num = this.audioSourceList[this._currentAudioIndex].clip.length;
				if (this.loopToggle.isOn)
				{
					num -= num - this._loopEnd;
				}
				this.discImage.localEulerAngles = new Vector3(0f, 0f, this.audioSourceList[this._currentAudioIndex].time * -10f);
				this.audioSourceList[this._currentAudioIndex].GetSpectrumData(this._spectrumData, 0, this.windowFunction);
			}
			else if (!this.loopToggle.isOn && !this.playToggle.isOn && this.CurrentWidgetIndex != -1 && !this._isLoading)
			{
				if (this.CurrentWidgetIndex == this._musicWidgets.Count - 1)
				{
					this._musicWidgets[0].PlayThis();
				}
				else
				{
					this._musicWidgets[this.CurrentWidgetIndex + 1].PlayThis();
				}
			}
			else
			{
				Array.Clear(this._spectrumData, 0, this._spectrumData.Length);
			}
			this.UpdateSpectrumArea();
		}

		// Token: 0x04000680 RID: 1664
		[SerializeField]
		private Button returnButton;

		// Token: 0x04000681 RID: 1665
		[SerializeField]
		private TextMeshProUGUI commentText;

		// Token: 0x04000682 RID: 1666
		[SerializeField]
		private TextMeshProUGUI originText;

		// Token: 0x04000683 RID: 1667
		[SerializeField]
		private TextMeshProUGUI artistText;

		// Token: 0x04000684 RID: 1668
		[SerializeField]
		private TextMeshProUGUI usesText;

		// Token: 0x04000685 RID: 1669
		[SerializeField]
		private CanvasGroup dialogGroup;

		// Token: 0x04000686 RID: 1670
		[SerializeField]
		private MusicWidget musicTemplate;

		// Token: 0x04000687 RID: 1671
		[SerializeField]
		private Transform musicRoot;

		// Token: 0x04000688 RID: 1672
		[SerializeField]
		private Transform discImage;

		// Token: 0x04000689 RID: 1673
		[SerializeField]
		private Transform armParent;

		// Token: 0x0400068A RID: 1674
		[SerializeField]
		private Toggle playToggle;

		// Token: 0x0400068B RID: 1675
		[SerializeField]
		private Toggle loopToggle;

		// Token: 0x0400068C RID: 1676
		[SerializeField]
		private Image loopImage;

		// Token: 0x0400068D RID: 1677
		[SerializeField]
		private List<AudioSource> audioSourceList;

		// Token: 0x0400068E RID: 1678
		private readonly List<MusicWidget> _musicWidgets = new List<MusicWidget>();

		// Token: 0x0400068F RID: 1679
		private int _currentAudioIndex;

		// Token: 0x04000690 RID: 1680
		private int _loopAudioIndex;

		// Token: 0x04000691 RID: 1681
		private bool _isLoading;

		// Token: 0x04000692 RID: 1682
		private CanvasGroup _canvasGroup;

		// Token: 0x04000693 RID: 1683
		private float _loopStart;

		// Token: 0x04000694 RID: 1684
		private float _loopEnd;

		// Token: 0x04000695 RID: 1685
		private float _nextLoop;

		// Token: 0x04000696 RID: 1686
		private float _currentLoop;

		// Token: 0x04000697 RID: 1687
		private double _tempPauseStartTime;

		// Token: 0x04000698 RID: 1688
		private double _tempPauseEndTime;

		// Token: 0x04000699 RID: 1689
		[Header("Spectrum Area")]
		[SerializeField]
		private RectTransform spectrumArea;

		// Token: 0x0400069A RID: 1690
		[SerializeField]
		private Color barColor = Color.white;

		// Token: 0x0400069B RID: 1691
		[SerializeField]
		private Color capColor = Color.white;

		// Token: 0x0400069C RID: 1692
		[Range(0f, 2f)]
		[SerializeField]
		private float barDropSpeed = 1f;

		// Token: 0x0400069D RID: 1693
		[SerializeField]
		private float barUpdateInterval = 0.02f;

		// Token: 0x0400069E RID: 1694
		[SerializeField]
		private FFTWindow windowFunction = FFTWindow.Blackman;

		// Token: 0x0400069F RID: 1695
		private const int Size = 8;

		// Token: 0x040006A0 RID: 1696
		private const int SpectrumSize = 128;

		// Token: 0x040006A1 RID: 1697
		private readonly float[] _spectrumData = new float[128];

		// Token: 0x040006A2 RID: 1698
		private readonly float[] _barValues = new float[8];

		// Token: 0x040006A3 RID: 1699
		private readonly float[] _bufferedBarValues = new float[8];

		// Token: 0x040006A4 RID: 1700
		private readonly RectTransform[] _bars = new RectTransform[8];

		// Token: 0x040006A5 RID: 1701
		private readonly RectTransform[] _caps = new RectTransform[8];

		// Token: 0x040006A6 RID: 1702
		private float _barUpdateTimeout;
	}
}
