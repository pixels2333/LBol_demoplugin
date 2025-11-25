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
	public class MusicRoomPanel : UiPanel, IInputActionHandler
	{
		private int CurrentWidgetIndex
		{
			get
			{
				return this._musicWidgets.FindIndex((MusicWidget widget) => widget.IsSelect);
			}
		}
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
		void IInputActionHandler.OnCancel()
		{
			this.returnButton.onClick.Invoke();
		}
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
		[SerializeField]
		private Button returnButton;
		[SerializeField]
		private TextMeshProUGUI commentText;
		[SerializeField]
		private TextMeshProUGUI originText;
		[SerializeField]
		private TextMeshProUGUI artistText;
		[SerializeField]
		private TextMeshProUGUI usesText;
		[SerializeField]
		private CanvasGroup dialogGroup;
		[SerializeField]
		private MusicWidget musicTemplate;
		[SerializeField]
		private Transform musicRoot;
		[SerializeField]
		private Transform discImage;
		[SerializeField]
		private Transform armParent;
		[SerializeField]
		private Toggle playToggle;
		[SerializeField]
		private Toggle loopToggle;
		[SerializeField]
		private Image loopImage;
		[SerializeField]
		private List<AudioSource> audioSourceList;
		private readonly List<MusicWidget> _musicWidgets = new List<MusicWidget>();
		private int _currentAudioIndex;
		private int _loopAudioIndex;
		private bool _isLoading;
		private CanvasGroup _canvasGroup;
		private float _loopStart;
		private float _loopEnd;
		private float _nextLoop;
		private float _currentLoop;
		private double _tempPauseStartTime;
		private double _tempPauseEndTime;
		[Header("Spectrum Area")]
		[SerializeField]
		private RectTransform spectrumArea;
		[SerializeField]
		private Color barColor = Color.white;
		[SerializeField]
		private Color capColor = Color.white;
		[Range(0f, 2f)]
		[SerializeField]
		private float barDropSpeed = 1f;
		[SerializeField]
		private float barUpdateInterval = 0.02f;
		[SerializeField]
		private FFTWindow windowFunction = FFTWindow.Blackman;
		private const int Size = 8;
		private const int SpectrumSize = 128;
		private readonly float[] _spectrumData = new float[128];
		private readonly float[] _barValues = new float[8];
		private readonly float[] _bufferedBarValues = new float[8];
		private readonly RectTransform[] _bars = new RectTransform[8];
		private readonly RectTransform[] _caps = new RectTransform[8];
		private float _barUpdateTimeout;
	}
}
